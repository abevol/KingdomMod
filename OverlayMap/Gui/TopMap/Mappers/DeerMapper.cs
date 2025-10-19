using HarmonyLib;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Patchers;
using KingdomMod.SharedLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap.Mappers
{
    public class DeerMapper(TopMapView view) : IComponentMapper
    {
        public Component[] GetComponents()
        {
            return GameExtensions.FindObjectsWithTagOfType<Deer>(Tags.Wildlife).Cast<Component>().ToArray();
        }

        public void Map(Component component)
        {
            view.TryAddMapMarker(component, MarkerStyle.Deer.Color, MarkerStyle.Deer.Sign, Strings.Deer, null,
                comp =>
            {
                var state = ((Deer)comp)._fsm.Current;
                return state == 5 ? MarkerStyle.DeerFollowing.Color : MarkerStyle.Deer.Color;
            }, comp => comp.gameObject.activeSelf && !((Deer)comp)._damageable.isDead, MarkerRow.Movable);
        }

        // [HarmonyPatch(typeof(Deer), nameof(Deer.Awake))]
        // private class Deer_Awake_Patch
        // {
        //     public static void Postfix(Deer __instance)
        //     {
        //         TopMapView.ForEachTopMapView(view => view.OnComponentCreated(__instance, [ObjectPatcher.SourceFlag.Create10]));
        //     }
        // }
        //
        // [HarmonyPatch(typeof(Deer), nameof(Deer.OnDestroy))]
        // private class Deer_OnDestroy_Patch
        // {
        //     public static void Prefix(Deer __instance)
        //     {
        //         TopMapView.ForEachTopMapView(view => view.OnComponentDestroyed(__instance, [ObjectPatcher.SourceFlag.Destroy10]));
        //     }
        // }
    }
}