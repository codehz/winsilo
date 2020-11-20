using System;
using System.Collections;
using System.Collections.Generic;
using NtApiDotNet;

namespace winsilo {
  public static class ObjectManager {
    public interface ITask {
      void Build(NtDirectory root, NtDirectory siloroot);
    }

    public enum SymlinkScope {
      Global,
      Local,
    }

    public class SymlinkTask : ITask {
      private string Name { get; init; }
      private string Destination { get; init; }
      private SymlinkScope Scope { get; init; }
      private AccessMask? AccessMask { get; init; }

      public SymlinkTask(string name, string dest, SymlinkScope scope = SymlinkScope.Local, AccessMask? accessMask = null) {
        Name = name;
        Destination = dest;
        Scope = scope;
        AccessMask = accessMask;
      }

      static readonly Lazy<SecurityDescriptor> BaseSecurityDescriptor = new Lazy<SecurityDescriptor>(() => {
        using var rr = NtDirectory.Open(@"\");
        return rr.SecurityDescriptor;
      });

      public void Build(NtDirectory root, NtDirectory siloroot) {
        var security = BaseSecurityDescriptor.Value;
        using var attr = new ObjectAttributes(
          object_name: Name,
          attributes: AttributeFlags.Permanent | AttributeFlags.CaseInsensitive,
          root: siloroot,
          sqos: null,
          security_descriptor: security
         );
        using var link = NtSymbolicLink.Create(attr, (SymbolicLinkAccessRights)0xFFFFF, Destination);
        if (Scope is SymlinkScope.Global) link.SetGlobalLink();
        if (AccessMask is AccessMask mask) link.SetAccessMask(mask);
      }
    }

    public class CloneDirectoryTask : ITask, IEnumerable<ITask> {
      private string RelativePath { get; init; }
      private string? OriginalRelativePath { get; init; }
      private bool Shadow { get; init; }
      private readonly List<ITask> SubTasks = new List<ITask>();
      public IEnumerator<ITask> GetEnumerator() => SubTasks.GetEnumerator();

      public CloneDirectoryTask(string relativePath, string? original = null, bool shadow = false) {
        RelativePath = relativePath;
        OriginalRelativePath = original;
        Shadow = shadow;
      }

      public void Build(NtDirectory root, NtDirectory siloroot) {
        using var realattr = new ObjectAttributes(
          OriginalRelativePath ?? RelativePath,
          AttributeFlags.CaseInsensitive,
          root
        );
        using var real = NtDirectory.Open(realattr, DirectoryAccessRights.MaximumAllowed);
        using var fakeattr = new ObjectAttributes(
          object_name: RelativePath,
          attributes: AttributeFlags.Permanent | AttributeFlags.Inherit | AttributeFlags.CaseInsensitive | AttributeFlags.OpenIf,
          root: siloroot,
          sqos: null,
          security_descriptor: real.SecurityDescriptor
        );
        using var silo = NtDirectory.Create(fakeattr, DirectoryAccessRights.MaximumAllowed, Shadow ? real : null);
        foreach (var task in SubTasks) {
          task.Build(real, silo);
        }
      }
      IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)SubTasks).GetEnumerator();

      public void Add(ITask task) => SubTasks.Add(task);
    }

    public static void Apply(NtDirectory siloroot, IEnumerable<ITask> tasks) {
      using var root = NtDirectory.Open(@"\");
      foreach (var task in tasks) {
        task.Build(root, siloroot);
      }
    }
  }
}
