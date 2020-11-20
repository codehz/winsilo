﻿//  Copyright 2016 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace NtApiDotNet.Win32 {
  [StructLayout(LayoutKind.Sequential)]
  struct SiObjectInfo {
    public SiObjectInfoFlags dwFlags;
    public IntPtr hInstance;
    public IntPtr pszServerName;
    public IntPtr pszObjectName;
    public IntPtr pszPageTitle;
    public Guid guidObjectType;
  }

  enum SiObjectInfoFlags : uint {
    SI_EDIT_PERMS = 0x00000000, // always implied
    SI_EDIT_OWNER = 0x00000001,
    SI_EDIT_AUDITS = 0x00000002,
    SI_CONTAINER = 0x00000004,
    SI_READONLY = 0x00000008,
    SI_ADVANCED = 0x00000010,
    SI_RESET = 0x00000020, //equals to SI_RESET_DACL|SI_RESET_SACL|SI_RESET_OWNER
    SI_OWNER_READONLY = 0x00000040,
    SI_EDIT_PROPERTIES = 0x00000080,
    SI_OWNER_RECURSE = 0x00000100,
    SI_NO_ACL_PROTECT = 0x00000200,
    SI_NO_TREE_APPLY = 0x00000400,
    SI_PAGE_TITLE = 0x00000800,
    SI_SERVER_IS_DC = 0x00001000,
    SI_RESET_DACL_TREE = 0x00004000,
    SI_RESET_SACL_TREE = 0x00008000,
    SI_OBJECT_GUID = 0x00010000,
    SI_EDIT_EFFECTIVE = 0x00020000,
    SI_RESET_DACL = 0x00040000,
    SI_RESET_SACL = 0x00080000,
    SI_RESET_OWNER = 0x00100000,
    SI_NO_ADDITIONAL_PERMISSION = 0x00200000,
    SI_VIEW_ONLY = 0x00400000,
    SI_PERMS_ELEVATION_REQUIRED = 0x01000000,
    SI_AUDITS_ELEVATION_REQUIRED = 0x02000000,
    SI_OWNER_ELEVATION_REQUIRED = 0x04000000,
    SI_SCOPE_ELEVATION_REQUIRED = 0x08000000,
    SI_MAY_WRITE = 0x10000000, //not sure if user can write permission
    SI_ENABLE_EDIT_ATTRIBUTE_CONDITION = 0x20000000,
    SI_ENABLE_CENTRAL_POLICY = 0x40000000,
    SI_DISABLE_DENY_ACE = 0x80000000,
    SI_EDIT_ALL = (SI_EDIT_PERMS | SI_EDIT_OWNER | SI_EDIT_AUDITS)
  }

  struct SiAccess {
    public IntPtr pguid; // Guid
    public uint mask;
    public IntPtr pszName;
    public SiAccessFlags dwFlags;
  }

  [Flags]
  enum SiAccessFlags {
    SI_ACCESS_SPECIFIC = 0x00010000,
    SI_ACCESS_GENERAL = 0x00020000,
    SI_ACCESS_CONTAINER = 0x00040000,
    SI_ACCESS_PROPERTY = 0x00080000,
  }

  [Guid("965FC360-16FF-11d0-91CB-00AA00BBB723"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
  interface ISecurityInformation {
    // *** ISecurityInformation methods ***
    void GetObjectInformation(IntPtr pObjectInfo);
    void GetSecurity(SecurityInformation RequestedInformation,
                    out IntPtr ppSecurityDescriptor,
                    [MarshalAs(UnmanagedType.Bool)] bool fDefault);

    void SetSecurity(SecurityInformation SecurityInformation,
                    IntPtr pSecurityDescriptor);

    void GetAccessRights(ref Guid pguidObjectType,
                        SiObjectInfoFlags dwFlags, // SI_EDIT_AUDITS, SI_EDIT_PROPERTIES
                        out IntPtr ppAccess,
                        out uint pcAccesses,
                        out uint piDefaultAccess);

    void MapGeneric(ref Guid pguidObjectType,
                    IntPtr pAceFlags,
                    ref AccessMask pMask);

    void GetInheritTypes(out IntPtr ppInheritTypes,
                        out uint pcInheritTypes);
    void PropertySheetPageCallback(IntPtr hwnd, uint uMsg, int uPage);
  }

  [ClassInterface(ClassInterfaceType.None), ComVisible(true)]
  class SecurityInformationImpl : ISecurityInformation, IDisposable {
    private GenericMapping _mapping;
    private DisposableList<SafeStringBuffer> _names;
    private SafeHGlobalBuffer _access_map; // SI_ACCESS
    private SafeStringBuffer _obj_name;
    private NtObject _handle;
    private byte[] _sd;
    private bool _read_only;

    private SecurityInformationImpl(string obj_name,
        Dictionary<uint, string> names, GenericMapping generic_mapping,
        bool read_only) {
      _mapping = generic_mapping;
      _obj_name = new SafeStringBuffer(obj_name);
      _access_map = new SafeHGlobalBuffer(Marshal.SizeOf(typeof(SiAccess)) * names.Count);
      SiAccess[] sis = new SiAccess[names.Count];
      IntPtr current_ptr = _access_map.DangerousGetHandle();
      _names = new DisposableList<SafeStringBuffer>();
      int i = 0;
      foreach (KeyValuePair<uint, string> pair in names) {
        _names.Add(new SafeStringBuffer(pair.Value));
        SiAccess si = new SiAccess {
          pguid = IntPtr.Zero,
          dwFlags = SiAccessFlags.SI_ACCESS_SPECIFIC | SiAccessFlags.SI_ACCESS_GENERAL,
          mask = pair.Key,
          pszName = _names[i].DangerousGetHandle()
        };
        sis[i] = si;
        i++;
      }
      _access_map.WriteArray(0, sis, 0, names.Count);
      _read_only = read_only;
    }

    public SecurityInformationImpl(string obj_name, NtObject handle,
        Dictionary<uint, string> names, GenericMapping generic_mapping,
        bool read_only) : this(obj_name, names, generic_mapping, read_only) {
      _handle = handle;
    }

    public SecurityInformationImpl(string obj_name, SecurityDescriptor sd,
        Dictionary<uint, string> names, GenericMapping generic_mapping)
        : this(obj_name, names, generic_mapping, true) {
      _sd = sd.ToByteArray();
    }

    public void GetAccessRights(ref Guid pguidObjectType, SiObjectInfoFlags dwFlags, out IntPtr ppAccess, out uint pcAccesses, out uint piDefaultAccess) {
      ppAccess = _access_map.DangerousGetHandle();
      pcAccesses = (uint)_names.Count;
      piDefaultAccess = 0;
    }

    public void GetInheritTypes(out IntPtr ppInheritTypes, out uint pcInheritTypes) {
      ppInheritTypes = IntPtr.Zero;
      pcInheritTypes = 0;
    }

    public void GetObjectInformation(IntPtr pObjectInfo) {
      SiObjectInfo object_info = new SiObjectInfo();
      SiObjectInfoFlags flags = SiObjectInfoFlags.SI_ADVANCED | SiObjectInfoFlags.SI_EDIT_ALL | SiObjectInfoFlags.SI_NO_ADDITIONAL_PERMISSION;
      if (_read_only || !_handle.IsAccessMaskGranted(GenericAccessRights.WriteDac)) {
        flags |= SiObjectInfoFlags.SI_READONLY;
      }

      object_info.dwFlags = flags;
      object_info.pszObjectName = _obj_name.DangerousGetHandle();
      Marshal.StructureToPtr(object_info, pObjectInfo, false);
    }

    public void GetSecurity(SecurityInformation RequestedInformation,
        out IntPtr ppSecurityDescriptor, [MarshalAs(UnmanagedType.Bool)] bool fDefault) {
      byte[] raw_sd = _sd ?? _handle.GetSecurityDescriptorBytes(RequestedInformation);
      IntPtr ret = Win32NativeMethods.LocalAlloc(0, new IntPtr(raw_sd.Length));
      Marshal.Copy(raw_sd, 0, ret, raw_sd.Length);
      ppSecurityDescriptor = ret;
    }

    public void MapGeneric(ref Guid pguidObjectType, IntPtr pAceFlags, ref AccessMask pMask) {
      pMask = _mapping.MapMask(pMask);
    }

    public void PropertySheetPageCallback(IntPtr hwnd, uint uMsg, int uPage) {
      // Do nothing.
    }

    public void SetSecurity(SecurityInformation SecurityInformation, IntPtr pSecurityDescriptor) {
      if (_read_only || _handle == null) {
        throw new SecurityException("Can't edit a read only security descriptor");
      }

      SecurityDescriptor sd = new SecurityDescriptor(pSecurityDescriptor);

      _handle.SetSecurityDescriptor(sd, SecurityInformation);
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing) {
      if (!disposedValue) {
        _names?.Dispose();
        _access_map?.Close();
        _obj_name?.Close();
        _handle?.Close();
        disposedValue = true;
      }
    }

    ~SecurityInformationImpl() {
      Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
