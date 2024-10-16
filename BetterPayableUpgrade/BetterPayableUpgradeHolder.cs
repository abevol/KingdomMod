using System;
using BepInEx.Logging;
using Coatsink.Common;
using UnityEngine;
using HarmonyLib;
#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;

#else
using System.Collections.Generic;
#endif

namespace KingdomMod.BetterPayableUpgrade;

public class BetterPayableUpgradeHolder : MonoBehaviour
{
    public static BetterPayableUpgradeHolder Instance { get; private set; }
    private static ManualLogSource log;
    private static System.Collections.Generic.Dictionary<GamePrefabID, ModifyData> _modifyDataDict;

    public class Patcher
    {
        public static void PatchAll()
        {
            try
            {
                var harmony = new Harmony("KingdomMod.BetterPayableUpgrade.Patcher");
                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                log.LogError($"[Patcher] => {ex}");
            }
        }

        [HarmonyPatch(typeof(CurrencyBag), "Init")]
        public class CoinBagInitPatcher
        {
            public static void Prefix(CurrencyBag __instance)
            {
                for (int i = 0; i < CurrencyManager.AllCurrencyTypes.Length; i++)
                {
                    CurrencyType type = CurrencyManager.AllCurrencyTypes[i];
                    if (!SingletonMonoBehaviour<Managers>.Inst.currency.TryGetData(type, out var currencyConfig))
                    {
                        throw new Exception(string.Format("Cannot get CurrencyData for {0}!", type));
                    }

                    if (currencyConfig.BagPrefab)
                    {
                        log.LogMessage($"CoinBagInitPatcher localScale: {currencyConfig.BagPrefab.gameObject.transform.localScale}");

                        currencyConfig.BagPrefab.gameObject.transform.localScale = new Vector3(0.668f, 0.668f, 0.668f);

                        var bagCoinPrefab = BiomeData.GetPrefabSwap(currencyConfig.BagPrefab);
                        bagCoinPrefab.gameObject.transform.localScale = new Vector3(0.668f, 0.668f, 0.668f);
                    }
                }
            }
        }
    }

    public static void Initialize(BetterPayableUpgradePlugin plugin)
    {
        log = plugin.LogSource;
#if IL2CPP
            ClassInjector.RegisterTypeInIl2Cpp<BetterPayableUpgradeHolder>();
#endif
        GameObject obj = new(nameof(BetterPayableUpgradeHolder));
        DontDestroyOnLoad(obj);
        obj.hideFlags = HideFlags.HideAndDontSave;
        Instance = obj.AddComponent<BetterPayableUpgradeHolder>();
    }

    public BetterPayableUpgradeHolder()
    {
        log.LogDebug($"{this.GetType().Name} constructor");
    }

    private void Start()
    {
        log.LogMessage($"{this.GetType().Name} Start.");

        Patcher.PatchAll();
        Game.OnGameStart += (Action)OnGameStart;
    }

    private void Update()
    {
    }

    private void OnGUI()
    {
    }

    private bool IsPlaying()
    {
        var game = Managers.Inst?.game;
        if (game == null) return false;
        return game.state is Game.State.Playing or Game.State.NetworkClientPlaying or Game.State.Menu;
    }

    public void OnGameStart()
    {
        log.LogDebug("OnGameStart.");

        AdjustCosts();
    }
        
