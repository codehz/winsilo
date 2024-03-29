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
  internal class _Unmarshal_HelperClaimSet : NdrUnmarshalBuffer {
    internal _Unmarshal_HelperClaimSet(NtApiDotNet.Win32.Rpc.RpcClientResponse r) :
            base(r.NdrBuffer, r.Handles, r.DataRepresentation) {
    }
    internal _Unmarshal_HelperClaimSet(byte[] ba) :
            base(ba) {
    }
    internal _Unmarshal_HelperClaimSet(NdrPickledType pickled_type) :
            base(pickled_type) {
    }
    internal CLAIMS_SET Read_0() {
      return ReadStruct<CLAIMS_SET>();
    }
    internal CLAIMS_ARRAY Read_1() {
      return ReadStruct<CLAIMS_ARRAY>();
    }
    internal CLAIM_ENTRY Read_2() {
      return ReadStruct<CLAIM_ENTRY>();
    }
    internal CLAIM_ENTRY_VALUE Read_3() {
      return ReadStruct<CLAIM_ENTRY_VALUE>();
    }
    internal CLAIM_TYPE_INT64 Read_4() {
      return ReadStruct<CLAIM_TYPE_INT64>();
    }
    internal CLAIM_TYPE_UINT64 Read_5() {
      return ReadStruct<CLAIM_TYPE_UINT64>();
    }
    internal CLAIM_TYPE_STRING Read_6() {
      return ReadStruct<CLAIM_TYPE_STRING>();
    }
    internal CLAIM_TYPE_BOOLEAN Read_7() {
      return ReadStruct<CLAIM_TYPE_BOOLEAN>();
    }
    internal CLAIMS_ARRAY[] Read_8() {
      return ReadConformantStructArray<CLAIMS_ARRAY>();
    }
    internal byte[] Read_9() {
      return ReadConformantArray<byte>();
    }
    internal CLAIM_ENTRY[] Read_10() {
      return ReadConformantStructArray<CLAIM_ENTRY>();
    }
    internal long[] Read_11() {
      return ReadConformantArray<long>();
    }
    internal string[] Read_12() {
      return ReadConformantStringArray(new System.Func<string>(this.ReadConformantVaryingString));
    }
  }
  #endregion
  #region Complex Types
  internal struct CLAIMS_SET : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal(((_Unmarshal_HelperClaimSet)u));
    }
    private void Unmarshal(_Unmarshal_HelperClaimSet u) {
      ulClaimsArrayCount = u.ReadInt32();
      ClaimsArrays = u.ReadEmbeddedPointer<CLAIMS_ARRAY[]>(new System.Func<CLAIMS_ARRAY[]>(u.Read_8), false);
      usReservedType = u.ReadInt16();
      ulReservedFieldSize = u.ReadInt32();
      ReservedField = u.ReadEmbeddedPointer<byte[]>(new System.Func<byte[]>(u.Read_9), false);
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal int ulClaimsArrayCount;
    internal NdrEmbeddedPointer<CLAIMS_ARRAY[]> ClaimsArrays;
    internal short usReservedType;
    internal int ulReservedFieldSize;
    internal NdrEmbeddedPointer<byte[]> ReservedField;
  }
  internal struct CLAIMS_ARRAY : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal(((_Unmarshal_HelperClaimSet)u));
    }
    private void Unmarshal(_Unmarshal_HelperClaimSet u) {
      usClaimsSourceType = u.ReadEnum16();
      ulClaimsCount = u.ReadInt32();
      ClaimEntries = u.ReadEmbeddedPointer<CLAIM_ENTRY[]>(new System.Func<CLAIM_ENTRY[]>(u.Read_10), false);
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal NdrEnum16 usClaimsSourceType;
    internal int ulClaimsCount;
    internal NdrEmbeddedPointer<CLAIM_ENTRY[]> ClaimEntries;
  }
  internal struct CLAIM_ENTRY : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_HelperClaimSet)(u));
    }
    private void Unmarshal(_Unmarshal_HelperClaimSet u) {
      Id = u.ReadEmbeddedPointer<string>(new System.Func<string>(u.ReadConformantVaryingString), false);
      ClaimType = u.ReadEnum16();
      Values = u.Read_3();
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal NdrEmbeddedPointer<string> Id;
    internal NdrEnum16 ClaimType;
    internal CLAIM_ENTRY_VALUE Values;
  }
  internal struct CLAIM_ENTRY_VALUE : INdrNonEncapsulatedUnion {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new System.NotImplementedException();
    }
    void INdrNonEncapsulatedUnion.Marshal(NdrMarshalBuffer m, long l) {
      throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal(((_Unmarshal_HelperClaimSet)(u)));
    }
    private void Unmarshal(_Unmarshal_HelperClaimSet u) {
      Selector = u.ReadEnum16();
      if ((Selector == 1)) {
        Arm_1 = u.Read_4();
        goto done;
      }
      if ((Selector == 2)) {
        Arm_2 = u.Read_5();
        goto done;
      }
      if ((Selector == 3)) {
        Arm_3 = u.Read_6();
        goto done;
      }
      if ((Selector == 6)) {
        Arm_6 = u.Read_7();
        goto done;
      }
      Arm_Default = u.ReadEmpty();
    done:
      return;
    }
    int INdrStructure.GetAlignment() {
      return 1;
    }
    private NdrEnum16 Selector;
    internal CLAIM_TYPE_INT64 Arm_1;
    internal CLAIM_TYPE_UINT64 Arm_2;
    internal CLAIM_TYPE_STRING Arm_3;
    internal CLAIM_TYPE_BOOLEAN Arm_6;
    internal NdrEmpty Arm_Default;
  }
  internal struct CLAIM_TYPE_INT64 : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal(((_Unmarshal_HelperClaimSet)(u)));
    }
    private void Unmarshal(_Unmarshal_HelperClaimSet u) {
      ValueCount = u.ReadInt32();
      Int64Values = u.ReadEmbeddedPointer<long[]>(new System.Func<long[]>(u.Read_11), false);
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal int ValueCount;
    internal NdrEmbeddedPointer<long[]> Int64Values;
  }
  internal struct CLAIM_TYPE_UINT64 : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal(((_Unmarshal_HelperClaimSet)(u)));
    }
    private void Unmarshal(_Unmarshal_HelperClaimSet u) {
      ValueCount = u.ReadInt32();
      Uint64Values = u.ReadEmbeddedPointer<long[]>(new System.Func<long[]>(u.Read_11), false);
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal int ValueCount;
    internal NdrEmbeddedPointer<long[]> Uint64Values;
  }
  internal struct CLAIM_TYPE_STRING : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal(((_Unmarshal_HelperClaimSet)(u)));
    }
    private void Unmarshal(_Unmarshal_HelperClaimSet u) {
      ValueCount = u.ReadInt32();
      StringValues = u.ReadEmbeddedPointer<string[]>(new System.Func<string[]>(u.Read_12), false);
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal int ValueCount;
    internal NdrEmbeddedPointer<string[]> StringValues;
  }
  internal struct CLAIM_TYPE_BOOLEAN : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal(((_Unmarshal_HelperClaimSet)(u)));
    }
    private void Unmarshal(_Unmarshal_HelperClaimSet u) {
      ValueCount = u.ReadInt32();
      BooleanValues = u.ReadEmbeddedPointer<long[]>(new System.Func<long[]>(u.Read_11), false);
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal int ValueCount;
    internal NdrEmbeddedPointer<long[]> BooleanValues;
  }
  #endregion
  #region Complex Type Encoders
  internal static class ClaimSetParser {
    internal static CLAIMS_SET? Decode(NdrPickledType pickled_type) {
      _Unmarshal_HelperClaimSet u = new _Unmarshal_HelperClaimSet(pickled_type);
      return u.ReadReferentValue(u.Read_0, false);
    }
  }
  #endregion
}

