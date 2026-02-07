using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class ScaffoldingMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var scaffolding = component.Cast<Scaffolding>();
            var building = scaffolding.Building;
            if (building == null) return;
            
            var wall = building.GetComponent<Wall>();
            if (wall != null)
            {
                // 使用 Wall 组件作为 target，确保使用正确的位置
                var marker = view.TryAddMapMarker(component, MarkerStyle.Wall.Building.Color, MarkerStyle.Wall.Sign, null);
                if (marker != null)
                {
                    // 添加到 LeftWalls 或 RightWalls 并创建连接线
                    view.WallController.AddWallToList(marker);
                }
            }
            else
            {
                var prefab = component.GetComponent<PrefabID>();
                if (prefab == null) return;
                if (prefab.prefabID == (int)GamePrefabID.Quarry)
                {
                    view.TryAddMapMarker(component, MarkerStyle.Quarry.Building.Color, MarkerStyle.Quarry.Sign, Strings.Quarry);
                }
                else if (prefab.prefabID == (int)GamePrefabID.Mine)
                {
                    view.TryAddMapMarker(component, MarkerStyle.Mine.Building.Color, MarkerStyle.Mine.Sign, Strings.Mine);
                }
            }
        }
    
        [HarmonyPatch(typeof(Scaffolding), nameof(Scaffolding.Awake))]
        private class AwakePatch
        {
            public static void Postfix(Scaffolding __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }
    
        [HarmonyPatch(typeof(Scaffolding), nameof(Scaffolding.CompleteAndRemove))]
        private class CompleteAndRemovePatch
        {
            public static void Prefix(Scaffolding __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
