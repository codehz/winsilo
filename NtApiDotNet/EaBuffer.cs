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
using System.IO;
using System.Linq;
using System.Text;

namespace NtApiDotNet {
  /// <summary>
  /// Flags for an EA entry
  /// </summary>
  [Flags]
  public enum EaBufferEntryFlags : byte {
    /// <summary>
    /// No flags.
    /// </summary>
    None = 0,
    /// <summary>
    /// Processor must handle this EA.
    /// </summary>
    NeedEa = 0x80,
  }

  /// <summary>
  /// A single EA entry.
  /// </summary>
  public sealed class EaBufferEntry {
    /// <summary>
    /// Name of the entry
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Data associated with the entry
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    /// Flags
    /// </summary>
    public EaBufferEntryFlags Flags { get; }

    internal EaBufferEntry Clone() {
      return new EaBufferEntry(Name, (byte[])Data.Clone(), Flags);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The name of the entry</param>
    /// <param name="data">Data associated with the entry</param>
    /// <param name="flags">Flags for entry.</param>
    public EaBufferEntry(string name, byte[] data, EaBufferEntryFlags flags) {
      Name = name;
      Data = data;
      Flags = flags;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The name of the entry</param>
    /// <param name="data">Data associated with the entry</param>
    /// <param name="flags">Flags for entry.</param>
    public EaBufferEntry(string name, int data, EaBufferEntryFlags flags)
        : this(name, BitConverter.GetBytes(data), flags) {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The name of the entry</param>
    /// <param name="data">Data associated with the entry</param>
    /// <param name="flags">Flags for entry.</param>
    public EaBufferEntry(string name, string data, EaBufferEntryFlags flags)
        : this(name, Encoding.Unicode.GetBytes(data), flags) {
    }

    /// <summary>
    /// Get the EA buffer data as a string.
    /// </summary>
    /// <returns>The data as a string.</returns>
    public string DataAsString() {
      if ((Data.Length % 2) != 0) {
        throw new ArgumentException("Invalid data length for a Unicode string");
      }
      return Encoding.Unicode.GetString(Data);
    }

    /// <summary>
    /// Get the EA buffer data as an Int32.
    /// </summary>
    /// <returns>The data as an Int32.</returns>
    public int DataAsInt32() {
      if (Data.Length != 4) {
        throw new ArgumentException("Invalid data length for an Int32");
      }
      return BitConverter.ToInt32(Data, 0);
    }

    /// <summary>
    /// Convert entry to a string
    /// </summary>
    /// <returns>The entry as a string</returns>
    public override string ToString() {
      return $"Name: {Name} - Data Size: {Data.Length} - Flags {Flags}";
    }
  }

  /// <summary>
  /// Class to create an Extended Attributes buffer for NtCreateFile
  /// </summary>
  public sealed class EaBuffer {
    /// <summary>
    /// Constructor
    /// </summary>
    public EaBuffer() : this(new EaBufferEntry[0]) {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="entries">List of entries to add.</param>
    public EaBuffer(IEnumerable<EaBufferEntry> entries) {
      _buffers = new List<EaBufferEntry>(entries);
    }

    /// <summary>
    /// Constructor from a binary EA buffer
    /// </summary>
    /// <param name="buffer">The EA buffer to parse</param>
    public EaBuffer(byte[] buffer) {
      MemoryStream stm = new MemoryStream(buffer);
      BinaryReader reader = new BinaryReader(stm);
      bool finished = false;
      _buffers = new List<EaBufferEntry>();
      while (!finished) {
        finished = DeserializeEntry(reader, out EaBufferEntry entry);
        _buffers.Add(entry);
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="buffer">Existing buffer to copy.</param>
    public EaBuffer(EaBuffer buffer)
        : this(buffer.Entries.Select(e => e.Clone())) {
    }

    private readonly List<EaBufferEntry> _buffers;

    private bool DeserializeEntry(BinaryReader reader, out EaBufferEntry entry) {
      long start_position = reader.BaseStream.Position;
      int next_offset = reader.ReadInt32();
      EaBufferEntryFlags flags = (EaBufferEntryFlags)reader.ReadByte();
      int ea_name_length = reader.ReadByte();
      int data_length = reader.ReadUInt16();
      string name = Encoding.ASCII.GetString(reader.ReadAllBytes(ea_name_length));
      reader.ReadByte();
      byte[] data = reader.ReadAllBytes(data_length);
      entry = new EaBufferEntry(name, data, flags);
      if (next_offset == 0) {
        return true;
      }
      reader.BaseStream.Position = start_position + next_offset;
      return false;
    }

    private byte[] SerializeEntry(EaBufferEntry entry, bool final) {
      int entry_size = (entry.Name.Length + entry.Data.Length + 9 + 3) & ~3;

      MemoryStream stm = new MemoryStream();
      BinaryWriter writer = new BinaryWriter(stm);
      // NextEntryOffset
      if (final) {
        writer.Write(0);
      } else {
        writer.Write(entry_size);
      }
      // Flags
      writer.Write((byte)entry.Flags);
      // EaNameLength
      writer.Write((byte)entry.Name.Length);
      // EaValueLength
      writer.Write((ushort)entry.Data.Length);
      // EaName
      writer.Write(Encoding.ASCII.GetBytes(entry.Name));
      // NUL terminator (not counted in name length)
      writer.Write((byte)0);
      // Data
      writer.Write(entry.Data);
      // Pad to next 4 byte boundary
      while (stm.Length < entry_size) {
        writer.Write((byte)0);
      }
      return stm.ToArray();
    }

    private void AddEntry(string name, byte[] data, EaBufferEntryFlags flags, bool clone) {
      _buffers.Add(new EaBufferEntry(name, clone ? (byte[])data.Clone() : data, flags));
    }

    /// <summary>
    /// Add a new EA entry from an old entry. The data will be cloned.
    /// </summary>
    /// <param name="entry">The entry to add.</param>
    public void AddEntry(EaBufferEntry entry) {
      AddEntry(entry.Name, entry.Data, entry.Flags);
    }

    /// <summary>
    /// Add a new EA entry
    /// </summary>
    /// <param name="name">The name of the entry</param>
    /// <param name="data">The associated data, will be cloned</param>
    /// <param name="flags">The entry flags.</param>
    public void AddEntry(string name, byte[] data, EaBufferEntryFlags flags) {
      AddEntry(name, data, flags, true);
    }

    /// <summary>
    /// Add a new EA entry
    /// </summary>
    /// <param name="name">The name of the entry</param>
    /// <param name="data">The associated data</param>
    /// <param name="flags">The entry flags.</param>
    public void AddEntry(string name, int data, EaBufferEntryFlags flags) {
      AddEntry(name, BitConverter.GetBytes(data), flags, false);
    }

    /// <summary>
    /// Add a new EA entry
    /// </summary>
    /// <param name="name">The name of the entry</param>
    /// <param name="data">The associated data</param>
    /// <param name="flags">The entry flags.</param>
    public void AddEntry(string name, string data, EaBufferEntryFlags flags) {
      AddEntry(name, Encoding.Unicode.GetBytes(data), flags, false);
    }

    private EaBufferEntry GetEntry(string name, bool throw_on_missing) {
      var ret = _buffers.Where(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      if (ret == null && throw_on_missing) {
        throw new KeyNotFoundException();
      }
      return ret;
    }

    /// <summary>
    /// Get an entry by name.
    /// </summary>
    /// <param name="name">The name of the entry.</param>
    /// <returns>The found entry.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no entry by that name.</exception>
    public EaBufferEntry GetEntry(string name) {
      return GetEntry(name, true);
    }

    /// <summary>
    /// Remove an entry from the buffer.
    /// </summary>
    /// <param name="entry">The entry to remove.</param>
    public void RemoveEntry(EaBufferEntry entry) {
      _buffers.Remove(entry);
    }

    /// <summary>
    /// Remove an entry from the buffer by name.
    /// </summary>
    /// <param name="name">The name of the entry.</param>
    /// <exception cref="KeyNotFoundException">Thrown if no entry by that name.</exception>
    public void RemoveEntry(string name) {
      RemoveEntry(GetEntry(name, true));
    }

    /// <summary>
    /// Convert to a byte array
    /// </summary>
    /// <returns>The byte array</returns>
    public byte[] ToByteArray() {
      using (MemoryStream stm = new MemoryStream()) {
        for (int i = 0; i < _buffers.Count; ++i) {
          byte[] entry = SerializeEntry(_buffers[i], i == _buffers.Count - 1);
          stm.Write(entry, 0, entry.Length);
        }
        return stm.ToArray();
      }
    }

    /// <summary>
    /// Get the list of entries.
    /// </summary>
    public IEnumerable<EaBufferEntry> Entries => _buffers.AsReadOnly();

    /// <summary>
    /// Get number of entries.
    /// </summary>
    public int Count => _buffers.Count;

    /// <summary>
    /// Get whether the buffer contains a specific entry.
    /// </summary>
    /// <param name="name">The name of the entry.</param>
    /// <returns>True if the buffer contains an entry with the name.</returns>
    public bool ContainsEntry(string name) {
      return GetEntry(name, false) != null;
    }

    /// <summary>
    /// Index to get an entry by name.
    /// </summary>
    /// <param name="name">The name of the entry.</param>
    /// <returns>The found entry.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no entry by that name.</exception>
    public EaBufferEntry this[string name] => GetEntry(name, true);

    /// <summary>
    /// Clear all entries.
    /// </summary>
    public void Clear() {
      _buffers.Clear();
    }
  }

}
