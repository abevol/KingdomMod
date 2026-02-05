using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using UnityEngine;

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
                var marker = view.TryAddMapMarker(wall, MarkerStyle.Wall.Building.Color, MarkerStyle.Wall.Sign, null);
                if (marker != null)
                {
                    // 添加到 LeftWalls 或 RightWalls 并创建连接线
                    view.AddWallToList(marker);
                }
            }
        }
    
        [HarmonyPatch(typeof(Scaffolding), nameof(Scaffolding.Awake))]
        private class AwakePatch
        {
            public static void Postfix(Scaffolding __instance)
            {
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }
    
        [HarmonyPatch(typeof(Scaffolding), nameof(Scaffolding.CompleteAndRemove))]
        private class CompleteAndRemovePatch
        {
            public static void Prefix(Scaffolding __instance)
            {
                var building = __instance.Building;
                if (building != null)
                {
                    var wall = building.GetComponent<Wall>();
                    if (wall != null)
                    {
                        // 使用 Wall 组件来清理 MapMarker（因为创建时用的是 Wall）
                        OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(wall));
                        return;
                    }
                }
                
                // 如果 Building 为 null，使用 Scaffolding 自己（兜底逻辑）
                OverlayMapHolder.ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}
