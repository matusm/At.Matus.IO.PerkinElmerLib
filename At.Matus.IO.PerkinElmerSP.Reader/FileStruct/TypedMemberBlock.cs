// ============================================================================
// File: TypedMemberBlock.cs
// Project: At.Matus.IO.PerkinElmerSP.Reader
// Description: Represents a typed member block in PerkinElmer SP file format,
//              extending the base Block class with an additional TypeCode field
//              that identifies the data type of the block's content. Used for
//              data member blocks within wrapper blocks.
//
//              ID (int16)
//              Len (int32)
//              TypeCode (int16) -- only DataSetDataMember?
//              Data (len)
// 
// Copyright (c) 2026 Michael Matus
// ============================================================================

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public class TypedMemberBlock : Block
    {
        public short TypeCode { get; }

        public TypedMemberBlock(BinaryReader file) : base(file.ReadInt16())
        {
            int len = file.ReadInt32();
            TypeCode = file.ReadInt16();
            Data = file.ReadBytes(len - 2);
        }

        public string DumpDataAsHex()
        {
            string dataString = string.Empty;
            if(Data == null) return dataString;
            foreach (byte b in Data)
            {
                dataString += $"{b:X2} ";
            }
            return dataString.TrimEnd();
        }

        public override string ToString() => $"TypedMemberBlock: Id={(Members)Id} dataLength={(Data == null ? 0 : Data.Length)} TypeCode={(TypeCodes)TypeCode}";
    }
}
