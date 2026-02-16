using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class WorkableBuildingMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var workableBuilding = component.Cast<WorkableBuilding>();
            if (workableBuilding == null) return;
            if (workableBuilding.gameObject.tag == Tags.Tholos)
            {
                view.TryAddMapMarker(component, MarkerStyle.Tholos.Color, MarkerStyle.Tholos.Sign, Strings.Tholos);
            }
        }

        [HarmonyPatch(typeof(WorkableBuilding), nameof(WorkableBuilding.Start))]
        private class StartPatch
        {
            public static void Postfix(WorkableBuilding __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(WorkableBuilding), nameof(WorkableBuilding.OnDestroy))]
        private class OnDestroyPatch
        {
            public static void Prefix(WorkableBuilding __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
