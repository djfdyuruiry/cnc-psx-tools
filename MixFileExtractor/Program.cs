const string outputDir = "DATA";

if (Directory.Exists(outputDir))
{
    Directory.Delete(outputDir, true);
}

Directory.CreateDirectory(outputDir);

using (var mixFile = File.OpenRead("DATA.MIX"))
using (var fileListingReader = new StreamReader("file-listing.txt"))
{
    var counter = 0;
    var line = await fileListingReader.ReadLineAsync();

    while (!string.IsNullOrWhiteSpace(line))
    {
        var record = line.Split(",");

        var currentFileName = record[0];
        var offset = UInt32.Parse(record[1]) * 2048;
        var length = UInt32.Parse(record[2]);

        var fileExtension = currentFileName.Split('.')[1];

        var fileByteBuffer = new byte[length];

        mixFile.Position = offset;
        await mixFile.ReadAsync(fileByteBuffer, 0, fileByteBuffer.Length);

        using (var currentFile = File.OpenWrite($"{outputDir}/{currentFileName}"))
        {
            await currentFile.WriteAsync(fileByteBuffer);
        }

        var hexFileName = $"{counter.ToString("X").PadLeft(8, '0')}.{fileExtension}";

        using (var hexFile = File.OpenWrite($"{outputDir}/{hexFileName}"))
        {
            await hexFile.WriteAsync(fileByteBuffer);
        }

        var paddedLength = length.ToString().PadLeft(8, '0');
        var paddedOffset = offset.ToString().PadLeft(8, '0');

        Console.WriteLine($"{currentFileName.PadRight(12)}: Read {paddedLength} bytes from offset {paddedOffset}");
        //Console.WriteLine($"{hexFileName.PadRight(12)}: Read {paddedLength} bytes from offset {paddedOffset}");

        line = await fileListingReader.ReadLineAsync();
        counter++;
    }
}
