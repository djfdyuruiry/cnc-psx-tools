using CommandLine;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

using CncPsxLib;

namespace FatFileParser
{ 
    internal static class Program
    {
        private static async Task OutputFatEntriesAsTable(Dictionary<string, FatFileEntry> fileEntries)
        {   
            await Console.Out.WriteLineAsync(
                @"┌──────────────┬────────────────┬──────────────┐
                  │ File Name    │ Offset         │ Size         │
                  ├──────────────┼────────────────┼──────────────┤".StripLeadingWhitespace()
            );

            foreach (var (_, entry) in fileEntries)
            {
                await Console.Out.WriteLineAsync(
                    $"│ {entry.FileName,-12} " +
                    $"│ 0x{entry.HexOffsetInBytes}     " +
                    $"│ {entry.SizeInBytes.FormatAsByteUnit(),-12} │"
                );
            }

            await Console.Out.WriteLineAsync("└──────────────┴────────────────┴──────────────┘");
        }
 
        private static async Task OutputFatFileAsTable(FatFile fatFile)
        {
            await Console.Out.WriteLineAsync($"File Path:   {fatFile.Path}");
            await Console.Out.WriteLineAsync($"Entry Count: {fatFile.EntryCount}\n");

            await OutputFatEntriesAsTable(fatFile.FileEntries);

            // TODO: could have cli switch to show extra entries
        }

        private static async Task<int> Run(CliOptions opts)
        {
            try
            {
                var fileReader = new FatFileReader();
                var fatFile = await fileReader.Read(opts.FatFilePath);

                if (opts.OutputYaml)
                {
                    var yamlSerializer = new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    await Console.Out.WriteLineAsync(
                        yamlSerializer.Serialize(fatFile)
                    );
                }
                else
                {
                    await OutputFatFileAsTable(fatFile);
                }
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.Message);
                return -1;
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
    }}
