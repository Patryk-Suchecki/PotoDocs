using PotoDocs.API.Services;

namespace PotoDocs.API.Utils;

public class FileTransactionScope(IFileStorageService fileStorage) : IAsyncDisposable
{
    private readonly List<(string Path, string FileName)> _filesToRollback = [];
    private bool _committed = false;

    public void RegisterCreatedFile(string path, string fileName)
    {
        _filesToRollback.Add((path, fileName));
    }

    public void Complete()
    {
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed && _filesToRollback.Count > 0)
        {
            foreach (var (path, fileName) in _filesToRollback)
            {
                fileStorage.DeleteFile(path, fileName);
            }
        }
        await Task.CompletedTask;
    }
}