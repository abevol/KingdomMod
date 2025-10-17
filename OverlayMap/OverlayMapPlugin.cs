using System;
using System.Globalization;
using BepInEx;
using BepInEx.Logging;
using KingdomMod.Shared.Attributes;

#if IL2CPP
using System.Reflection;
using BepInEx.Unity.IL2CPP;
#endif

#if MONO
using BepInEx.Unity.Mono;
#endif

namespace KingdomMod.OverlayMap;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("KingdomTwoCrowns.exe")]
public class OverlayMapPlugin :
#if IL2CPP
    BasePlugin
#else
    BaseUnityPlugin
#endif
{
    public static OverlayMapPlugin Instance;
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
            // debug localization

            // string myCulture = "en-GB";
            // Strings.Culture = CultureInfo.GetCultureInfo(myCulture);
            // CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(myCulture);

            Instance = this;

            LogSource.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            OverlayMapHolder.Initialize(this);
        }
        catch (Exception e)
        {
            LogSource.LogInfo(e);
            throw;
        }
    }
}