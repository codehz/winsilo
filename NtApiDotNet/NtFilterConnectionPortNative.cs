﻿//  Copyright 2020 Google Inc. All Rights Reserved.
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

namespace NtApiDotNet {
#pragma warning disable 1591
  [Flags]
  public enum FilterConnectionPortAccessRights : uint {
    Connect = 1,
    GenericRead = GenericAccessRights.GenericRead,
    GenericWrite = GenericAccessRights.GenericWrite,
    GenericExecute = GenericAccessRights.GenericExecute,
    GenericAll = GenericAccessRights.GenericAll,
    Delete = GenericAccessRights.Delete,
    ReadControl = GenericAccessRights.ReadControl,
    WriteDac = GenericAccessRights.WriteDac,
    WriteOwner = GenericAccessRights.WriteOwner,
    Synchronize = GenericAccessRights.Synchronize,
    MaximumAllowed = GenericAccessRights.MaximumAllowed,
    AccessSystemSecurity = GenericAccessRights.AccessSystemSecurity
  }
}
