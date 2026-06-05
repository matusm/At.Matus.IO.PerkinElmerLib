// ============================================================================
// File: HistoryRecordEntry.cs
// Project: At.Matus.IO.PerkinElmerSP.Reader
// Description: Represents a single history record entry in PerkinElmer SP files,
//              containing metadata about measurements and instrument parameters.
//              Provides ID-to-name mapping, value type detection, and byte offset
//              calculation for parsing sequential history records.
// 
// Copyright (c) 2026 Michael Matus
// ============================================================================

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public class HistoryRecordEntry
    {
        public int ID { get; set; }
        public string RecordText { get; set; } = string.Empty;
        public string KeyName => ToKeyName(ID);
        public bool IsKnownRecord => Enum.IsDefined(typeof(HistoryRecordTitle), ID);
        public int AdvanceBy => CalculateOffset(); // Advance scan index by this number of bytes to get to the next record
        public HistoryRecordValueType ValueType { get; set; } = HistoryRecordValueType.Unknown;

        private string ToKeyName(int code)
        {
            if (IsKnownRecord)
                return ((HistoryRecordTitle)code).ToString();
            else
                return $"_UnknownRecord{KeyNamePostfix()}_ID{code:D3}";
        }

        private string KeyNamePostfix()
        {
            {
                switch (ValueType)
                {
                    case HistoryRecordValueType.Text:
                        return "_String";
                    case HistoryRecordValueType.Short:
                        return "_Short";
                    case HistoryRecordValueType.Double:
                        return "_Double";
                    case HistoryRecordValueType.ShortX:
                        return "_ShortX";
                    default:
                        return string.Empty;
                }
            }
        }

        private int CalculateOffset()
        {
            switch (ValueType)
            {
                case HistoryRecordValueType.Text:
                    return RecordText.Length + 4; // for text records, length is the length of the text plus 4 bytes for the length field
                case HistoryRecordValueType.Short:
                    return 4;
                case HistoryRecordValueType.Double:
                    return 10; // for double records, length is always 14 bytes
                default:
                    return 0;
            }
        }

        public override string ToString() => $"ID: {ID,3}, KeyName: {KeyName}, RecordText: {RecordText}";
    }
}
