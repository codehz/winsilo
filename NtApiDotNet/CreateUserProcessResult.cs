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

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;

namespace NtApiDotNet {
  /// <summary>
  /// Result from a native create process call.
  /// </summary>
  [Obsolete("Use NtProcessCreateResult")]
  public sealed class CreateUserProcessResult : IDisposable {
    /// <summary>
    /// Handle to the process
    /// </summary>
    public NtProcess Process { get; }

    /// <summary>
    /// Handle to the initial thread
    /// </summary>
    public NtThread Thread {
      get;
    }

    /// <summary>
    /// Handle to the image file
    /// </summary>
    public NtFile ImageFile {
      get;
    }

    /// <summary>
    /// Handle to the image section
    /// </summary>
    public NtSection SectionHandle { get; }

    /// <summary>
    /// Handle to the IFEO key (if it exists)
    /// </summary>
    public RegistryKey IFEOKeyHandle {
      get;
    }

    /// <summary>
    /// Image information
    /// </summary>
    public SectionImageInformation ImageInfo {
      get;
    }

    /// <summary>
    /// Client ID of process and thread
    /// </summary>
    public ClientId ClientId {
      get;
    }

    /// <summary>
    /// Process ID
    /// </summary>
    public int ProcessId => ClientId.UniqueProcess.ToInt32();

    /// <summary>
    /// Thread ID
    /// </summary>
    public int ThreadId => ClientId.UniqueThread.ToInt32();

    /// <summary>
    /// Create status
    /// </summary>
    public NtStatus Status {
      get;
    }

    /// <summary>
    /// True if create succeeded
    /// </summary>
    public bool Success => Status == 0;

    /// <summary>
    /// Result of the create information
    /// </summary>
    public ProcessCreateInfoData CreateInfo {
      get;
    }

    /// <summary>
    /// Creation state
    /// </summary>
    public ProcessCreateState CreateState {
      get;
    }

    internal CreateUserProcessResult(SafeKernelObjectHandle process_handle, SafeKernelObjectHandle thread_handle,
      ProcessCreateInfoData create_info,
      SectionImageInformation image_info, ClientId client_id, bool terminate_on_dispose) {
      Process = new NtProcess(process_handle);
      Thread = new NtThread(thread_handle);
      ImageFile = new NtFile(new SafeKernelObjectHandle(create_info.Success.FileHandle, true));
      SectionHandle = new NtSection(new SafeKernelObjectHandle(create_info.Success.SectionHandle, true));
      ImageInfo = image_info;
      ClientId = client_id;
      CreateInfo = create_info;
      CreateState = ProcessCreateState.Success;
      TerminateOnDispose = terminate_on_dispose;
    }

    internal CreateUserProcessResult(NtStatus status, ProcessCreateInfoData create_info, ProcessCreateState create_state) {
      ImageFile = null;
      if (create_state == ProcessCreateState.FailOnSectionCreate) {
        ImageFile = new NtFile(new SafeKernelObjectHandle(create_info.FileHandle, true));
      } else if (create_state == ProcessCreateState.FailExeName) {
        IFEOKeyHandle = RegistryKey.FromHandle(new SafeRegistryHandle(create_info.IFEOKey, true));
      }
      Status = status;
      CreateInfo = create_info;
      CreateState = create_state;

      Process = null;
      Thread = null;
      SectionHandle = null;
      ImageInfo = new SectionImageInformation();
      ClientId = new ClientId();
    }

    /// <summary>
    /// Terminate the process
    /// </summary>
    /// <param name="exitcode">Exit code for termination</param>
    public void Terminate(NtStatus exitcode) {
      Process?.Terminate(exitcode);
    }

    /// <summary>
    /// Resume initial thread
    /// </summary>
    /// <returns>The suspend count</returns>
    public int Resume() {
      return Thread?.Resume() ?? 0;
    }

    /// <summary>
    /// Set to true to terminate process on disposal
    /// </summary>
    public bool TerminateOnDispose {
      get; set;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    private void Dispose(bool disposing) {
      if (!disposedValue) {
        if (TerminateOnDispose) {
          try {
            Terminate(NtStatus.STATUS_WAIT_1);
          } catch (NtException) {
          }
        }

        Process?.Close();
        Thread?.Close();
        ImageFile?.Close();
        SectionHandle?.Close();
        IFEOKeyHandle?.Close();
        disposedValue = true;
      }
    }

    /// <summary>
    /// Finalizer
    /// </summary>
    ~CreateUserProcessResult() {
      Dispose(false);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
