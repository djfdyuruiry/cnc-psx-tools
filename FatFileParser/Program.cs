using CncPsxLib;
using System.Text.Json;

var reader = new FatFileReader();
var file = reader.Read("DATA.FAT");

Console.WriteLine(
    JsonSerializer.Serialize(
        file.FileEntries, 
        new JsonSerializerOptions { WriteIndented = true }
    )
);

Console.WriteLine("DONE");
