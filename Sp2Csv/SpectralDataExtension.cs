// ============================================================================
// File: SpectralDataExtension.cs
// Project: Sp2Csv
// Description: Extension methods for the Spectrum2d class providing CSV export
//              functionality formatted for German locale Excel compatibility,
//              with optional sorting of spectral data points.
// 
// Copyright (c) 2026 Michael Matus
// ============================================================================

using At.Matus.IO.PerkinElmerSP.Reader;
using System.Globalization;

namespace Sp2Csv
{
    public static class SpectralDataExtension
    {
        public static void WriteGermanTextForExcel(this Spectrum2d spectrum, TextWriter w, bool sort = true)
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            if (sort)
                Array.Sort(spectrum.Points);
            foreach (var point in spectrum.Points)
                w.WriteLine($"{point.X} {point.Y:F4}");
        }
    }
}
