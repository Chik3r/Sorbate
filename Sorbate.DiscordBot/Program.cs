using Sorbate.Storage;
using Sorbate.Storage.Models;

namespace Sorbate.DiscordBot;

class Program {
    static async Task Main(string[] args) {
        byte[] randomData = new byte[1024];
        Random.Shared.NextBytes(randomData);
        using MemoryStream stream = new(randomData);

        await StorageHandler.Instance.StoreFile(stream, new ModFile {
            InternalModName = "MyMod",
            ModVersion = "1.0.0",
            ModLoaderVersion = "0.11.7.5"
        }, "C:/tmp/sorbate/mods/");
        
        
        // using StorageContext db = new();
        //
        // int howMany = db.Files.Count();
        // Console.WriteLine($"{howMany} files in the database");
        //
        // Console.WriteLine("Inserting a new file");
        // db.Add(new ModFile { InternalModName = "Calamity", ModVersion = "0.1"});
        // db.SaveChanges();
        //
        // Console.WriteLine("Querying for a file");
        // ModFile file = db.Files
        //     .OrderBy(b => b.InternalModName)
        //     .First();
        // Console.WriteLine($"{file.InternalModName} - {file.ModVersion} - {file.Id}");
        //
        // // Console.WriteLine("Delete the file");
        // // db.Remove(file);
        // // db.SaveChanges();
    }
}