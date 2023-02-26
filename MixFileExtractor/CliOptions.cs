using System.Text.RegularExpressions;

using CommandLine;

namespace MixFileExtractor
{
#pragma warning disable CS8618 
    internal class CliOptions
    {
        [Option(
            'p',
            "path",
            Required = true,
            HelpText = "Path to a MIX/XA file from a PS1 C&C game; XA files are MIX files but contain audio/video data."
        )]
        public string MixFilePath { get; set; }

        public bool MixFileIsXaData => Path.GetExtension(MixFilePath).ToUpper() == ".XA";

        public string MixFileName => Path.GetFileNameWithoutExtension(MixFilePath);

        [Option(
            'f',
            "fat-path",
            HelpText = "Path to a FAT file that matches the MIX/XA file provided in --path. Defaults " +
            "to MIX file name, e.g. '--path DATA.MIX' will default to 'DATA.FAT'."
        )]
        public string? FatFilePath { get; set; }

        public string FatFilePathOrDefault =>
            FatFilePath ?? Path.Combine(
                Path.GetDirectoryName(MixFilePath)!,
                $"{MixFileName}.FAT"
            );

        [Option(
            'e',
            "extract-files",
            Default = new string[] { "*" },
            Separator = ',',
            HelpText = "CSV list of filenames to extract from MIX/XA file, supports wildcards. " +
            "Defaults to '*', which extracts all files."
        )]
        public IEnumerable<string> FilesToExtract { get; set; }

        [Option(
            'i',
            "ignore-files",
            Separator = ',',
            HelpText = "CSV list of filenames to ignore when extracting files, supports " +
            "wildcards."
        )]
        public IEnumerable<string> FilesToIgnore { get; set; }

        [Option(
            'o',
            "output-path",
            HelpText = "Directory to write extract files to. Defaults to MIX/XA file name, " +
            "e.g. '--path DATA.XA' will default to writing in the directory 'DATA'."
        )]
        public string? OutputPath { get; set; }

        public string OutputPathOrDefault => OutputPath ?? MixFileName;

        public IEnumerable<Regex> BuildExtractPatterns() =>
            FilesToExtract
                .Select(f => f.Trim().Replace(".", "[.]").Replace("*", ".+"))
                .Select(f => new Regex(f));

        public IEnumerable<Regex> BuildIgnorePatterns() =>
            FilesToIgnore
                .Select(f => f.Trim().Replace(".", "[.]").Replace("*", ".+"))
                .Select(f => new Regex(f));
    }
#pragma warning restore CS8618 
}
