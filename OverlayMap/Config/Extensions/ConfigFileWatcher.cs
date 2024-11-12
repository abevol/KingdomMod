using System;
using System.Collections;
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
        _watcher.Path = Path.Combine(BepInExDir, "config");
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
            Instance.StartCoroutine(OnConfigFileChangedCoroutine(source, e));
        }
        catch (Exception exception)
        {
            LogError($"HResult: {exception.HResult:X}, {exception.Message}");
        }
    }

    private IEnumerator OnConfigFileChangedCoroutine(object source, FileSystemEventArgs e)
    {
        // 等待到下一帧，确保在主线程中执行
        yield return null;

        _changedEventHandler?.Invoke(source, e);
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
                    LogError($"HResult: {e.HResult:X}, {e.Message}");
            }

            retry++;
            System.Threading.Thread.Sleep(10);
        } while (retry < 3);

        return "";
    }
}