using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;

using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 墙体标记映射器。
    /// </summary>
    public class WallMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Wall;

        public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
        {
            if (resolverType == ResolverType.Scaffolding)
            {
                LogWarning($"Mapping scaffolding component: {component}");
                var scaffolding = component.Cast<Scaffolding>();
                var building = scaffolding.Building;
                if (building == null) return;

                if (building.gameObject.tag == Tags.Wall)
                {
                    var marker = view.TryAddMapMarker(component, MarkerStyle.Wall.Building.Color, MarkerStyle.Wall.Sign, null);
                    if (marker != null)
                    {
                        // 添加到 LeftWalls 或 RightWalls 并创建连接线
                        view.WallController.AddWallToList(marker);
                    }
                }
            }
            else if (resolverType == ResolverType.PayableUpgrade)
            {
                var payableUpgrade = component.Cast<PayableUpgrade>();
                var prefabId = payableUpgrade.gameObject.GetComponent<PrefabID>();
                if (prefabId == null) return;
                var gamePrefabId = (GamePrefabID)prefabId.prefabID;
                switch (gamePrefabId)
                {
                    case GamePrefabID.Wall0:
                        // 墙体地基
                        view.TryAddMapMarker(component, MarkerStyle.WallFoundation.Color, MarkerStyle.WallFoundation.Sign, null);
                        break;

                    case GamePrefabID.Wall1:
                    case GamePrefabID.Wall2:
                    case GamePrefabID.Wall3:
                    case GamePrefabID.Wall4:
                    case GamePrefabID.Wall5:
                        // 正常墙体
                        var marker = view.TryAddMapMarker(component, MarkerStyle.Wall.Color, MarkerStyle.Wall.Sign, null);
                        if (marker != null)
                        {
                            view.WallController.AddWallToList(marker);  // 添加到 LeftWalls 或 RightWalls 并创建连接线
                        }
                        break;

                    case GamePrefabID.Wall1_Wreck:
                    case GamePrefabID.Wall2_Wreck:
                    case GamePrefabID.Wall3_Wreck:
                    case GamePrefabID.Wall4_Wreck:
                    case GamePrefabID.Wall5_Wreck:
                        // 损坏的墙体
                        var wallWreckMarker = view.TryAddMapMarker(component, MarkerStyle.Wall.Wrecked.Color, MarkerStyle.Wall.Sign, null);
                        if (wallWreckMarker != null)
                        {
                            view.WallController.AddWallToList(wallWreckMarker);  // 添加到 LeftWalls 或 RightWalls 并创建连接线
                        }
                        break;
                }
            }
        }
    }
}