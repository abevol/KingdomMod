using System;
using System.Globalization;
using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace KingdomMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("KingdomTwoCrowns.exe")]
    public class OverlayMapPlugin : BasePlugin
    {
        public override void Load()
        {
            try
            {
                // debug localization

                // string myCulture = "en-GB";
                // Strings.Culture = CultureInfo.GetCultureInfo(myCulture);
                // CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(myCulture);

                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

                OverlayMap.Initialize(this);
            }
            catch (Exception e)
            {
                Log.LogInfo(e);
                throw;
            }
        }
    }
}