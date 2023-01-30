using CncPsxLib;

const string outputDir = "DATA";

var fatFileReader = new FatFileReader();
var fatFile = await fatFileReader.Read("DATA.FAT");

if (Directory.Exists(outputDir))
{
    Directory.Delete(outputDir, true);
}

Directory.CreateDirectory(outputDir);

using (var mixFile = File.OpenRead("DATA.MIX"))
{
    foreach (var (fileName, entry) in fatFile.FileEntries)
    {
        var fileByteBuffer = new byte[entry.SizeInBytes];

        mixFile.Position = entry.OffsetInBytes;
        await mixFile.ReadAsync(fileByteBuffer, 0, fileByteBuffer.Length);

        using (var currentFile = File.OpenWrite($"{outputDir}/{fileName}"))
        {
            await currentFile.WriteAsync(fileByteBuffer);
        }

        var paddedSize = entry.SizeInBytes.ToString().PadLeft(8, '0');
        var paddedOffset = entry.OffsetInBytes.ToString().PadLeft(8, '0');

        await Console.Out.WriteLineAsync(
            $"{fileName.PadRight(12)}: Read {paddedSize} bytes from offset {paddedOffset}"
        );
    }
}
