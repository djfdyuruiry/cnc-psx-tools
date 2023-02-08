namespace CncPsxLib
{
    public class FatFile
    {
        public string Path { get; set; }

        public Dictionary<string, FatFileEntry> FileEntries { get; set; }

        public Dictionary<string, FatFileEntry> ExtraFileEntries { get; set; }

        public int EntryCount => FileEntries.Count;

        public int ExtraEntryCount => ExtraFileEntries.Count;
    }
}
