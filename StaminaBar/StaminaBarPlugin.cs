using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace KingdomMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("KingdomTwoCrowns.exe")]
    public class StaminaBarPlugin : BasePlugin
    {
        public override void Load()
        {
            try
            {
                // Plugin startup logic
                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

                StaminaBar.Initialize(this);
            }
            catch (Exception e)
            {
                Log.LogInfo(e);
                throw;
            }
        }
    }
}