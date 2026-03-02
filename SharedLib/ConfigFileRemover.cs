using System;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;

namespace KingdomMod.SharedLib;

/// <summary>
/// Utility class for dynamically removing a ConfigFile from BepInExConfigManager
/// without requiring a compile-time reference to BepInExConfigManager.Patcher.
///
/// Usage: Copy this class into your own plugin project, then call:
///   ConfigFileRemover.RemoveConfigFile(myConfigFile);
/// </summary>
public static class ConfigFileRemover
{
    private static bool _resolved;
    private static MethodInfo _removeMethod;

    /// <summary>
    /// Remove a ConfigFile from BepInExConfigManager's tracked list.
    /// Uses reflection to locate and invoke Patcher.RemoveConfigFile() at runtime.
    /// Safe to call even if BepInExConfigManager is not installed.
    /// </summary>
    /// <returns>true if the call succeeded; false if Patcher was not found or invocation failed.</returns>
    public static bool RemoveConfigFile(ConfigFile configFile)
    {
        if (configFile == null)
            return false;

        if (!_resolved)
            Resolve();

        if (_removeMethod == null)
            return false;

        try
        {
            _removeMethod.Invoke(null, new object[] { configFile });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void Resolve()
    {
        _resolved = true;
        try
        {
            Type patcherType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(asm =>
                {
                    try { return asm.GetType("Patcher", false); }
                    catch { return null; }
                })
                .FirstOrDefault(t => t != null
                                     && t.IsAbstract && t.IsSealed  // static class
                                     && t.GetMethod("RemoveConfigFile",
                                         BindingFlags.Public | BindingFlags.Static,
                                         null, new[] { typeof(ConfigFile) }, null) != null);

            _removeMethod = patcherType?.GetMethod("RemoveConfigFile",
                BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(ConfigFile) }, null);
        }
        catch
        {
            _removeMethod = null;
        }
    }
}
