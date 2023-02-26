using CommandLine;

namespace FatFileParser
{
#pragma warning disable CS8618 
    internal class CliOptions
    {
        [Option('p', "path", Required = true, HelpText = "Path to a FAT file from a PS1 C&C game.")]
        public string FatFilePath { get; set; }

        [Option('y', "yaml", Default = false, HelpText = "Output FAT file entries in YAML format.")]
        public bool OutputYaml { get; set; }
    }
#pragma warning restore CS8618 
}
