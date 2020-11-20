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

namespace NtApiDotNet {
  /// <summary>
  /// Class to create a new user process using the native APIs.
  /// </summary>
  [Obsolete("Use NtProcessCreateConfig")]
  public sealed class CreateUserProcess {
    /// <summary>
    /// Path to the executable to start.
    /// </summary>
    public string ImagePath { get; set; }

    /// <summary>
    /// Path to the executable to start which is passed in the process configuration.
    /// </summary>
    public string ConfigImagePath {
      get; set;
    }

    /// <summary>
    /// Command line
    /// </summary>
    public string CommandLine { get; set; }

    /// <summary>
    /// Prepared environment block.
    /// </summary>
    public byte[] Environment {
      get; set;
    }

    /// <summary>
    /// Title of the main window.
    /// </summary>
    public string WindowTitle { get; set; }

    /// <summary>
    /// Path to DLLs.
    /// </summary>
    public string DllPath { get; set; }

    /// <summary>
    /// Current directory for new process
    /// </summary>
    public string CurrentDirectory { get; set; }

    /// <summary>
    /// Desktop information value
    /// </summary>
    public string DesktopInfo { get; set; }

    /// <summary>
    /// Shell information value
    /// </summary>
    public string ShellInfo {
      get; set;
    }

    /// <summary>
    /// Runtime data.
    /// </summary>
    public string RuntimeData {
      get; set;
    }

    /// <summary>
    /// Prohibited image characteristics for new process
    /// </summary>
    public ImageCharacteristics ProhibitedImageCharacteristics {
      get; set;
    }

    /// <summary>
    /// Additional file access for opened executable file.
    /// </summary>
    public FileAccessRights AdditionalFileAccess {
      get; set;
    }

    /// <summary>
    /// Process create flags.
    /// </summary>
    public ProcessCreateFlags ProcessFlags {
      get; set;
    }

    /// <summary>
    /// Thread create flags.
    /// </summary>
    public ThreadCreateFlags ThreadFlags {
      get; set;
    }

    /// <summary>
    /// Initialization flags
    /// </summary>
    public ProcessCreateInitFlag InitFlags {
      get; set;
    }

    /// <summary>
    /// Parent process.
    /// </summary>
    public NtProcess ParentProcess {
      get; set;
    }

    /// <summary>
    /// Restrict new child processes
    /// </summary>
    public bool RestrictChildProcess {
      get; set;
    }

    /// <summary>
    /// Override restrict child process
    /// </summary>
    public bool OverrideRestrictChildProcess {
      get; set;
    }

    /// <summary>
    /// Extra process/thread attributes
    /// </summary>
    public List<ProcessAttribute> AdditionalAttributes {
      get; private set;
    }

    /// <summary>
    /// Added protected process protection level.
    /// </summary>
    /// <param name="type">The type of protected process.</param>
    /// <param name="signer">The signer level.</param>
    public void AddProtectionLevel(PsProtectedType type, PsProtectedSigner signer) {
      AdditionalAttributes.Add(ProcessAttribute.ProtectionLevel(type, signer, false));
    }

    /// <summary>
    /// Return on error instead of throwing an exception.
    /// </summary>
    public bool ReturnOnError {
      get; set;
    }

    /// <summary>
    /// Whether to terminate the process on dispose.
    /// </summary>
    public bool TerminateOnDispose {
      get; set;
    }

    /// <summary>
    /// Specify a security descriptor for the process.
    /// </summary>
    public SecurityDescriptor ProcessSecurityDescriptor {
      get; set;
    }

    /// <summary>
    /// Specify a security descriptor for the initial thread.
    /// </summary>
    public SecurityDescriptor ThreadSecurityDescriptor {
      get; set;
    }

    /// <summary>
    /// Specify the primary token for the new process.
    /// </summary>
    public NtToken Token {
      get; set;
    }

    /// <summary>
    /// Access for process handle.
    /// </summary>
    public ProcessAccessRights ProcessDesiredAccess {
      get; set;
    }

