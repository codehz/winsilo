using NtApiDotNet;
using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace winsilo {
  class Program {
    private static void SetupRootDirectory(NtDirectory root) {

      ObjectManager.Apply(root, new ObjectManager.ITask[] {
        new ObjectManager.CloneDirectoryTask("FileSystem"),
        new ObjectManager.CloneDirectoryTask("Windows"),
        new ObjectManager.CloneDirectoryTask("Callback"),
        new ObjectManager.CloneDirectoryTask("Driver"),
        new ObjectManager.CloneDirectoryTask("DriverStore"),
        new ObjectManager.CloneDirectoryTask("RPC Control"),
        new ObjectManager.CloneDirectoryTask("BaseNamedObjects") {
          new ObjectManager.SymlinkTask("FontCachePort", @"\BaseNamedObjects\FontCachePort", scope: ObjectManager.SymlinkScope.Global),
          new ObjectManager.SymlinkTask("FontCachePort3.0.0.0", @"\BaseNamedObjects\FontCachePort3.0.0.0", scope: ObjectManager.SymlinkScope.Global),
          new ObjectManager.SymlinkTask("CoreMessagingRegistrar", @"\BaseNamedObjects\CoreMessagingRegistrar", scope: ObjectManager.SymlinkScope.Global),
          new ObjectManager.SymlinkTask("Local", @"\BaseNamedObjects"),
          new ObjectManager.SymlinkTask("Global", @"\BaseNamedObjects"),
          new ObjectManager.SymlinkTask("Session", @"\Sessions\BNOLINKS"),
          new ObjectManager.SymlinkTask("AppContainerNamedObject", @"\Sessions\0\AppContainerNamedObject"),
          new ObjectManager.CloneDirectoryTask("Restricted")
        },
        new ObjectManager.CloneDirectoryTask("GLOBAL??") {
          new ObjectManager.SymlinkTask("C:", @"\Device\HarddiskVolume4", scope: ObjectManager.SymlinkScope.Global),
          new ObjectManager.SymlinkTask("D:", @"\Device\HarddiskVolume1", scope: ObjectManager.SymlinkScope.Global),
          new ObjectManager.SymlinkTask("X:", @"\Device\HarddiskVolume1\vv", scope: ObjectManager.SymlinkScope.Global),
          new ObjectManager.SymlinkTask("AUX", @"\Device\Null"),
          new ObjectManager.SymlinkTask("CON", @"\Device\ConDrv\Console"),
          new ObjectManager.SymlinkTask("CONIN$", @"\Device\ConDrv\CurrentIn"),
          new ObjectManager.SymlinkTask("CONOUT$", @"\Device\ConDrv\CurrentOut"),
          new ObjectManager.SymlinkTask("NUL", @"\Device\Null"),
          new ObjectManager.SymlinkTask("PIPE", @"\Device\NamedPipe"),
          new ObjectManager.SymlinkTask("Tcp", @"\Device\Tcp"),
        },
      });
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetShellWindow();
    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    private static NtToken FetchDesktopToken() {
      var shell = GetShellWindow();
      _ = GetWindowThreadProcessId(shell, out var shellProcessId);
      using var proc = NtProcess.Open(shellProcessId, ProcessAccessRights.MaximumAllowed, true).GetResultOrThrow();
      return NtToken.OpenProcessToken(proc);
      //using var token = NtToken.OpenProcessToken(proc);
      //return token.DuplicateToken(TokenType.Primary, SecurityImpersonationLevel.Impersonation, TokenAccessRights.MaximumAllowed);
    }

    private static void WaitForDebugger() {
      Console.WriteLine("Waiting for debugger");
      while (!Debugger.IsAttached) {
        Thread.Sleep(100);
      }
    }

    [DllImport("ntdll.dll")]
    private extern static NtStatus RtlConnectToSm(IntPtr a, IntPtr b, IntPtr c, out IntPtr d);
    [DllImport("ntdll.dll")]
    private extern static NtStatus RtlSendMsgToSm(IntPtr a, ref SmData b);
    [StructLayout(LayoutKind.Explicit)]
    private struct SmData {
      [FieldOffset(0x0)]
      public uint u1;
      [FieldOffset(0x4)]
      public uint u2;
      [FieldOffset(0x14)]
      public uint msg;
      [FieldOffset(0x18)]
      public uint cb;
      [FieldOffset(0x28)]
      readonly ushort value;
      [FieldOffset(0x30)]
      readonly IntPtr job;
      [FieldOffset(0x38)]
      readonly ushort unknown;

      public SmData(ushort value, IntPtr job) {
        u1 = 0;
        u2 = 0;
        msg = 0;
        cb = 0;
        this.value = value;
        this.job = job;
        unknown = 0;
      }
    }
    private static void NotifySM(NtJob job, ushort code) {
      var data = new SmData(code, job.Handle.DangerousGetHandle());
      RtlConnectToSm(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out var handle).ToNtException();
      RtlSendMsgToSm(handle, ref data).ToNtException();
      Console.WriteLine(data.u1);
      Console.WriteLine(data.u2);
      Console.WriteLine(data.msg);
      Console.WriteLine(data.cb);
    }

    static void Main() {
      SetTokenPriv.EnablePrivilege();
      //using var _ = new ApplicationPrivilege(new[] {
      //  TokenPrivilegeValue.SeAssignPrimaryTokenPrivilege,
      //  TokenPrivilegeValue.SeTakeOwnershipPrivilege,
      //  TokenPrivilegeValue.SeLoadDriverPrivilege,
      //  TokenPrivilegeValue.SeSecurityPrivilege,
      //  TokenPrivilegeValue.SeTcbPrivilege,
      //  TokenPrivilegeValue.SeBackupPrivilege,
      //  TokenPrivilegeValue.SeRestorePrivilege,
      //});
      //WaitForDebugger();

      using var evt = NtEvent.Create(null, EventType.NotificationEvent, false);
      using var job = NtJob.CreateServerSilo(SiloObjectRootDirectoryControlFlags.All, @"C:\Windows", evt, false);
      using (var root = NtDirectory.Open(job.SiloRootDirectory)) {
        Console.WriteLine(root);
        SetupRootDirectory(root);
      }
      //Debugger.Break();
      //NotifySM(job, 7);

      //ProcessExtensions.GetSessionUserToken(out var tok);
      var config = new NtProcessCreateConfig {
        ImagePath = @"\SystemRoot\System32\cmd.exe",
        ConfigImagePath = @"C:\Windows\System32\cmd.exe",
        CurrentDirectory = @"C:\Windows\System32",
        WindowTitle = "Demo",
        ParentProcess = NtProcess.Current,
        TerminateOnDispose = true,
        ThreadFlags = ThreadCreateFlags.Suspended,
      };
      config.AddAttribute(ProcessAttribute.JobList(new[] { job }));
      using var proc = NtProcess.Create(config);
      proc.Thread.Resume();
      proc.Process.Wait().ToNtException();
      Console.WriteLine($"status: {proc.Process.ExitNtStatus}");
    }
  }
}
