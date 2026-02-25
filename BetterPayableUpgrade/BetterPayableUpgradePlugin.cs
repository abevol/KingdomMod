using System;
using BepInEx;
using BepInEx.Logging;

#if IL2CPP
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using KingdomMod.SharedLib.Attributes;
#endif

#if MONO
using BepInEx.Unity.Mono;
#endif

namespace KingdomMod.BetterPayableUpgrade;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("KingdomTwoCrowns.exe")]
public class BetterPayableUpgradePlugin :
#if IL2CPP
    BasePlugin
#else
    BaseUnityPlugin
#endif
{
    public static BetterPayableUpgradePlugin Instance;
    public ManualLogSource LogSource
#if IL2CPP
        => Log;
#else
        => Logger;
#endif

#if IL2CPP
    public override void Load()
    {
        RegisterTypeInIl2Cpp.RegisterAssembly(Assembly.GetExecutingAssembly());

        Init();
    }
#else
    internal void Awake()
    {
        Init();
    }
#endif

    private void Init()
    {
        try
        {
            Instance = this;

            LogSource.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            BetterPayableUpgradeHolder.Initialize(this);
        }
        catch (Exception e)
        {
            LogSource.LogInfo(e);
            throw;
        }
    }
}