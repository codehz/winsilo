using NtApiDotNet;
using NtApiDotNet.Win32;
using System;
using System.Runtime.InteropServices;

namespace helper {
  class Program {

    [DllImport("user32.dll")]
    public static extern IntPtr GetShellWindow();
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);


    private static NtToken FetchDesktopToken() {
      var shell = GetShellWindow();
      _ = GetWindowThreadProcessId(shell, out var shellProcessId);
      using var proc = NtProcess.Open(shellProcessId, ProcessAccessRights.MaximumAllowed, true).GetResultOrThrow();
      using var token = NtToken.OpenProcessToken(proc);
      return token.DuplicateToken(TokenType.Primary, SecurityImpersonationLevel.Impersonation, TokenAccessRights.MaximumAllowed);
    }
    static void Main() {
      using var token = FetchDesktopToken();
      using var proc = Win32Process.CreateProcessWithToken(token, new Win32ProcessConfig {
        ApplicationName = @"C:\Windows\System32\cmd.exe",
        CommandLine = @"",
        CreationFlags = CreateProcessFlags.NewConsole,
        Token = token,
      });
    }
  }
}
