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

using NtApiDotNet.Utilities.ASN1;
using System.Collections.Generic;
using System.IO;

namespace NtApiDotNet.Win32.Security.Authentication.Kerberos {
  /// <summary>
  /// A Kerberos Principal Name.
  /// </summary>
  public sealed class KerberosPrincipalName {
    /// <summary>
    /// The name type.
    /// </summary>
    public KerberosNameType NameType { get; private set; }
    /// <summary>
    /// The names for the principal.
    /// </summary>
    public IReadOnlyList<string> Names { get; private set; }
    /// <summary>
    /// Full name.
    /// </summary>
    public string FullName => string.Join("/", Names);

    /// <summary>
    /// ToString method.
    /// </summary>
    /// <returns>String of the object.</returns>
    public override string ToString() {
      return $"{NameType} - {FullName}";
    }

    /// <summary>
    /// Get principal name with a realm.
    /// </summary>
    /// <param name="realm">The realm for the principal.</param>
    /// <returns>The principal.</returns>
    public string GetPrincipal(string realm) {
      return $"{FullName}@{realm}";
    }

    internal KerberosPrincipalName()
        : this(KerberosNameType.UNKNOWN, new string[0]) {
    }

    internal KerberosPrincipalName(KerberosNameType name_type,
        IEnumerable<string> names) {
      NameType = name_type;
      Names = new List<string>(names).AsReadOnly();
    }

    internal static KerberosPrincipalName Parse(DERValue value) {
      if (!value.HasChildren())
        throw new InvalidDataException();
      KerberosPrincipalName ret = new KerberosPrincipalName();
      foreach (var next in value.Children) {
        if (next.Type != DERTagType.ContextSpecific)
          throw new InvalidDataException();
        switch (next.Tag) {
          case 0:
          ret.NameType = (KerberosNameType)next.ReadChildInteger();
          break;
          case 1:
          ret.Names = next.ReadChildStringSequence().AsReadOnly();
          break;
          default:
          throw new InvalidDataException();
        }
      }
      return ret;
    }
  }
}