    /// <summary>
    /// Access for thread handle.
    /// </summary>
    public ThreadAccessRights ThreadDesiredAccess {
      get; set;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public CreateUserProcess() {
      DesktopInfo = @"WinSta0\Default";
      ShellInfo = "";
      RuntimeData = "";
      WindowTitle = "";
      AdditionalAttributes = new List<ProcessAttribute>();
      ProcessDesiredAccess = ProcessAccessRights.MaximumAllowed;
      ThreadDesiredAccess = ThreadAccessRights.MaximumAllowed;
    }

    /// <summary>
    /// For the current process
    /// </summary>
    /// <returns>The new forked process result</returns>
    public static CreateUserProcessResult Fork() {
      return Fork(ProcessCreateFlags.InheritFromParent,
          ThreadCreateFlags.Suspended, true).Result;
    }

    /// <summary>
    /// For the current process
    /// </summary>
    /// <param name="process_create_flags">Process create flags.</param>
    /// <param name="thread_create_flags">Thread create flags.</param>
    /// <returns>The new forked process result</returns>
    public static CreateUserProcessResult Fork(ProcessCreateFlags process_create_flags,
        ThreadCreateFlags thread_create_flags) {
      return Fork(process_create_flags, thread_create_flags, true).Result;
    }

    /// <summary>
    /// For the current process
    /// </summary>
    /// <param name="process_create_flags">Process create flags.</param>
    /// <param name="thread_create_flags">Thread create flags.</param>
    /// <param name="throw_on_error">True to throw on error.</param>
    /// <returns>The new forked process result</returns>
    public static NtResult<CreateUserProcessResult> Fork(ProcessCreateFlags process_create_flags,
        ThreadCreateFlags thread_create_flags, bool throw_on_error) {
      using (var attrs = new DisposableList<ProcessAttribute>()) {
        ProcessCreateInfo create_info = new ProcessCreateInfo();

        SafeStructureInOutBuffer<ClientId> client_id = new SafeStructureInOutBuffer<ClientId>();
        attrs.Add(ProcessAttribute.ClientId(client_id));

        ProcessAttributeList attr_list = new ProcessAttributeList(attrs);

        return NtSystemCalls.NtCreateUserProcess(
            out SafeKernelObjectHandle process_handle, out SafeKernelObjectHandle thread_handle,
            ProcessAccessRights.MaximumAllowed, ThreadAccessRights.MaximumAllowed,
            null, null, process_create_flags | ProcessCreateFlags.InheritFromParent,
            thread_create_flags, IntPtr.Zero, create_info, attr_list).CreateResult(throw_on_error,
                () => new CreateUserProcessResult(process_handle, thread_handle,
                    create_info.Data, new SectionImageInformation(), client_id.Result, false));
      }
    }

    /// <summary>
    /// Start the new process based on the ImagePath parameter.
    /// </summary>
    /// <returns>The result of the process creation</returns>
    public CreateUserProcessResult Start() {
      return Start(ImagePath);
    }

    /// <summary>
    /// Start the new process
    /// </summary>
    /// <param name="image_path">The image path to the file to execute</param>
    /// <returns>The result of the process creation</returns>
    public CreateUserProcessResult Start(string image_path) {
      if (image_path == null)
        throw new ArgumentNullException("image_path");

      using (var process_params = SafeProcessParametersBuffer.Create(ConfigImagePath ?? image_path, DllPath, CurrentDirectory,
            CommandLine, Environment, WindowTitle, DesktopInfo, ShellInfo, RuntimeData, CreateProcessParametersFlags.Normalize)) {
        using (var attrs = new DisposableList<ProcessAttribute>()) {
          ProcessCreateInfo create_info = new ProcessCreateInfo();

          attrs.Add(ProcessAttribute.ImageName(image_path));
          SafeStructureInOutBuffer<SectionImageInformation> image_info = new SafeStructureInOutBuffer<SectionImageInformation>();
          attrs.Add(ProcessAttribute.ImageInfo(image_info));
          SafeStructureInOutBuffer<ClientId> client_id = new SafeStructureInOutBuffer<ClientId>();
          attrs.Add(ProcessAttribute.ClientId(client_id));
          attrs.AddRange(AdditionalAttributes);
          if (ParentProcess != null) {
            attrs.Add(ProcessAttribute.ParentProcess(ParentProcess.Handle));
          }

          if (RestrictChildProcess || OverrideRestrictChildProcess) {
            attrs.Add(ProcessAttribute.ChildProcess(RestrictChildProcess, OverrideRestrictChildProcess));
          }

          if (Token != null) {
            attrs.Add(ProcessAttribute.Token(Token.Handle));
          }

          using (ProcessAttributeList attr_list = ProcessAttributeList.Create(attrs)) {
            create_info.Data.InitFlags = InitFlags | ProcessCreateInitFlag.WriteOutputOnExit;
            create_info.Data.ProhibitedImageCharacteristics = ProhibitedImageCharacteristics;
            create_info.Data.AdditionalFileAccess = AdditionalFileAccess;

            using (ObjectAttributes proc_attr = new ObjectAttributes(null, AttributeFlags.None,
                SafeKernelObjectHandle.Null, null, ProcessSecurityDescriptor),
                thread_attr = new ObjectAttributes(null, AttributeFlags.None,
                SafeKernelObjectHandle.Null, null, ThreadSecurityDescriptor)) {
              NtStatus status = NtSystemCalls.NtCreateUserProcess(
                  out SafeKernelObjectHandle process_handle, out SafeKernelObjectHandle thread_handle,
                  ProcessDesiredAccess, ThreadDesiredAccess,
                  proc_attr, thread_attr, ProcessFlags,
                  ThreadFlags, process_params.DangerousGetHandle(), create_info, attr_list);

              if (!status.IsSuccess() && !ReturnOnError) {
                // Close handles which come from errors
                switch (create_info.State) {
                  case ProcessCreateState.FailOnSectionCreate:
                  NtSystemCalls.NtClose(create_info.Data.FileHandle);
                  break;
                  case ProcessCreateState.FailExeName:
                  NtSystemCalls.NtClose(create_info.Data.IFEOKey);
                  break;
                }

                status.ToNtException();
              }

              if (create_info.State == ProcessCreateState.Success) {
                return new CreateUserProcessResult(process_handle, thread_handle,
                    create_info.Data, image_info.Result, client_id.Result, TerminateOnDispose);
              } else {
                return new CreateUserProcessResult(status, create_info.Data, create_info.State);
              }
            }
          }
        }
      }
    }
  }
}
