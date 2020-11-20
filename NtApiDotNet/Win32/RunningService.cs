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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace NtApiDotNet.Win32 {
  /// <summary>
  /// Class representing a running service instance.
  /// </summary>
  public class RunningService {
    private ServiceInformation GetServiceInformation() {
      return ServiceUtils.GetServiceInformation(Name,
          false).GetResultOrDefault(new ServiceInformation(Name));
    }

    private readonly Lazy<ServiceInformation> _service_information;

    /// <summary>
    /// The name of the service.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The description of the service.
    /// </summary>
    public string DisplayName { get; }
    /// <summary>
    /// Type of service.
    /// </summary>
    public ServiceType ServiceType { get; }
    /// <summary>
    /// Image path for the service.
    /// </summary>
    public string ImagePath { get; }
    /// <summary>
    /// Command line for the service.
    /// </summary>
    public string CommandLine { get; }
    /// <summary>
    /// Service DLL if a shared process server.
    /// </summary>
    public string ServiceDll { get; }
    /// <summary>
    /// Current service status.
    /// </summary>
    public ServiceStatus Status { get; }
    /// <summary>
    /// What controls are accepted by the service.
    /// </summary>
    public ServiceControlsAccepted ControlsAccepted { get; }
    /// <summary>
    /// The Win32 exit code.
    /// </summary>
    public Win32Error Win32ExitCode { get; }
    /// <summary>
    /// The service specific exit code, if Win32ExitCode is Win32Error.ERROR_SERVICE_SPECIFIC_ERROR.
    /// </summary>
    public int ServiceSpecificExitCode { get; }
    /// <summary>
    /// The checkpoint while starting.
    /// </summary>
    public int CheckPoint { get; }
    /// <summary>
    /// Waiting hint time.
    /// </summary>
    public int WaitHint { get; }
    /// <summary>
    /// Service flags.
    /// </summary>
    public ServiceFlags ServiceFlags { get; }
    /// <summary>
    /// Process ID of the running service.
    /// </summary>
    public int ProcessId { get; }
    /// <summary>
    /// The security descriptor of the service.
    /// </summary>
    public SecurityDescriptor SecurityDescriptor => _service_information.Value.SecurityDescriptor;
    /// <summary>
    /// The list of triggers for the service.
    /// </summary>
    public IEnumerable<ServiceTriggerInformation> Triggers => _service_information.Value.Triggers;
    /// <summary>
    /// The service SID type.
    /// </summary>
    public ServiceSidType SidType => _service_information.Value.SidType;
    /// <summary>
    /// The service launch protected setting.
    /// </summary>
    public ServiceLaunchProtectedType LaunchProtected => _service_information.Value.LaunchProtected;
    /// <summary>
    /// The service required privileges.
    /// </summary>
    public IEnumerable<string> RequiredPrivileges => _service_information.Value.RequiredPrivileges;
    /// <summary>
    /// Service start type.
    /// </summary>
    public ServiceStartType StartType => _service_information.Value.StartType;
    /// <summary>
    /// Error control.
    /// </summary>
    public ServiceErrorControl ErrorControl => _service_information.Value.ErrorControl;
    /// <summary>
    /// Load order group.
    /// </summary>
    public string LoadOrderGroup => _service_information.Value.LoadOrderGroup;
    /// <summary>
    /// Tag ID for load order.
    /// </summary>
    public int TagId => _service_information.Value.TagId;
    /// <summary>
    /// Dependencies.
    /// </summary>
    public IEnumerable<string> Dependencies => _service_information.Value.Dependencies;
    /// <summary>
    /// The user name this service runs under.
    /// </summary>
    public string UserName { get; }
    /// <summary>
    /// Whether the service can be stopped.
    /// </summary>
    public bool CanStop => ControlsAccepted.HasFlagSet(ServiceControlsAccepted.Stop);
    /// <summary>
    /// Type of service host when using Win32Share.
    /// </summary>
    public string ServiceHostType { get; }

    private static RegistryKey OpenKeySafe(RegistryKey rootKey, string path) {
      try {
        return rootKey.OpenSubKey(path);
      } catch (SecurityException) {
        return null;
      }
    }

    private static string ReadStringFromKey(RegistryKey rootKey, string keyName, string valueName) {
      RegistryKey key = rootKey;

      try {
        if (keyName != null) {
          key = OpenKeySafe(rootKey, keyName);
        }

        string valueString = string.Empty;
        if (key != null) {
          object valueObject = key.GetValue(valueName);
          if (valueObject != null) {
            valueString = valueObject.ToString();
          }
        }

        return valueString.TrimEnd('\0');
      } finally {
        if (key != null && key != rootKey) {
          key.Close();
        }
      }
    }

    private static string GetString(IntPtr ptr) {
      if (ptr == IntPtr.Zero) {
        return string.Empty;
      }
      return Marshal.PtrToStringUni(ptr);
    }

    internal RunningService(string name, string display_name, SERVICE_STATUS_PROCESS status) {
      Name = name;
      DisplayName = display_name;
      ServiceType = status.dwServiceType;
      Status = status.dwCurrentState;
      ProcessId = status.dwProcessId;
      ControlsAccepted = status.dwControlsAccepted;
      Win32ExitCode = status.dwWin32ExitCode;
      ServiceSpecificExitCode = status.dwServiceSpecificExitCode;
      CheckPoint = status.dwCheckPoint;
      WaitHint = status.dwWaitHint;
      ServiceFlags = status.dwServiceFlags;
      ServiceDll = string.Empty;
      ImagePath = string.Empty;
      CommandLine = string.Empty;
      ServiceHostType = string.Empty;

      using (RegistryKey key = OpenKeySafe(Registry.LocalMachine, $@"SYSTEM\CurrentControlSet\Services\{Name}")) {
        if (key != null) {
          CommandLine = ReadStringFromKey(key, null, "ImagePath");
          ImagePath = Win32Utils.GetImagePathFromCommandLine(CommandLine);
          string[] args = Win32Utils.ParseCommandLine(CommandLine);
          if (ServiceType.HasFlagSet(ServiceType.Win32ShareProcess)) {
            for (int i = 0; i < args.Length - 1; ++i) {
              if (args[i] == "-k") {
                ServiceHostType = args[i + 1];
                break;
              }
            }
          }
          ServiceDll = ReadStringFromKey(key, "Parameters", "ServiceDll");

          if (string.IsNullOrEmpty(ServiceDll)) {
            ServiceDll = ReadStringFromKey(key, null, "ServiceDll");
          }
          UserName = ReadStringFromKey(key, null, "ObjectName");
        }
      }
      _service_information = new Lazy<ServiceInformation>(GetServiceInformation);
    }

    internal RunningService(ENUM_SERVICE_STATUS_PROCESS process)
        : this(GetString(process.lpServiceName), GetString(process.lpDisplayName),
              process.ServiceStatusProcess) {
    }
  }
#pragma warning restore
}
