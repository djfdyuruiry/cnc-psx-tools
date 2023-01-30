using System.Text.Json;

using CncPsxLib;

var reader = new FatFileReader();
var file = reader.Read("DATA.FAT");

Console.WriteLine(
    JsonSerializer.Serialize(
        file.FileEntries, 
        new JsonSerializerOptions { WriteIndented = true }
    )
);

Console.WriteLine("DONE");
