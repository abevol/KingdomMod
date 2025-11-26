using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class PayableUpgradeMapper(TopMapView view) : IComponentMapper
    {
        public void Map(Component component)
        {
            var payableUpgrade = component.Cast<PayableUpgrade>();
            var prefabId = payableUpgrade.gameObject.GetComponent<PrefabID>();
            if (prefabId == null) return;
            switch ((GamePrefabID)prefabId.prefabID)
            {
                case GamePrefabID.Wall0:
                    // view.TryAddMapMarker(component, MarkerStyle.WallFoundation.Color, MarkerStyle.WallFoundation.Sign, null);
                    break;
                case GamePrefabID.Wall1:
                case GamePrefabID.Wall2:
                case GamePrefabID.Wall3:
                case GamePrefabID.Wall4:
                case GamePrefabID.Wall5:
                    // LogMessage($"Wall: InstanceID: {component.GetInstanceID()}, {GameObjectDetails.JsonSerialize(new GameObjectDetails(component.gameObject))}");
                    var marker = view.TryAddMapMarker(component, MarkerStyle.Wall.Building.Color, MarkerStyle.Wall.Sign, null);
                    if (marker != null)
                        view.AddWallNode(marker);
                    ConstructionEventHandler.Create(marker, MarkerStyle.Wall.Color);
                    break;
                case GamePrefabID.Wall1_Wreck:
                case GamePrefabID.Wall2_Wreck:
                case GamePrefabID.Wall3_Wreck:
                case GamePrefabID.Wall4_Wreck:
                case GamePrefabID.Wall5_Wreck:
                    // LogMessage($"Wall_Wreck: InstanceID: {component.GetInstanceID()}, {GameObjectDetails.JsonSerialize(new GameObjectDetails(component.gameObject))}");
                    view.TryAddMapMarker(component, MarkerStyle.Wall.Wrecked.Color, MarkerStyle.Wall.Sign, null);
                    break;
                case GamePrefabID.Lighthouse_undeveloped:
                    view.TryAddMapMarker(component, MarkerStyle.Lighthouse.Unpaid.Color, MarkerStyle.Lighthouse.Sign, Strings.Lighthouse,
                        comp => comp.Cast<PayableUpgrade>().Price);
                    break;
                case GamePrefabID.Lighthouse_Wood:
                case GamePrefabID.Lighthouse_Stone:
                case GamePrefabID.Lighthouse_Iron:
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
                case GamePrefabID.Quarry_undeveloped:
                    view.TryAddMapMarker(component, MarkerStyle.Quarry.Locked.Color, MarkerStyle.Quarry.Sign, Strings.Quarry,
                        comp => comp.Cast<PayableUpgrade>().Price);
                    break;
                case GamePrefabID.Quarry:
                    view.TryAddMapMarker(component, null, MarkerStyle.Quarry.Sign, Strings.Quarry, null,
                        comp => comp.gameObject.activeSelf ? MarkerStyle.Quarry.Unlocked.Color : MarkerStyle.Quarry.Building.Color);
                    break;
                case GamePrefabID.Mine_undeveloped:
                    view.TryAddMapMarker(component, MarkerStyle.Mine.Locked.Color, MarkerStyle.Mine.Sign, Strings.Mine,
                        comp => comp.Cast<PayableUpgrade>().Price);
                    break;
                case GamePrefabID.Mine:
                    view.TryAddMapMarker(component, null, MarkerStyle.Mine.Sign, Strings.Mine, null,
                        comp => comp.gameObject.activeSelf ? MarkerStyle.Mine.Unlocked.Color : MarkerStyle.Mine.Building.Color);
                    break;
                case GamePrefabID.Cliff_Portal:
                    view.TryAddMapMarker(component, MarkerStyle.PortalCliff.Color, MarkerStyle.PortalCliff.Sign, Strings.PortalCliff,
                        comp => comp.Cast<PayableUpgrade>().Price);
                    break;
            }
        }
    }
}