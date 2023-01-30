using CommandLine.Text;
using CommandLine;

namespace FatFileParser
{
    internal class CliOptions
    {
        [Option('p', "path", Required = true, HelpText = "Path to a FAT file from a PS1 C&C game.")]
        public string FatFilePath { get; set; }

        [Option('y', "yaml", Default = false, HelpText = "Output FAT file entries in YAML format.")]
        public bool OutputYaml { get; set; }
    }
}
