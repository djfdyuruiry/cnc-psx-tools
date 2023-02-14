using System.Text.RegularExpressions;

using CommandLine;

using CncPsxLib;  

namespace MixFileExtractor
{
    internal static class Program
    {
        private static async Task ExtractFile(
            MixFile mixFile,
            string fileName,
            FatFileEntry entry,
            string outputPath
        )
        {
            var fileBytes = await mixFile.ReadFile(entry);

            using (var currentFile = File.OpenWrite($"{outputPath}/{fileName}"))
            {
                await currentFile.WriteAsync(fileBytes);
            }

            var paddedSize = entry.SizeInBytes.ToString().PadLeft(8, '0');
            var paddedOffset = entry.OffsetInBytes.ToString().PadLeft(8, '0');

            await Console.Out.WriteLineAsync(
                $"{fileName.PadRight(12)}: Read {paddedSize} bytes from offset {paddedOffset}"
            );
        }

        private static bool ShouldExtractFile(
            IEnumerable<Regex> filesToExtract,
            IEnumerable<Regex> filesToIgnore,
            string fileName
        ) =>
            filesToExtract.Any(r => r.IsMatch(fileName))
            && !filesToIgnore.Any(r => r.IsMatch(fileName));

        private static async Task ExtractMixFileEntries(
            MixFile mixFile,
            IEnumerable<Regex> filesToExtract,
            IEnumerable<Regex> filesToIgnore,
            List<FatFileEntry> fileEntries,
            string outputPath
        )
        {
            var fileNamesExtracted = new Dictionary<string, bool>();

            foreach (var entry in fileEntries)
            {
                var sanitisedFileName = entry.FileName;

                if (fileNamesExtracted.ContainsKey(sanitisedFileName))
                {
                    // detect duplicate filename entries
                    sanitisedFileName = sanitisedFileName.Replace(".", "-1.");
                }

                if (!ShouldExtractFile(filesToExtract, filesToIgnore, sanitisedFileName))
                {
                    continue;
                }

                await ExtractFile(mixFile, sanitisedFileName, entry, outputPath);
                fileNamesExtracted[sanitisedFileName] = true;
            }
        }

        private static async Task<int> Run(CliOptions opts)
        {
            var fatFileReader = new FatFileReader();
            var fatFile = await fatFileReader.Read(opts.FatFilePathOrDefault);

            Directory.CreateDirectory(opts.OutputPathOrDefault);

            var entries = fatFile.MixFileEntries;
            var filesToExtract = opts.BuildExtractPatterns();
            var filesToIgnore = opts.BuildIgnorePatterns();

            if (opts.MixFileIsXaData)
            {
                entries = fatFile.XaFileEntries;
            }

            using (var mixFile = MixFile.Open(opts.MixFilePath))
            {
                await ExtractMixFileEntries(
                    mixFile, filesToExtract, filesToIgnore, entries, opts.OutputPathOrDefault
                );
            }

            return 0;
        }

        private static async Task<int> Main(string[] args) =>
            await Parser.Default
                .ParseArguments<CliOptions>(args)
                .MapResult(
                    Run,
                    errs => Task.FromResult(-1)
                );
    }

}
