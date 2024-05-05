using Sorbate.Storage.Models;

namespace Sorbate.DiscordBot;

class Program {
    static void Main(string[] args) {
        
        using StorageContext db = new();
        
        Console.WriteLine("Inserting a new file");
        db.Add(new ModFile { InternalModName = "Calamity", ModVersion = "0.1"});
        db.SaveChanges();

        Console.WriteLine("Querying for a file");
        ModFile file = db.Files
            .OrderBy(b => b.InternalModName)
            .First();
        Console.WriteLine($"{file.InternalModName} - {file.ModVersion} - {file.Id}");

        Console.WriteLine("Delete the file");
        db.Remove(file);
        db.SaveChanges();
    }
}