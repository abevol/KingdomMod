using System;
using System.IO;
using System.Security.Cryptography;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Config.Extensions;

public class ConfigFileWatcher
{
    private readonly FileSystemWatcher _watcher = new();
    private string _configFileHash;
    private FileSystemEventHandler _changedEventHandler;

    public void Set(string fileName, FileSystemEventHandler changed)
    {
        _watcher.Path = Path.Combine(GetBepInExDir(), "config");
        _watcher.NotifyFilter = NotifyFilters.LastWrite;
        _watcher.Filter = fileName ?? "*.cfg";
        _watcher.Changed += OnConfigFileChanged;
        _watcher.IncludeSubdirectories = false;
        _watcher.EnableRaisingEvents = true;
        _changedEventHandler = changed;
    }

    private void OnConfigFileChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            var hash = GetFileHash(e.FullPath);
            if (hash == "") return;
            if (hash == _configFileHash) return;
            _configFileHash = hash;
            _changedEventHandler?.Invoke(source, e);
        }
        catch (Exception exception)
        {
            LogMessage($"HResult: {exception.HResult:X}, {exception.Message}");
        }
    }

    public static string GetFileHash(string filename)
    {
        int retry = 0;
        do
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
                    }
                }
            }
            catch (Exception e)
            {
                if ((uint)e.HResult != 0x80070020)
                    LogMessage($"HResult: {e.HResult:X}, {e.Message}");
            }

            retry++;
            System.Threading.Thread.Sleep(10);
        } while (retry < 3);

        return "";
    }
}