﻿//  Copyright 2016, 2017 Google Inc. All Rights Reserved.
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

using System.Collections.Generic;

namespace NtApiDotNet.Win32 {
  /// <summary>
  /// Class representing the information about a service.
  /// </summary>
  public class ServiceInformation {
    /// <summary>
    /// The name of the service.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The security descriptor of the service.
    /// </summary>
    public SecurityDescriptor SecurityDescriptor { get; }
    /// <summary>
    /// The list of triggers for the service.
    /// </summary>
    public IEnumerable<ServiceTriggerInformation> Triggers { get; }
    /// <summary>
    /// The service SID setting.
    /// </summary>
    public ServiceSidType SidType { get; }
    /// <summary>
    /// The service launch protected setting.
    /// </summary>
    public ServiceLaunchProtectedType LaunchProtected { get; }
    /// <summary>
    /// The service required privileges.
    /// </summary>
    public IEnumerable<string> RequiredPrivileges { get; }
    /// <summary>
    /// The service type.
    /// </summary>
    public ServiceType ServiceType { get; }
    /// <summary>
    /// Service start type.
    /// </summary>
    public ServiceStartType StartType { get; }
    /// <summary>
    /// Error control.
    /// </summary>
    public ServiceErrorControl ErrorControl { get; }
    /// <summary>
    /// Binary path name.
    /// </summary>
    public string BinaryPathName { get; }
    /// <summary>
    /// Load order group.
    /// </summary>
    public string LoadOrderGroup { get; }
    /// <summary>
    /// Tag ID for load order.
    /// </summary>
    public int TagId { get; }
    /// <summary>
    /// Dependencies.
    /// </summary>
    public IEnumerable<string> Dependencies { get; }
    /// <summary>
    /// Display name.
    /// </summary>
    public string DisplayName { get; }
    /// <summary>
    /// Service start name. For user mode services this is the username, for drivers it's the driver name.
    /// </summary>
    public string ServiceStartName { get; }

    internal ServiceInformation(string name, SecurityDescriptor sd,
        IEnumerable<ServiceTriggerInformation> triggers, ServiceSidType sid_type,
        ServiceLaunchProtectedType launch_protected, IEnumerable<string> required_privileges,
        SafeStructureInOutBuffer<QUERY_SERVICE_CONFIG> config) {
      using (config) {
        Name = name;
        SecurityDescriptor = sd;
        Triggers = triggers;
        SidType = sid_type;
        LaunchProtected = launch_protected;
        RequiredPrivileges = required_privileges;

        if (config == null) {
          BinaryPathName = string.Empty;
          LoadOrderGroup = string.Empty;
          Dependencies = new string[0];
          DisplayName = string.Empty;
          ServiceStartName = string.Empty;
          return;
        }

        var result = config.Result;
        ServiceType = result.dwServiceType;
        StartType = result.dwStartType;
        ErrorControl = result.dwErrorControl;
        BinaryPathName = result.lpBinaryPathName.GetString();
        LoadOrderGroup = result.lpLoadOrderGroup.GetString();
        TagId = result.dwTagId;
        Dependencies = result.lpLoadOrderGroup.GetMultiString();
        DisplayName = result.lpDisplayName.GetString();
        ServiceStartName = result.lpServiceStartName.GetString();
      }
    }

    internal ServiceInformation(string name) : this(name, null,
            new ServiceTriggerInformation[0], ServiceSidType.None,
            ServiceLaunchProtectedType.None, new string[0], null) {
    }
  }
#pragma warning restore
}
