namespace CncPsxLib
{
    public class MixFileReader : IDisposable
    {
        public FileStream MixStream;

        public string Path { get; }

        public static MixFileReader Open(string path) => new(path);

        private MixFileReader(string path)
        {
            Path = path;

            MixStream = File.OpenRead(Path);
        }

        public async Task<byte[]> ReadFile(FatFileEntry mixFileEntry)
        {
            MixStream.Position = mixFileEntry.OffsetInBytes;

            var (readOk, fileByteBuffer) = await MixStream.ReadExactlyAsync((int)mixFileEntry.SizeInBytes);

            if (!readOk)
            {
                throw new InvalidDataException("Corrupt MIX file or FAT/MIX mismatch: data for file entry missing");
            }

            return fileByteBuffer;
        }

        public void Dispose() => MixStream.Dispose();
    }
}
