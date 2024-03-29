//  Copyright 2020 Google Inc. All Rights Reserved.
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

using NtApiDotNet.Ndr.Marshal;
using System;

namespace NtApiDotNet.Win32.Security.Authentication.Kerberos.Ndr {
#pragma warning disable 1591
  #region Marshal Helpers
  internal class _Unmarshal_HelperClaimSetMetadata : NdrUnmarshalBuffer {
    internal _Unmarshal_HelperClaimSetMetadata(NdrPickledType pickled_type) :
            base(pickled_type) {
    }
    internal CLAIM_SET_METADATA Read_0() {
      return ReadStruct<CLAIM_SET_METADATA>();
    }
    internal byte[] Read_1() {
      return ReadConformantArray<byte>();
    }
    internal byte[] Read_2() {
      return ReadConformantArray<byte>();
    }
  }
  #endregion
  #region Complex Types
  internal struct CLAIM_SET_METADATA : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_HelperClaimSetMetadata)u);
    }
    private void Unmarshal(_Unmarshal_HelperClaimSetMetadata u) {
      ulClaimsSetSize = u.ReadInt32();
      ClaimsSet = u.ReadEmbeddedPointer<byte[]>(new System.Func<byte[]>(u.Read_1), false);
      usCompressionFormat = u.ReadEnum16();
      ulUncompressedClaimsSetSize = u.ReadInt32();
      usReservedType = u.ReadInt16();
      ulReservedFieldSize = u.ReadInt32();
      ReservedField = u.ReadEmbeddedPointer<byte[]>(new System.Func<byte[]>(u.Read_2), false);
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal int ulClaimsSetSize;
    internal NdrEmbeddedPointer<byte[]> ClaimsSet;
    internal NdrEnum16 usCompressionFormat;
    internal int ulUncompressedClaimsSetSize;
    internal short usReservedType;
    internal int ulReservedFieldSize;
    internal NdrEmbeddedPointer<byte[]> ReservedField;
  }
  #endregion
  #region Complex Type Encoders

  internal static class ClaimSetMetadataParser {
    internal static CLAIM_SET_METADATA? Decode(NdrPickledType pickled_type) {
      _Unmarshal_HelperClaimSetMetadata u = new _Unmarshal_HelperClaimSetMetadata(pickled_type);
      return u.ReadReferentValue(u.Read_0, false);
    }
  }
  #endregion
}

