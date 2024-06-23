﻿using Microsoft.EntityFrameworkCore;
using Sorbate.Storage.Analyzers;
using Sorbate.Storage.Models;
using Tomat.FNB.TMOD;

namespace Sorbate.Storage;

// TODO
// - Store the SHA1 hash of the tmod file, prevents repeated files by checking if they are already stored
// - logging
// - store the author of the mod

public class StorageHandler {
    internal const string ModFileDirectory = "tmod_files";
    
    private readonly HttpClient _client;
    private readonly AnalyzerService _analyzer;
    private readonly IDbContextFactory<StorageContext> _dbFactory;

    public StorageHandler(HttpClient client, AnalyzerService analyzer, IDbContextFactory<StorageContext> dbFactory) {
        _client = client;
        _analyzer = analyzer;
        _dbFactory = dbFactory;
    }

    public async Task StoreFile(Stream fileData, ModFile fileInfo, string outputDirectory) {
        await using StorageContext db = await _dbFactory.CreateDbContextAsync();
        if (db.Files.Any(x => x.FileHash == fileInfo.FileHash)) {
            // File already exists
            return;
        }

        Directory.CreateDirectory(outputDirectory);

        Guid fileGuid = Guid.NewGuid();
        fileInfo.Id = fileGuid;
        string fileName = fileGuid.ToString();
        string filePath = Path.Combine(outputDirectory, fileName);

        await using FileStream fileStream = File.Create(filePath);
        fileData.Position = 0;
        await fileData.CopyToAsync(fileStream);

        db.Files.Add(fileInfo);
        await db.SaveChangesAsync();
    }

    public async Task StoreModFile(Stream modFile) => await StoreModFile(modFile, ModFileDirectory);

    public async Task StoreModFile(Stream modFileStream, string outputDirectory) {
        if (!TmodFile.TryReadFromStream(modFileStream, out TmodFile? tmodFile))
            throw new Exception("Unable to read .tmod file from stream");

        ModFile modInfo = new() {
            Id = new Guid(),
            InternalModName = tmodFile.Name,
            ModVersion = tmodFile.Version,
            ModLoaderVersion = tmodFile.ModLoaderVersion
        };

        modInfo = await _analyzer.AnalyzeMod(modFileStream, modInfo, tmodFile);
        
        await StoreFile(modFileStream, modInfo, outputDirectory);
    }

    public async Task<Stream> DownloadModFile(string url) {
        await using Stream stream = await _client.GetStreamAsync(url);
        
        // Copy to a memory stream to ensure it is fully downloaded
        MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }
}