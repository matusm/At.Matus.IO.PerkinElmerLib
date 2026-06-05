// ==============================================================================
// File: HistoryRecordParser.cs
// Description: Parser for PerkinElmer SP file format history records.
//              Extracts and interprets history record entries from
//              TypedMemberBlock data,
//              supporting text, short integer, and double value types.
//
//              The parser identifies records based on specific byte patterns.
//              It validates the structure of records and maps record IDs to
//              known titles, while also allowing retrieval of unknown records.
//              The main method GetHistoryRecordsAsDictionary returns a
//              dictionary of record titles and their corresponding values,
//              with an option to include or exclude unknown records.
//
//              The history records are identified by specific byte patterns in the data:
//              - Text records start with the bytes 0x23 0x75 "#u", followed by a 2-byte length and the text itself.
//              - Short integer records start with the bytes 0x2D 0x75 "-u", followed by a 2-byte value.
//              - Double records start with the bytes 0x1C 0x75 ".u", followed by an 8-byte double value.
//              Each record is preceded by 6 bytes that include three short integers (id1, id2, id3).
//              The first short integer (id1) is considered as the actual record id (after adding 29839 to make it positive),
//              the second short integer (id2) indicates the length of the record text plus 4 for text records, or is 8 for short integer records, or 14 for double records,
//              and the third short integer (id3) is always 0.
//
// Copyright (c) 2026 Michael Matus
// ==============================================================================

using System.Text;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public class HistoryRecordParser
    {
        public HistoryRecordParser(TypedMemberBlock tmb)
        {
            if (tmb.Id != (short)Members.DataSetHistoryRecord)
                throw new NotSupportedException("Unsupported data type for history record.");
            if (tmb.Data == null)
                throw new InvalidOperationException("TypedMemberBlock.Data is null.");
            _data = tmb.Data!;
        }

        public Dictionary<string, string> GetHistoryRecordsAsDictionary(bool includeUnknowns = false)
        {
            var historyRecords = GetHistoryRecordsAsObjects();
            Dictionary<string, string> records = new Dictionary<string, string>();
            for (int i = 0; i < historyRecords.Length; i++)
                if (includeUnknowns || historyRecords[i].IsKnownRecord)
                    records[historyRecords[i].KeyName] = historyRecords[i].RecordText;
            return records;
        }

        public HistoryRecordEntry[] GetHistoryRecordsAsObjects()
        {
            List<HistoryRecordEntry> records = new List<HistoryRecordEntry>();
            for (int i = 6; i < _data.Length - 4; i++)
            {
                var recordType = ScanForHistoryRecords(i);
                if (recordType != HistoryRecordValueType.Unknown)
                {
                    var historyRecord = new HistoryRecordEntry();
                    historyRecord.ValueType = recordType;
                    historyRecord.ID = GetRecordId1(i) + 29839; // to make the id positive
                    switch (recordType)
                    {
                        case HistoryRecordValueType.Text:
                            historyRecord.RecordText = Encoding.Default.GetString(_data, i + 4, GetRecordId4(i));
                            break;
                        case HistoryRecordValueType.Short:
                        case HistoryRecordValueType.ShortX:
                            historyRecord.RecordText = GetRecordId4(i).ToString();
                            break;
                        case HistoryRecordValueType.Double:
                            historyRecord.RecordText = GetRecordDoubleValue(i).ToString();
                            break;
                        default:
                            break;
                    }
                    i += historyRecord.AdvanceBy; // move index to the end of the current record
                    records.Add(historyRecord);
                }
            }
            return records.ToArray();
        }

        private HistoryRecordValueType ScanForHistoryRecords(int index)
        {
            if (_data[index] == 0x23 && _data![index + 1] == 0x75) // #u
                if (GetRecordId3(index) == 0 && GetRecordId2(index) - GetRecordId4(index) == 4)
                    return HistoryRecordValueType.Text;
            if (_data[index] == 0x2D && _data![index + 1] == 0x75) // -u
                if (GetRecordId3(index) == 0 && GetRecordId2(index) == 8)
                    return HistoryRecordValueType.Short;
            if (_data[index] == 0x1C && _data[index + 1] == 0x75) // .u
                if (GetRecordId3(index) == 0 && GetRecordId2(index) == 14)
                    return HistoryRecordValueType.Double;
            if (_data[index] == 0x15 && _data[index + 1] == 0x75) // 0x15u
                if (GetRecordId3(index) == 0 && GetRecordId2(index) == 4)
                    return HistoryRecordValueType.ShortX;
            return HistoryRecordValueType.Unknown;
        }

        private short GetRecordId1(int index)
        {
            return BitConverter.ToInt16(new byte[] { _data[index - 6], _data[index - 5] }, 0);
        }

        private short GetRecordId2(int index)
        {
            return BitConverter.ToInt16(new byte[] { _data[index - 4], _data[index - 3] }, 0);
        }

        private short GetRecordId3(int index)
        {
            return BitConverter.ToInt16(new byte[] { _data[index - 2], _data[index - 1] }, 0);
        }

        private short GetRecordId4(int index)
        {
            return BitConverter.ToInt16(new byte[] { _data[index + 2], _data[index + 3] }, 0);
        }

        private double GetRecordDoubleValue(int index)
        {
            return BitConverter.ToDouble(new byte[] { _data[index + 2], _data[index + 3], _data[index + 4], _data[index + 5], _data[index + 6], _data[index + 7], _data[index + 8], _data[index + 9] }, 0);
        }

        private readonly byte[] _data;
    }
}
