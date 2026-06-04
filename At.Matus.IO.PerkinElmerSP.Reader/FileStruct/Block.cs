// ============================================================================
// File: Block.cs
// Project: At.Matus.IO.PerkinElmerSP.Reader
// Description: Represents a data block in PerkinElmer SP file format, containing
//              an ID, length, and binary data. Blocks can be wrapper-blocks or
//              member-blocks (with inner type codes) forming the hierarchical
//              structure of SP spectral files.
//
//              Each block contains fields:
//              Id (int16), Length (int32),
//              [for "member"-blocks only: innerCode (int16) = data type],
//              data (arbitrary).
//              For *.SP files "member-blocks" are considered as data
//              of "wrapper-blocks"
// 
// Copyright (c) 2026 Michael Matus
// All rights reserved.
// ============================================================================

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public class Block
    {
        public short Id { get; }
        public byte[] Data { get; protected set; }

        protected Block(short id)
        {
            Id = id;
        }

        public Block(BinaryReader file)
        {
            Id = file.ReadInt16();
            int len = file.ReadInt32();
            Data = file.ReadBytes(len);
            if (Data.Length < len) throw new EndOfStreamException();
        }

        public Block(short id, byte[] data)
        {
            Id = id;
            Data = data;
        }
    }
}
