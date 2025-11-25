using KingdomMod.OverlayMap.Gui.TopMap;
using KingdomMod.Shared.Attributes;
using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui
{
    [RegisterTypeInIl2Cpp]
    public class GuiStyle : MonoBehaviour
    {
        public TopMapStyle topMapStyle;

#if IL2CPP
        public GuiStyle(IntPtr ptr) : base(ptr) { }
#endif

        private void Awake()
        {
            LogTrace($"GuiStyle.Awake");
            topMapStyle = CreateTopMapStyle();
        }

        private TopMapStyle CreateTopMapStyle()
        {
            LogDebug($"CreateTopMapStyle");

            var guiObj = new GameObject(nameof(TopMapStyle));
            guiObj.transform.SetParent(this.transform, false);
            var guiComp = guiObj.AddComponent<TopMapStyle>();

            return guiComp;
        }
    }
}
