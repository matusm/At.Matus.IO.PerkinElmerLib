using At.Matus.IO.PerkinElmerSP.Reader;
using System.Globalization;
using System.Reflection;

namespace Sp2Csv
{
    static class Program
    {
        static bool recursiveOption = false;
        static bool overwriteOption = false;
        static bool debuggingOption = false;

        public static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            Console.WriteLine($"This is {Assembly.GetExecutingAssembly().GetName().Name.ToString()} version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            Console.WriteLine();
            recursiveOption = args.Contains("-r");
            overwriteOption = args.Contains("-o");
            debuggingOption = args.Contains("-d");
            Console.WriteLine($"Options:");
            Console.WriteLine($"   Recursive folder processing (-r): {recursiveOption}");
            Console.WriteLine($"   Overwrite existing CSV: (-o): {overwriteOption}");
            Console.WriteLine($"   Include unknown keys (-d): {debuggingOption}");
            Console.WriteLine();
            List<string> files = new List<string>();
            if (args.Length > 0)
            {
                foreach (var item in args)
                {
                    files.AddRange(GetFileOrDir(item));
                }
            }
            else
            {
                files.AddRange(Directory.GetFiles(Environment.CurrentDirectory));
            }

            IEnumerable<string> query = files.Where(f => string.Equals(Path.GetExtension(f), ".sp", StringComparison.OrdinalIgnoreCase)).ToList();            
            if (!overwriteOption)
            {
                query = query.Where(x => !File.Exists(GetCsvOutputFilePath(x)));
            }
            files = query.ToList();
            Console.WriteLine($"Total files to process = {files.Count}.");
            Console.WriteLine();

            foreach (var file in files)
            {
                ProcessFile(file);
            }

            Console.WriteLine("Finished.");
        }

        static void ProcessFile(string inputPath)
        {
            string outputPath = GetCsvOutputFilePath(inputPath);
            string jsonPath = Path.ChangeExtension(inputPath, ".json");
            string textPath = Path.ChangeExtension(inputPath, ".txt");
            try
            {
                var reader = new SpFileTool();
                reader.IncludUnknownBlocksInMetaData = debuggingOption;
                var spectrum = reader.GetData(inputPath);
                using (var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8))
                {
                    spectrum.WriteMetaDataAsComments(writer);
                    spectrum.WriteCsv(writer);
                }
                using (var writer = new StreamWriter(jsonPath, false, System.Text.Encoding.UTF8))
                {
                    spectrum.WriteMetaDataAsJson(writer);
                }
                using (var writer = new StreamWriter(textPath, false, System.Text.Encoding.UTF8))
                {
                    spectrum.WriteGermanText(writer);
                }
                Console.WriteLine($"Successfully converted {inputPath} to {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: for file '{inputPath}', {Environment.NewLine}\t{ex}");
            }
        }

        static string[] GetFileOrDir(string path)
        {
            if (File.Exists(path))
            {
                return new string[] { path };
            }
            else if (Directory.Exists(path))
            {
                return recursiveOption ? Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    : Directory.GetFiles(path);
            }
            else
            {
                return new string[] { };
            }
        }

        static string GetCsvOutputFilePath(string inputPath) => Path.ChangeExtension(inputPath, ".csv");

    }
}

