using KingdomMod.OverlayMap.Config;
using KingdomMod.SharedLib;
using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    /// <summary>
    /// 石矿标记映射器。
    /// </summary>
    public class QuarryMapper(TopMapView view) : IComponentMapper
    {
        public MapMarkerType? MarkerType => MapMarkerType.Quarry;

        public void Map(Component component, NotifierType notifierType, ResolverType resolverType)
        {
            switch (resolverType)
            {
                case ResolverType.PayableUpgrade:
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

                    break;
                }
                case ResolverType.PayableBlocker:
                {
                    view.TryAddMapMarker(component, null, MarkerStyle.Quarry.Sign, Strings.Quarry, null,
                        comp => comp.gameObject.activeSelf ? MarkerStyle.Quarry.Unlocked.Color : MarkerStyle.Quarry.Building.Color);
                    break;
                }
                case ResolverType.Scaffolding:
                {
                    view.TryAddMapMarker(component, MarkerStyle.Quarry.Building.Color, MarkerStyle.Quarry.Sign, Strings.Quarry);
                        break;
                }
            }
        }
    }
}