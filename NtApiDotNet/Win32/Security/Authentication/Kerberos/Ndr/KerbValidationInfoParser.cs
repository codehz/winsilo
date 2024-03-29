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
using System.Linq;

namespace NtApiDotNet.Win32.Security.Authentication.Kerberos.Ndr {
#pragma warning disable 1591

  #region Marshal Helpers
  internal class _Unmarshal_Helper : NdrUnmarshalBuffer {
    internal _Unmarshal_Helper(NdrPickledType pickled_type) :
            base(pickled_type) {
    }
    internal KERB_VALIDATION_INFO Read_0() {
      return ReadStruct<KERB_VALIDATION_INFO>();
    }
    internal FILETIME Read_1() {
      return ReadStruct<FILETIME>();
    }
    internal RPC_UNICODE_STRING Read_2() {
      return ReadStruct<RPC_UNICODE_STRING>();
    }
    internal USER_SESSION_KEY Read_3() {
      return ReadStruct<USER_SESSION_KEY>();
    }
    internal CYPHER_BLOCK Read_4() {
      return ReadStruct<CYPHER_BLOCK>();
    }
    internal RPC_SID Read_5() {
      return ReadStruct<RPC_SID>();
    }
    internal RPC_SID_IDENTIFIER_AUTHORITY Read_6() {
      return ReadStruct<RPC_SID_IDENTIFIER_AUTHORITY>();
    }
    internal GROUP_MEMBERSHIP[] Read_GROUP_MEMBERSHIP() {
      return ReadConformantStructArray<GROUP_MEMBERSHIP>();
    }
    internal int[] Read_9() {
      return ReadFixedPrimitiveArray<int>(2);
    }
    internal int[] Read_10() {
      return ReadFixedPrimitiveArray<int>(7);
    }
    internal KERB_SID_AND_ATTRIBUTES[] Read_11() {
      return ReadConformantStructArray<KERB_SID_AND_ATTRIBUTES>();
    }
    internal char[] Read_13() {
      return ReadConformantVaryingArray<char>();
    }
    internal CYPHER_BLOCK[] Read_14() {
      return ReadFixedStructArray<CYPHER_BLOCK>(2);
    }
    internal sbyte[] Read_15() {
      return ReadFixedPrimitiveArray<sbyte>(8);
    }
    internal int[] Read_16() {
      return ReadConformantArray<int>();
    }
    internal byte[] Read_17() {
      return ReadFixedByteArray(6);
    }
  }
  #endregion
  #region Complex Types
  internal struct KERB_VALIDATION_INFO : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_Helper)u);
    }
    private void Unmarshal(_Unmarshal_Helper u) {
      LogonTime = u.Read_1();
      LogoffTime = u.Read_1();
      KickOffTime = u.Read_1();
      PasswordLastSet = u.Read_1();
      PasswordCanChange = u.Read_1();
      PasswordMustChange = u.Read_1();
      EffectiveName = u.Read_2();
      FullName = u.Read_2();
      LogonScript = u.Read_2();
      ProfilePath = u.Read_2();
      HomeDirectory = u.Read_2();
      HomeDirectoryDrive = u.Read_2();
      LogonCount = u.ReadInt16();
      BadPasswordCount = u.ReadInt16();
      UserId = u.ReadInt32();
      PrimaryGroupId = u.ReadInt32();
      GroupCount = u.ReadInt32();
      GroupIds = u.ReadEmbeddedPointer(u.Read_GROUP_MEMBERSHIP, false);
      UserFlags = u.ReadInt32();
      UserSessionKey = u.Read_3();
      LogonServer = u.Read_2();
      LogonDomainName = u.Read_2();
      LogonDomainId = u.ReadEmbeddedPointer(u.Read_5, false);
      Reserved1 = u.Read_9();
      UserAccountControl = u.ReadInt32();
      Reserved3 = u.Read_10();
      SidCount = u.ReadInt32();
      ExtraSids = u.ReadEmbeddedPointer(u.Read_11, false);
      ResourceGroupDomainSid = u.ReadEmbeddedPointer(u.Read_5, false);
      ResourceGroupCount = u.ReadInt32();
      ResourceGroupIds = u.ReadEmbeddedPointer(u.Read_GROUP_MEMBERSHIP, false);
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal FILETIME LogonTime;
    internal FILETIME LogoffTime;
    internal FILETIME KickOffTime;
    internal FILETIME PasswordLastSet;
    internal FILETIME PasswordCanChange;
    internal FILETIME PasswordMustChange;
    internal RPC_UNICODE_STRING EffectiveName;
    internal RPC_UNICODE_STRING FullName;
    internal RPC_UNICODE_STRING LogonScript;
    internal RPC_UNICODE_STRING ProfilePath;
    internal RPC_UNICODE_STRING HomeDirectory;
    internal RPC_UNICODE_STRING HomeDirectoryDrive;
    internal short LogonCount;
    internal short BadPasswordCount;
    internal int UserId;
    internal int PrimaryGroupId;
    internal int GroupCount;
    internal NdrEmbeddedPointer<GROUP_MEMBERSHIP[]> GroupIds;
    internal int UserFlags;
    internal USER_SESSION_KEY UserSessionKey;
    internal RPC_UNICODE_STRING LogonServer;
    internal RPC_UNICODE_STRING LogonDomainName;
    internal NdrEmbeddedPointer<RPC_SID> LogonDomainId;
    internal int[] Reserved1;
    internal int UserAccountControl;
    internal int[] Reserved3;
    internal int SidCount;
    internal NdrEmbeddedPointer<KERB_SID_AND_ATTRIBUTES[]> ExtraSids;
    internal NdrEmbeddedPointer<RPC_SID> ResourceGroupDomainSid;
    internal int ResourceGroupCount;
    internal NdrEmbeddedPointer<GROUP_MEMBERSHIP[]> ResourceGroupIds;
  }
  internal struct FILETIME : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_Helper)u);
    }
    private void Unmarshal(_Unmarshal_Helper u) {
      LowerValue = u.ReadInt32();
      UpperValue = u.ReadInt32();
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal int LowerValue;
    internal int UpperValue;

    public DateTime ToTime() {
      long value = UpperValue;
      value <<= 32;
      value |= (uint)LowerValue;

      try {
        return DateTime.FromFileTime(value);
      } catch (ArgumentOutOfRangeException) {
        return DateTime.MaxValue;
      }
    }
  }
  internal struct GROUP_MEMBERSHIP : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_Helper)u);
    }
    private void Unmarshal(_Unmarshal_Helper u) {
      RelativeId = u.ReadInt32();
      Attributes = u.ReadInt32();
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal int RelativeId;
    internal int Attributes;
  }
  internal struct RPC_UNICODE_STRING : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_Helper)u);
    }
    private void Unmarshal(_Unmarshal_Helper u) {
      Length = u.ReadInt16();
      MaximumLength = u.ReadInt16();
      Buffer = u.ReadEmbeddedPointer(u.Read_13, false);
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal short Length;
    internal short MaximumLength;
    internal NdrEmbeddedPointer<char[]> Buffer;
    public override string ToString() {
      if (Buffer == null)
        return string.Empty;
      return new string(Buffer, 0, Length / 2);
    }
  }
  internal struct USER_SESSION_KEY : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_Helper)u);
    }
    private void Unmarshal(_Unmarshal_Helper u) {
      data = u.Read_14();
    }
    int INdrStructure.GetAlignment() {
      return 1;
    }
    internal CYPHER_BLOCK[] data;
  }
  internal struct CYPHER_BLOCK : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_Helper)u);
    }
    private void Unmarshal(_Unmarshal_Helper u) {
      data = u.Read_15();
    }
    int INdrStructure.GetAlignment() {
      return 1;
    }
    internal sbyte[] data;
  }
  internal struct RPC_SID : INdrConformantStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_Helper)u);
    }
    private void Unmarshal(_Unmarshal_Helper u) {
      Revision = u.ReadSByte();
      SubAuthorityCount = u.ReadSByte();
      IdentifierAuthority = u.Read_6();
      SubAuthority = u.Read_16();
    }
    int INdrConformantStructure.GetConformantDimensions() {
      return 1;
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }

    internal Sid ToSid() {
      return new Sid(new SidIdentifierAuthority(IdentifierAuthority.Value),
          SubAuthority.Select(r => (uint)r).ToArray());
    }

    internal sbyte Revision;
    internal sbyte SubAuthorityCount;
    internal RPC_SID_IDENTIFIER_AUTHORITY IdentifierAuthority;
    internal int[] SubAuthority;
  }
  internal struct RPC_SID_IDENTIFIER_AUTHORITY : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_Helper)u);
    }
    private void Unmarshal(_Unmarshal_Helper u) {
      Value = u.Read_17();
    }
    int INdrStructure.GetAlignment() {
      return 1;
    }
    internal byte[] Value;
  }
  internal struct KERB_SID_AND_ATTRIBUTES : INdrStructure {
    void INdrStructure.Marshal(NdrMarshalBuffer m) {
      throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u) {
      Unmarshal((_Unmarshal_Helper)u);
    }
    private void Unmarshal(_Unmarshal_Helper u) {
      Sid = u.ReadEmbeddedPointer(u.Read_5, false);
      Attributes = u.ReadInt32();
    }
    int INdrStructure.GetAlignment() {
      return 4;
    }
    internal NdrEmbeddedPointer<RPC_SID> Sid;
    internal int Attributes;
  }
  #endregion
  #region Complex Type Encoders
  internal static class KerbValidationInfoParser {
    internal static KERB_VALIDATION_INFO? Decode(NdrPickledType pickled_type) {
      _Unmarshal_Helper u = new _Unmarshal_Helper(pickled_type);
      return u.ReadReferentValue(u.Read_0, false);
    }
  }
  #endregion
}