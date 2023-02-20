using System.Text.RegularExpressions;

using CommandLine;

using CncPsxLib;  

namespace MixFileExtractor
{
    internal static class Program
    {
        private static async Task ExtractFile(
            MixFileReader mixFile,
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
            MixFileReader mixFile,
            IEnumerable<Regex> filesToExtract,
            IEnumerable<Regex> filesToIgnore,
            List<FatFileEntry> fileEntries,
            string outputPath
        )
        {
            var fileNamesExtracted = new Dictionary<string, int>();

            foreach (var entry in fileEntries)
            {
                var sanitisedFileName = entry.FileName;

                if (fileNamesExtracted.ContainsKey(sanitisedFileName))
                {
                    // allow duplicate filename entries
                    sanitisedFileName = sanitisedFileName.Replace(
                        ".",
                        $"-{fileNamesExtracted[sanitisedFileName]}."
                    );
                }

                if (!ShouldExtractFile(filesToExtract, filesToIgnore, sanitisedFileName))
                {
                    continue;
                }

                await ExtractFile(mixFile, sanitisedFileName, entry, outputPath);

                fileNamesExtracted[sanitisedFileName] =
                    fileNamesExtracted.ContainsKey(sanitisedFileName) ?
                    fileNamesExtracted[sanitisedFileName]++ :
                    1;
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

            // var manager = new MixFileManager(fatFile, opts.MixFilePath);

            // var entry = fatFile.MixFileEntries.First(e => e.FileName == "SCB01EA.INI");

            // using (var entryStream = File.OpenRead(@"C:\Users\Matthew\Downloads\SCB01EA.INI"))
            // {
            //     await manager.ReplaceFile(entry, entryStream);
            // }

            using (var mixFile = MixFileReader.Open(opts.MixFilePath))
            {
                await ExtractMixFileEntries(
                    mixFile, filesToExtract, filesToIgnore, entries, opts.OutputPathOrDefault
                );
            }

            //using (var mixFile = MixFileWriter.Open("DATA.MIX"))
            //{
            //    foreach (var entry in entries)
            //    {
            //        var fileName = entry.FileName;
            //        var sanitizedFileName = fileName.Replace(
            //            ".",
            //            $"-1."
            //        );

            //        if (!File.Exists(fileName) && File.Exists(sanitizedFileName))
            //        {
            //            fileName = sanitizedFileName;
            //        }

            //        using (var entryData = File.OpenRead($"DATA/{fileName}"))
            //        {
            //            await mixFile.WriteFile(entry, entryData);
            //        }
            //    }    
            //}

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
