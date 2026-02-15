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
            var prefabId = component.gameObject.GetComponent<PrefabID>();
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
                    if (component.TryCast<PayableUpgrade>() != null) return;
                    view.TryAddMapMarker(component, null, MarkerStyle.Lighthouse.Sign, Strings.Lighthouse,
                        comp =>
                        {
                            var p = comp.GetComponent<PayableUpgrade>();
                            if (p == null) return 0;
                            bool canPay = !p.IsLocked(OverlayMapHolder.GetLocalPlayer(), out var reason);
                            var price = canPay ? p.Price : 0;
                            return price;
                        },
                        comp =>
                        {
                            if (!comp.gameObject.activeSelf)
                                return MarkerStyle.Lighthouse.Building.Color;
                            var p = comp.GetComponent<PayableUpgrade>();
                            if (p == null) return MarkerStyle.Lighthouse.Color;
                            bool canPay = !p.IsLocked(OverlayMapHolder.GetLocalPlayer(), out var reason);
                            bool isLocked = (reason != LockIndicator.LockReason.NotLocked && reason != LockIndicator.LockReason.NoUpgrade);
                            var color = isLocked ? MarkerStyle.Lighthouse.Locked.Color : MarkerStyle.Lighthouse.Color;
                            return color;
                        });
                    break;
            }
        }

        // 已由 PayableMapper 中的父类方法补丁通知组件的启用和禁用事件

        [HarmonyPatch(typeof(Lighthouse), nameof(Lighthouse.OnEnable))]
        private class OnEnablePatch
        {
            public static void Postfix(Lighthouse __instance)
            {
                ForEachTopMapView(view => view.OnComponentCreated(__instance));
            }
        }

        [HarmonyPatch(typeof(Lighthouse), nameof(Lighthouse.OnDisable))]
        private class OnDisablePatch
        {
            public static void Prefix(Lighthouse __instance)
            {
                ForEachTopMapView(view => view.OnComponentDestroyed(__instance));
            }
        }
    }
}