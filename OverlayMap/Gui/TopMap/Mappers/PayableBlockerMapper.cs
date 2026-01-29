using HarmonyLib;
using Il2CppInterop.Runtime;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using KingdomMod.SharedLib;
using System.Collections.Generic;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableBlockerMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var obj = component.Cast<PayableBlocker>();
            var prefabId = obj.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return;
            switch ((GamePrefabID)prefabId.prefabID)
            {
                case GamePrefabID.Quarry:
                    view.TryAddMapMarker(component, null, MarkerStyle.Quarry.Sign, Strings.Quarry, null,
                        comp => comp.gameObject.activeSelf ? MarkerStyle.Quarry.Unlocked.Color : MarkerStyle.Quarry.Building.Color);
                    return;
            }
        }

        private static readonly Il2CppSystem.Type[] PuzzleTypes =
        [
            Il2CppType.Of<CerberusPuzzleController>(),
            Il2CppType.Of<ChariotPuzzleController>(),
            Il2CppType.Of<HeimdallPuzzleController>(),
            Il2CppType.Of<HelPuzzleController>(),
            Il2CppType.Of<ThorPuzzleController>()
        ];

        [HarmonyPatch(typeof(PayableBlocker), nameof(PayableBlocker.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(PayableBlocker __instance)
            {
                Component target = null;
                foreach (var type in PuzzleTypes)
                {
                    target = __instance.GetComponent(type);
                    if (target != null) break;
                }
                target ??= __instance;
                ForEachTopMapView(view => view.OnComponentCreated(target));
            }
        }

        [HarmonyPatch(typeof(PayableBlocker), nameof(PayableBlocker.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(PayableBlocker __instance)
            {
                Component target = null;
                foreach (var type in PuzzleTypes)
                {
                    target = __instance.GetComponent(type);
                    if (target != null) break;
                }
                target ??= __instance;
                ForEachTopMapView(view => view.OnComponentDestroyed(target));
            }
        }
    }
}