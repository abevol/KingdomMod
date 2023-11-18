using System;
using System.Globalization;
using BepInEx;
using BepInEx.Logging;

#if IL2CPP
using BepInEx.Unity.IL2CPP;
#endif

#if MONO
using BepInEx.Unity.Mono;
#endif

namespace KingdomMod
{
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

                OverlayMap.Initialize(this);
            }
            catch (Exception e)
            {
                LogSource.LogInfo(e);
                throw;
            }
        }
    }
}