using CncPsxLib;
using CommandLine;

using static CncPsxLib.DirectoryExtensions;

namespace MixFileExtractor
{
    internal static class Program
    {
        private static async Task<int> Run(CliOptions opts)
        {
            var fatFileReader = new FatFileReader();
            var fatFile = await fatFileReader.Read(opts.FatFilePathOrDefault);

            EnsureDirectoryExists(opts.OutputPathOrDefault);

            using (var mixFile = MixFile.Open(opts.MixFilePath))
            {
                foreach (var (fileName, entry) in fatFile.FileEntries)
                {
                    var fileBytes = await mixFile.ReadFile(entry);

                    using (var currentFile = File.OpenWrite($"{opts.OutputPathOrDefault}/{fileName}"))
                    {
                        await currentFile.WriteAsync(fileBytes);
                    }

                    var paddedSize = entry.SizeInBytes.ToString().PadLeft(8, '0');
                    var paddedOffset = entry.OffsetInBytes.ToString().PadLeft(8, '0');

                    await Console.Out.WriteLineAsync(
                        $"{fileName.PadRight(12)}: Read {paddedSize} bytes from offset {paddedOffset}"
                    );
                }
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
