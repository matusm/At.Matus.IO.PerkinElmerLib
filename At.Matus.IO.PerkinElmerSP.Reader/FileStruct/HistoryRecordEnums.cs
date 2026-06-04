// ============================================================================
// File: HistoryRecordEnums.cs
// Project: At.Matus.IO.PerkinElmerSP.Reader
// Description: Enumerations for PerkinElmer SP file history record structure:
//              HistoryRecordValueType defines data types for history values,
//              and HistoryRecordTitle defines title codes (stored as 2-byte
//              integers, offset by 29839) mapped to instrument and measurement
//              parameter descriptions.
// 
// Copyright (c) 2026 Michael Matus
// All rights reserved.
// ============================================================================

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public enum HistoryRecordValueType
    {
        Unknown,
        Text,
        Short,
        Double,
        ShortX
    }

    // History record title codes
    // In the file, these are stored as a 2-byte integer, 29839 is added to make it positive
    // It is eventually mapped to a string title by the software
    public enum HistoryRecordTitle : int
    {
        OperatorName = 1,
        Modification = 2,
        ModificationDateTime = 3,
        Parameters = 4,
        SampleDescription = 5,
        InstrumentModel = 140,
        InstrumentSerialNumber = 141,
        InstrumentSoftwareRevision = 142,
        SlitWidth = 210,
        LampsUsed = 213,
        CycleTime = 215,
        Cycles = 217,
        InstrumentAccessories = 218,
        UvVisSlitMode = 224,
        NirSlitMode = 225,
        NirSlitWidth = 226,
        UvVisIntegrationTime = 227,
        NirIntegrationTime = 228,
        NirDetectorGain = 230,
        MonochromatorChangeAt = 236,
        LampChangeAt = 237,
        DetectorChangeAt = 238,
        SampleBeamPosition = 239,
        CommonBeamMask = 240,
        CommonBeamDepolarizer = 241,
        AttenuatorsUsed = 242
    }
}

