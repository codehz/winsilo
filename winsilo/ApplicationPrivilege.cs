using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NtApiDotNet;

namespace winsilo {
  class ApplicationPrivilege : IDisposable {

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern NtStatus RtlAdjustPrivilege(TokenPrivilegeValue Privilege, bool bEnablePrivilege, bool IsThreadPrivilege, out bool PreviousValue);
    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern NtStatus RtlImpersonateSelf(SecurityImpersonationLevel securityImpersonationLevel);
    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool RevertToSelf();

    public ApplicationPrivilege(IEnumerable<TokenPrivilegeValue> lists) {
      RtlImpersonateSelf(SecurityImpersonationLevel.Impersonation).ToNtException();
      foreach (var privilege in lists) {
        RtlAdjustPrivilege(privilege, true, true, out var _).ToNtException();
      }
    }
    public void Dispose() {
      RevertToSelf();
    }
  }
}
