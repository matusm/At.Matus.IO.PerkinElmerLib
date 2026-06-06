// ============================================================================
// File: SpFileTool.cs
// Project: At.Matus.IO.PerkinElmerSP.Reader
// Description: Main tool for reading and parsing PerkinElmer SP spectral files.
//              Extracts spectrum data, metadata, and history records from binary
//              SP file format into Spectrum2d objects. Based on original Matlab
//              code by Stephen Westlake and Seer Green (Perkin Elmer, 2007),
//              adapted for C#.NET by Kutukov Pavel (2022).
// 
// Copyright (c) 2026 Michael Matus
// ============================================================================

using System.Globalization;
using System.Text;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public class SpFileTool
    {
        public bool IncludUnknownBlocksInMetaData { get; set; } = false;

        public Spectrum2d GetData(string path)
        {
            if(path == null)
                throw new ArgumentNullException(nameof(path));
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian architectures are not supported (yet).");
            Block main = BlockFile.Load(path).Contents.FirstOrDefault(x => x.Id == (short)mainBlock)!;
            if (main == null)
                throw new NotSupportedException($"This SP file doesn't contain a {Enum.GetName(typeof(Blocks), mainBlock)} block.");
            if(main.Data == null)
                throw new ArgumentException("Main block contains no data.");
            Spectrum2d spec = new Spectrum2d();
            foreach (TypedMemberBlock item in ParseMembers(main.Data))
            {
                GetSpectrumWrapper(item, spec);
            }
            spec.MetaData.AddRecord("BlockFileDescription", BlockFile.Load(path).Description);
            spec.MetaData.AddRecord("SourceFile", Path.GetFileName(path));
            spec.MetaData.AddRecord("DllVersion", $"{typeof(SpFileTool).Assembly.GetName().Name} {typeof(SpFileTool).Assembly.GetName().Version}");
            return spec;
        }

        private string ReadString(byte[] data)
        {
            try
            {
                int len = BitConverter.ToInt16(data, 0);
                return Encoding.Latin1.GetString(data, 2, len);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Couldn't read a string field due to a bad length value.");
            }
        }

        private void GetSpectrumWrapper(TypedMemberBlock tmb, Spectrum2d sp)
        {
            if(tmb.Data == null)
                throw new ArgumentException("Data member block contains no data.");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            switch ((Members)tmb.Id)
            {
                case Members.DataSetAbscissaRange:
                    if (tmb.TypeCode != (short)TypeCodes.CvCoOrdRange)
                        throw new NotSupportedException("Not supported data type for X axis range.");
                    sp.StartX = BitConverter.ToDouble(tmb.Data, 0);
                    sp.EndX = BitConverter.ToDouble(tmb.Data, sizeOfDouble);
                    sp.MetaData.AddRecord("DataSetAbscissaRange", $"{sp.EndX} {sp.StartX}");
                    break;
                case Members.DataSetInterval:
                    sp.ResolutionX = BitConverter.ToDouble(tmb.Data, 0);
                    sp.MetaData.AddRecord("DataSetInterval", $"{sp.ResolutionX}");
                    break;
                case Members.DataSetNumPoints:
                    // there might be an inconsitency with the actual length of the data array!
                    var numPoints = BitConverter.ToInt32(tmb.Data, 0);
                    sp.Points = new Point2d[numPoints];
                    sp.MetaData.AddRecord("DataSetNumPoints", $"{numPoints}");
                    break;
                case Members.DataSetXAxisLabel:
                    sp.LabelX = ReadString(tmb.Data);
                    sp.MetaData.AddRecord("DataSetXAxisLabel", $"{sp.LabelX}");
                    break;
                case Members.DataSetYAxisLabel:
                    sp.LabelY = ReadString(tmb.Data);
                    sp.MetaData.AddRecord("DataSetYAxisLabel", $"{sp.LabelY}");
                    break;
                case Members.DataSetData:
                    if (tmb.TypeCode != (short)TypeCodes.CvCoOrdArray)
                        throw new NotSupportedException("Not supported data type for Y data array.");
                    if (sp.Points == null)
                        sp.Points = new Point2d[BitConverter.ToInt32(tmb.Data, 0) / sizeOfDouble];
                    try
                    {
                        for (int i = 0; i < sp.Points.Length; i++)
                        {
                            double y = BitConverter.ToDouble(tmb.Data, dataMemberDataOffset + i * sizeOfDouble);
                            sp.Points[i] = new Point2d { X = sp.StartX + i * sp.ResolutionX, Y = y };
                        }
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Warning: an unexpected end of data member block has been encountered.");
                    }
                    break;
                case Members.DataSetName:
                    sp.MetaData.AddRecord("DataSetName", $"{ReadString(tmb.Data)}");
                    break;
                case Members.DataSetAlias:
                    sp.MetaData.AddRecord("DataSetAlias", $"{ReadString(tmb.Data)}");
                    break;
                case Members.DataSetHistoryRecord:
                    var histParser = new HistoryRecordParser(tmb);
                    var histRecordsAsDictionary = histParser.GetHistoryRecordsAsDictionary(IncludUnknownBlocksInMetaData);
                    sp.MetaData.AddRecords(histRecordsAsDictionary);
                    break;
                case Members.DataSetDataType:
                    sp.MetaData.AddRecord("DataSetDataType", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetFileType:
                    sp.MetaData.AddRecord("DataSetFileType", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetSamplingMethod:
                    sp.MetaData.AddRecord("DataSetSamplingMethod", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetXAxisUnitType:
                    sp.MetaData.AddRecord("DataSetXAxisUnitType", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetYAxisUnitType:
                    sp.MetaData.AddRecord("DataSetYAxisUnitType", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetChecksum:
                    sp.MetaData.AddRecord("DataSetChecksum", $"{BitConverter.ToInt32(tmb.Data, 0)}");
                    break;
                case Members.DataSetOrdinateRange:
                    if (tmb.TypeCode != (short)TypeCodes.CvCoOrdRange)
                        throw new NotSupportedException("Not supported data type for DataSetOrdinateRange.");
                    var f1 = BitConverter.ToDouble(tmb.Data, 0);
                    var f2 = BitConverter.ToDouble(tmb.Data, sizeOfDouble);
                    sp.MetaData.AddRecord("DataSetOrdinateRange", $"{f1} {f2}");
                    break;
                default:
                    if (IncludUnknownBlocksInMetaData)
                        sp.MetaData.AddRecord($"XXX_{(Members)tmb.Id}", $"{tmb.DumpDataAsHex()}");
                    break;
            }
        }

        private static IEnumerable<TypedMemberBlock> ParseMembers(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    TypedMemberBlock tmb;
                    try
                    {
                        tmb = new TypedMemberBlock(binaryReader);
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }
                    yield return tmb;
                }
            }
        }

        private const Blocks mainBlock = Blocks.DSet2DC1DI;
        private const int dataMemberDataOffset = 4;
        private const int sizeOfDouble = 8;
    }
}
