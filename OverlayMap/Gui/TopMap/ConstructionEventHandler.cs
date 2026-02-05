#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using KingdomMod.Shared.Attributes;
#endif

using System;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    [RegisterTypeInIl2Cpp]
    public class ConstructionEventHandler : MonoBehaviour
    {
        private MapMarker _owner;
        private RectTransform _rectTransform;
        private Color _normalColor;
        private Color _constructionalColor;
        private ConstructionBuildingComponent _constructionBuildingComponent;

#if IL2CPP
        public ConstructionEventHandler(IntPtr ptr) : base(ptr) { }
#endif

        public static void Create(MapMarker marker, Color normalColor, Color constructionalColor)
        {
            GameObject obj = new GameObject(nameof(ConstructionEventHandler));
            obj.transform.SetParent(marker.transform, false);
            var comp = obj.AddComponent<ConstructionEventHandler>();
            comp.Init(normalColor, constructionalColor);
        }

        private void Init(Color normalColor, Color constructionalColor)
        {
            LogInfo("Init");
            _rectTransform = this.gameObject.AddComponent<RectTransform>();
            _owner = _rectTransform.parent.GetComponent<MapMarker>();
            _normalColor = normalColor;
            _constructionalColor = constructionalColor;
        }

        private void Awake()
        {
            LogInfo("Awake");
        }

        private void Start()
        {
            LogInfo("Start");
            _constructionBuildingComponent = _owner.Data.Target.GetComponent<ConstructionBuildingComponent>();
            if (_constructionBuildingComponent != null)
            {
                LogInfo("Found ConstructionBuildingComponent, subscribing to events");
                _constructionBuildingComponent.OnManualConstructionStarted += (System.Action)OnManualConstructionStarted;
                _constructionBuildingComponent.OnConstructionComplete += (System.Action)OnConstructionComplete;
            }
        }

        public void OnManualConstructionStarted()
        {
            LogInfo("Manual construction started");
            _constructionBuildingComponent.OnManualConstructionStarted -= (System.Action)OnManualConstructionStarted;

            // 更新 marker 颜色
            _owner.UpdateColor(_constructionalColor);
        }

        public void OnConstructionComplete()
        {
            LogInfo("Construction complete");
            _constructionBuildingComponent.OnConstructionComplete -= (System.Action)OnConstructionComplete;

            // 更新 marker 颜色
            _owner.UpdateColor(_normalColor);
        }
    }
}