    public void AdjustCosts()
    {
        var prefabs = Managers.Inst.prefabs;
        if (prefabs != null)
        {
            log.LogDebug("Handle prefabs start.");
            
            var prefabIds = new System.Collections.Generic.List<GamePrefabID>
            {
                GamePrefabID.Citizen_House,
                GamePrefabID.Workshop,
                GamePrefabID.Tower_Baker,
                GamePrefabID.Tower6,
                GamePrefabID.Tower5,
                GamePrefabID.Tower4,
                GamePrefabID.Tower3,
                GamePrefabID.Tower2,
                GamePrefabID.Tower1,
                GamePrefabID.Tower0
            };

            foreach (var prefabId in prefabIds)
            {
                if (prefabId == GamePrefabID.MerchantHouse) continue;
                var go = prefabs.GetPrefabById((int)prefabId);
                if (go == null) continue;
            
                HandlePayable(go, true);
            }
            
            log.LogDebug("Handle prefabs end.");
        }

        var swapData = BiomeHolder.Inst.LoadedBiome?.swapData;
        if (swapData != null)
        {
            log.LogDebug($"swapData._prefabSwapDictionary: {swapData._prefabSwapDictionary.Count}");
            foreach (var pair in swapData._prefabSwapDictionary)
            {
                log.LogDebug($"swapData: {pair.Key.name}, {pair.Value.name}, {pair.Value.GetComponent<PrefabID>()?.prefabID}");
                HandlePayable(pair.Value.gameObject, true);
            }
        }

        var customSwapData = ChallengeData.Current?.customSwapData;
        if (customSwapData != null)
        {
            log.LogDebug($"customSwapData._prefabSwapDictionary: {customSwapData._prefabSwapDictionary.Count}");
            foreach (var pair in customSwapData._prefabSwapDictionary)
            {
                log.LogDebug($"customSwapData: {pair.Key.name}, {pair.Value.name}");
                HandlePayable(pair.Value.gameObject, true);
            }
        }

        var payables = SingletonMonoBehaviour<Managers>.Inst.payables;
        if (payables != null)
        {
            foreach (var obj in payables.AllPayables
                    )
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;

                HandlePayable(go, false);
            }

            foreach (var obj in payables._allBlockers)
            {
                if (obj == null) continue;
                var scaffolding = obj.GetComponent<Scaffolding>();
                if (scaffolding == null) continue;
                var go = scaffolding.Building;
                if (go == null) continue;

                HandlePayable(go, false, false);
            }
        }

    }

    private static void HandlePayable(GameObject go, bool isPrefab, bool modifyBuildPoints = true)
    {
        _modifyDataDict ??= new System.Collections.Generic.Dictionary<GamePrefabID, ModifyData>
        {
            { GamePrefabID.Citizen_House, new ModifyData(3) },
            { GamePrefabID.Workshop, new ModifyData(3) },
            { GamePrefabID.Tower_Baker, new ModifyData(2) },
            { GamePrefabID.Tower6, new ModifyData(16, 120) },
            { GamePrefabID.Tower5, new ModifyData(12, 80, GamePrefabID.Invalid, GamePrefabID.Tower3) },
            { GamePrefabID.Tower4, new ModifyData(8, 70) },
            { GamePrefabID.Tower3, new ModifyData(8, 50, GamePrefabID.Tower5) },
            { GamePrefabID.Tower2, new ModifyData(5, 30, GamePrefabID.Invalid, GamePrefabID.Tower0) },
            { GamePrefabID.Tower1, new ModifyData(4, 20) },
            { GamePrefabID.Tower0, new ModifyData(2, 10, GamePrefabID.Tower2) },
        };

        var prefab = go.GetComponent<PrefabID>();
        if (prefab == null)
            return;

        var prefabId = (GamePrefabID)prefab.prefabID;
        switch (prefabId)
        {
            case GamePrefabID.Citizen_House:
            {
                var payable = go.GetComponent<CitizenHousePayable>();
                if (payable != null)
                {
                    log.LogDebug($"Change {go.name} price from {payable.Price} to 3");
                    payable.Price = 3;
                }
                break;
            }
            case GamePrefabID.Workshop:
            {
                var payableWorkshop = go.GetComponent<PayableWorkshop>();
                if (payableWorkshop != null)
                {
                    var payable = payableWorkshop.barrelCounterpart;
                    if (payable != null)
                    {
                        log.LogDebug($"Change {go.name} price from {payable.Price} to 3");
                        payable.Price = 3;
                    }
                }
                break;
            }
            case GamePrefabID.Tower_Baker:
            {
                var payable = go.GetComponent<PayableShop>();
                if (payable != null)
                {
                    log.LogDebug($"Change {go.name} price from {payable.Price} to 2");
                    payable.Price = 2;
                }
                break;
            }
            case GamePrefabID.Tower6:
            case GamePrefabID.Tower5:
            case GamePrefabID.Tower4:
            case GamePrefabID.Tower3:
            case GamePrefabID.Tower2:
            case GamePrefabID.Tower1:
            case GamePrefabID.Tower0:
                HandleTower(go, prefabId, isPrefab, modifyBuildPoints);
                break;
        }
    }

    private static void HandleTower(GameObject go, GamePrefabID prefabId, bool isPrefab, bool modifyBuildPoints = true)
    {
        var prefabs = SingletonMonoBehaviour<Managers>.Inst.prefabs;
        if (prefabs == null) return;

        var modifyData = _modifyDataDict[prefabId];

        if (modifyBuildPoints)
        {
            var workable = go.GetComponent<WorkableBuilding>();
            if (workable != null)
            {
                var constructionBuilding = Require.Component<ConstructionBuildingComponent>(workable);
                var buildPoints = constructionBuilding._buildPoints;
                log.LogDebug($"Change {go.name} buildPoints from {buildPoints} to {modifyData.BuildPoints}");
                constructionBuilding._buildPoints = modifyData.BuildPoints;
            }
        }

        var payable = go.GetComponent<PayableUpgrade>();
        if (payable != null)
        {
            log.LogDebug($"Change {go.name} price from {payable.Price} to {modifyData.Price}");
            payable.Price = modifyData.Price;

            if (modifyData.NextPrefab != GamePrefabID.Invalid)
            {
                if (isPrefab)
                    payable.nextPrefab = prefabs.GetPrefabById((int)modifyData.NextPrefab);
                else
                    payable.SetNextPrefab((int)modifyData.NextPrefab);

                log.LogDebug($"Change {prefabId} nextPrefab to {modifyData.NextPrefab}");
            }
        }
    }

    public struct ModifyData
    {
        public int Price;
        public int BuildPoints;
        public GamePrefabID NextPrefab;
        public GamePrefabID LastPrefab;

        public ModifyData(int price, int buildPoints = 0, GamePrefabID nextPrefab = GamePrefabID.Invalid, GamePrefabID lastPrefab = GamePrefabID.Invalid)
        {
            Price = price; BuildPoints = buildPoints; NextPrefab = nextPrefab; LastPrefab = lastPrefab;
        }
    }
}