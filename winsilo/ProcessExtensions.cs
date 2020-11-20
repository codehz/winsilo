using NtApiDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace winsilo {
  public static class ProcessExtensions {
    #region Win32 Constants

    private const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
    private const int CREATE_NO_WINDOW = 0x08000000;
    private const int CREATE_SUSPENDED = 0x00000004;

    private const int CREATE_NEW_CONSOLE = 0x00000010;

    private const uint INVALID_SESSION_ID = 0xFFFFFFFF;
    private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

    #endregion

    #region DllImports

    [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern bool CreateProcessAsUser(
        IntPtr hToken,
        string? lpApplicationName,
        string? lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandle,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string? lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
    private static extern bool DuplicateTokenEx(
        IntPtr ExistingTokenHandle,
        uint dwDesiredAccess,
        IntPtr lpThreadAttributes,
        int TokenType,
        int ImpersonationLevel,
        out IntPtr DuplicateTokenHandle);

    [DllImport("userenv.dll", SetLastError = true)]
    private static extern bool CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

    [DllImport("userenv.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hSnapshot);

    [DllImport("kernel32.dll")]
    private static extern uint WTSGetActiveConsoleSessionId();

    [DllImport("Wtsapi32.dll")]
    private static extern uint WTSQueryUserToken(uint SessionId, out IntPtr phToken);

    [DllImport("wtsapi32.dll", SetLastError = true)]
    private static extern int WTSEnumerateSessions(
        IntPtr hServer,
        int Reserved,
        int Version,
        ref IntPtr ppSessionInfo,
        ref int pCount);

    #endregion

    #region Win32 Structs

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1069", Justification = "<挂起>")]
    private enum SW {
      SW_HIDE = 0,
      SW_SHOWNORMAL = 1,
      SW_NORMAL = 1,
      SW_SHOWMINIMIZED = 2,
      SW_SHOWMAXIMIZED = 3,
      SW_MAXIMIZE = 3,
      SW_SHOWNOACTIVATE = 4,
      SW_SHOW = 5,
      SW_MINIMIZE = 6,
      SW_SHOWMINNOACTIVE = 7,
      SW_SHOWNA = 8,
      SW_RESTORE = 9,
      SW_SHOWDEFAULT = 10,
      SW_MAX = 10
    }

    private enum WTS_CONNECTSTATE_CLASS {
      WTSActive,
      WTSConnected,
      WTSConnectQuery,
      WTSShadow,
      WTSDisconnected,
      WTSIdle,
      WTSListen,
      WTSReset,
      WTSDown,
      WTSInit
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION {
      public IntPtr hProcess;
      public IntPtr hThread;
      public uint dwProcessId;
      public uint dwThreadId;
    }

    private enum SECURITY_IMPERSONATION_LEVEL {
      SecurityAnonymous = 0,
      SecurityIdentification = 1,
      SecurityImpersonation = 2,
      SecurityDelegation = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct STARTUPINFO {
      public int cb;
      public string lpReserved;
      public string lpDesktop;
      public string lpTitle;
      public uint dwX;
      public uint dwY;
      public uint dwXSize;
      public uint dwYSize;
      public uint dwXCountChars;
      public uint dwYCountChars;
      public uint dwFillAttribute;
      public uint dwFlags;
      public short wShowWindow;
      public short cbReserved2;
      public IntPtr lpReserved2;
      public IntPtr hStdInput;
      public IntPtr hStdOutput;
      public IntPtr hStdError;
    }

    private enum TOKEN_TYPE {
      TokenPrimary = 1,
      TokenImpersonation = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WTS_SESSION_INFO {
      public readonly UInt32 SessionID;

      [MarshalAs(UnmanagedType.LPStr)]
      public readonly String pWinStationName;

      public readonly WTS_CONNECTSTATE_CLASS State;
    }

    #endregion

    // Gets the user token from the currently active session
    public static bool GetSessionUserToken(out IntPtr phUserToken) {
      var bResult = false;
      var activeSessionId = INVALID_SESSION_ID;
      var pSessionInfo = IntPtr.Zero;
      var sessionCount = 0;

      // Get a handle to the user access token for the current active session.
      if (WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pSessionInfo, ref sessionCount) != 0) {
        var arrayElementSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
        var current = pSessionInfo;

        for (var i = 0; i < sessionCount; i++) {
#pragma warning disable CS8605
          var si = (WTS_SESSION_INFO)Marshal.PtrToStructure(current, typeof(WTS_SESSION_INFO));
#pragma warning restore CS8605
          current += arrayElementSize;

          if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive) {
            activeSessionId = si.SessionID;
          }
        }
      }

      // If enumerating did not work, fall back to the old method
      if (activeSessionId == INVALID_SESSION_ID) {
        activeSessionId = WTSGetActiveConsoleSessionId();
      }

      if (WTSQueryUserToken(activeSessionId, out var hImpersonationToken) != 0) {
        // Convert the impersonation token to a primary token
        bResult = DuplicateTokenEx(hImpersonationToken, 0, IntPtr.Zero,
            (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, (int)TOKEN_TYPE.TokenPrimary,
            out phUserToken);

        CloseHandle(hImpersonationToken);
      } else {
        phUserToken = IntPtr.Zero;
      }

      return bResult;
    }

    public static bool StartProcessAsCurrentUser(string appPath, NtJob job) {
      var hUserToken = IntPtr.Zero;
      var startInfo = new STARTUPINFO();
      var procInfo = new PROCESS_INFORMATION();
      var pEnv = IntPtr.Zero;
      int iResultOfCreateProcessAsUser;

      startInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));

      try {
        if (!GetSessionUserToken(out hUserToken)) {
          throw new Exception("StartProcessAsCurrentUser: GetSessionUserToken failed.");
        }

        uint dwCreationFlags = CREATE_UNICODE_ENVIRONMENT | CREATE_SUSPENDED | CREATE_NEW_CONSOLE;
        startInfo.wShowWindow = (short)(SW.SW_SHOW);
        startInfo.lpDesktop = @"winsta0\default";

        if (!CreateEnvironmentBlock(ref pEnv, hUserToken, false)) {
          throw new Exception("StartProcessAsCurrentUser: CreateEnvironmentBlock failed.");
        }

        if (!CreateProcessAsUser(hUserToken,
            appPath, // Application Name
            null,
            IntPtr.Zero,
            IntPtr.Zero,
            false,
            dwCreationFlags,
            pEnv,
            null,
            ref startInfo,
            out procInfo)) {
          iResultOfCreateProcessAsUser = Marshal.GetLastWin32Error();
          throw new Exception("StartProcessAsCurrentUser: CreateProcessAsUser failed.  Error Code -" + iResultOfCreateProcessAsUser);
        }

        job.AssignProcess(NtProcess.FromHandle(procInfo.hProcess));
        NtThread.FromHandle(procInfo.hThread).Resume();

        iResultOfCreateProcessAsUser = Marshal.GetLastWin32Error();
      } finally {
        CloseHandle(hUserToken);
        if (pEnv != IntPtr.Zero) {
          DestroyEnvironmentBlock(pEnv);
        }
        CloseHandle(procInfo.hThread);
        CloseHandle(procInfo.hProcess);
      }

      return true;
    }

  }
}
