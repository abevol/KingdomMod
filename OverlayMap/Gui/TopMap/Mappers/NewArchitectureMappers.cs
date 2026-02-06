using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 灯塔标记映射器。
    /// 这是新架构的示例：通过 MapMarkerType.Lighthouse 独立定义，不依赖游戏类型。
    /// </summary>
    public class LighthouseMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Lighthouse;

        public void Map(Component component)
        {
            var payableUpgrade = component.Cast<PayableUpgrade>();
            var prefabId = payableUpgrade.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return;

            var gamePrefabId = (GamePrefabID)prefabId.prefabID;

            switch (gamePrefabId)
            {
                case GamePrefabID.Lighthouse_undeveloped:
                    // 未开发状态：显示价格
                    view.TryAddMapMarker(component, MarkerStyle.Lighthouse.Unpaid.Color, MarkerStyle.Lighthouse.Sign, Strings.Lighthouse,
                        comp => comp.Cast<PayableUpgrade>().Price);
                    break;

                case GamePrefabID.Lighthouse_Wood:
                case GamePrefabID.Lighthouse_Stone:
                case GamePrefabID.Lighthouse_Iron:
                    // 已开发状态：根据锁定状态显示不同颜色
                    view.TryAddMapMarker(component, null, MarkerStyle.Lighthouse.Sign, Strings.Lighthouse,
                        comp =>
                        {
                            var p = comp.Cast<PayableUpgrade>();
                            bool canPay = !p.IsLocked(GetLocalPlayer(), out var reason);
                            var price = canPay ? p.Price : 0;
                            return price;
                        },
                        comp =>
                        {
                            if (!comp.gameObject.activeSelf)
                                return MarkerStyle.Lighthouse.Building.Color;
                            var p = comp.Cast<PayableUpgrade>();
                            bool canPay = !p.IsLocked(GetLocalPlayer(), out var reason);
                            bool isLocked = (reason != LockIndicator.LockReason.NotLocked && reason != LockIndicator.LockReason.NoUpgrade);
                            var color = isLocked ? MarkerStyle.Lighthouse.Locked.Color : MarkerStyle.Lighthouse.Color;
                            return color;
                        });
                    break;
            }
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }

    /// <summary>
    /// 采石场标记映射器。
    /// </summary>
    public class QuarryMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Quarry;

        public void Map(Component component)
        {
            var payableUpgrade = component.Cast<PayableUpgrade>();
            var prefabId = payableUpgrade.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return;

            var gamePrefabId = (GamePrefabID)prefabId.prefabID;

            switch (gamePrefabId)
            {
                case GamePrefabID.Quarry_undeveloped:
                    view.TryAddMapMarker(component, MarkerStyle.Quarry.Locked.Color, MarkerStyle.Quarry.Sign, Strings.Quarry,
                        comp => comp.Cast<PayableUpgrade>().Price);
                    break;

                case GamePrefabID.Quarry:
                    view.TryAddMapMarker(component, null, MarkerStyle.Quarry.Sign, Strings.Quarry, null,
                        comp => comp.gameObject.activeSelf ? MarkerStyle.Quarry.Unlocked.Color : MarkerStyle.Quarry.Building.Color);
                    break;
            }
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }

    /// <summary>
    /// 矿井标记映射器。
    /// </summary>
    public class MineMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Mine;

        public void Map(Component component)
        {
            var payableUpgrade = component.Cast<PayableUpgrade>();
            var prefabId = payableUpgrade.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return;

            var gamePrefabId = (GamePrefabID)prefabId.prefabID;

            switch (gamePrefabId)
            {
                case GamePrefabID.Mine_undeveloped:
                    view.TryAddMapMarker(component, MarkerStyle.Mine.Locked.Color, MarkerStyle.Mine.Sign, Strings.Mine,
                        comp => comp.Cast<PayableUpgrade>().Price);
                    break;

                case GamePrefabID.Mine:
                    view.TryAddMapMarker(component, null, MarkerStyle.Mine.Sign, Strings.Mine, null,
                        comp => comp.gameObject.activeSelf ? MarkerStyle.Mine.Unlocked.Color : MarkerStyle.Mine.Building.Color);
                    break;
            }
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }

    /// <summary>
    /// 墙体标记映射器。
    /// 这是新架构的另一个示例：Wall 没有专属的游戏类型，现在可以独立创建 Mapper。
    /// </summary>
    public class WallMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Wall;

        public void Map(Component component)
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
                        view.AddWallToList(marker);  // 添加到 LeftWalls 或 RightWalls 并创建连接线
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
                        view.AddWallToList(wallWreckMarker);  // 添加到 LeftWalls 或 RightWalls 并创建连接线
                    }
                    break;
            }
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件
    }
}
