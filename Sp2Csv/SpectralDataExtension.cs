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
