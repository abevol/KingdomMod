using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Coatsink.Common;
using UnityEngine;

namespace KingdomMod
{
    public class BetterPayableUpgrade : MonoBehaviour
    {
        private static ManualLogSource log;
        private static Dictionary<PrefabIDs, ModifyData> ModifyDataDict;

        public static void Initialize(BetterPayableUpgradePlugin plugin)
        {
            log = plugin.Log;
            var component = plugin.AddComponent<BetterPayableUpgrade>();
            component.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(component.gameObject);
        }

        public BetterPayableUpgrade()
        {
            log.LogDebug($"{this.GetType().Name} constructor");
        }

        private void Start()
        {
            log.LogMessage($"{this.GetType().Name} Start.");
            Game.add_OnGameStart((Action)OnGameStart);
        }

        private void Update()
        {
        }

        private void OnGUI()
        {
        }

        private bool IsPlaying()
        {
            if (!Managers.Inst) return false;
            if (!Managers.Inst.game) return false;
            return Managers.Inst.game.state is Game.State.Playing or Game.State.Menu;
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
            
                var prefabIds = new List<PrefabIDs>
                {
                    PrefabIDs.Citizen_House,
                    PrefabIDs.Workshop,
                    PrefabIDs.Tower_Baker,
                    PrefabIDs.Tower6,
                    PrefabIDs.Tower5,
                    PrefabIDs.Tower4,
                    PrefabIDs.Tower3,
                    PrefabIDs.Tower2,
                    PrefabIDs.Tower1,
                    PrefabIDs.Tower0
                };
                foreach (var prefabId in prefabIds)
                {
                    if (prefabId == PrefabIDs.MerchantHouse) continue;
                    var go = prefabs.GetPrefabById((int)prefabId);
                    if (go == null) continue;
            
                    HandlePayable(go, true);
                }
            
                log.LogDebug("Handle prefabs end.");
            }

            var holder = SingletonMonoBehaviour<Managers>.Inst.holder;
            if (holder != null)
            {
                foreach (var towerPrefab in holder.towerPrefabs)
                {
                    HandlePayable(towerPrefab.gameObject, true);
                }
            }

            var swapData = BiomeHolder.Inst.LoadedBiome?.swapData;
            if (swapData != null)
            {
                log.LogDebug($"swapData._prefabSwapDictionary: {swapData._prefabSwapDictionary.Count}");
                foreach (var pair in swapData._prefabSwapDictionary)
                {
                    log.LogDebug($"swapData: {pair.key.name}, {pair.value.name}, {pair.value.GetComponent<PrefabID>()?.prefabID}");
                    HandlePayable(pair.value.gameObject, true);
                }
            }

            var customSwapData = ChallengeData.Current?.customSwapData;
            if (customSwapData != null)
            {
                log.LogDebug($"customSwapData._prefabSwapDictionary: {customSwapData._prefabSwapDictionary.Count}");
                foreach (var pair in customSwapData._prefabSwapDictionary)
                {
                    log.LogDebug($"customSwapData: {pair.key.name}, {pair.value.name}");
                    HandlePayable(pair.value.gameObject, true);
                }
            }

            var payables = SingletonMonoBehaviour<Managers>.Inst.payables;
            if (payables != null)
            {
                foreach (var obj in payables.AllPayables)
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
                    var go = scaffolding.building;
                    if (go == null) continue;

                    HandlePayable(go, false, false);
                }
            }

        }

        private static void HandlePayable(GameObject go, bool isPrefab, bool modifyBuildPoints = true)
        {
            ModifyDataDict ??= new Dictionary<PrefabIDs, ModifyData>
            {
                { PrefabIDs.Citizen_House, new ModifyData(3) },
                { PrefabIDs.Workshop, new ModifyData(3) },
                { PrefabIDs.Tower_Baker, new ModifyData(2) },
                { PrefabIDs.Tower6, new ModifyData(16, 120) },
                { PrefabIDs.Tower5, new ModifyData(12, 80, PrefabIDs.None, PrefabIDs.Tower3) },
                { PrefabIDs.Tower4, new ModifyData(8, 70) },
                { PrefabIDs.Tower3, new ModifyData(8, 50, PrefabIDs.Tower5) },
                { PrefabIDs.Tower2, new ModifyData(5, 30, PrefabIDs.None, PrefabIDs.Tower0) },
                { PrefabIDs.Tower1, new ModifyData(4, 20) },
                { PrefabIDs.Tower0, new ModifyData(2, 10, PrefabIDs.Tower2) },
            };

            var prefab = go.GetComponent<PrefabID>();
            if (prefab == null)
                return;

            var prefabId = (PrefabIDs)prefab.prefabID;
            switch (prefabId)
            {
                case PrefabIDs.Citizen_House:
                    {
                        var payable = go.GetComponent<CitizenHousePayable>();
                        if (payable != null)
                        {
                            log.LogDebug($"Change {go.name} price from {payable.price} to 3");
                            payable.price = 3;
                        }
                        break;
                    }
                case PrefabIDs.Workshop:
                    {
                        var payableWorkshop = go.GetComponent<PayableWorkshop>();
                        if (payableWorkshop != null)
                        {
                            var payable = payableWorkshop.barrelCounterpart;
                            if (payable != null)
                            {
                                log.LogDebug($"Change {go.name} price from {payable.price} to 3");
                                payable.price = 3;
                            }
                        }
                        break;
                    }
                case PrefabIDs.Tower_Baker:
                    {
                        var payable = go.GetComponent<PayableShop>();
                        if (payable != null)
                        {
                            log.LogDebug($"Change {go.name} price from {payable.price} to 2");
                            payable.price = 2;
                        }
                        break;
                    }
                case PrefabIDs.Tower6:
                    {
                        HandleTower(go, prefabId, 6, isPrefab, modifyBuildPoints);
                        break;
                    }
                case PrefabIDs.Tower5:
                    {
                        HandleTower(go, prefabId, 5, isPrefab, modifyBuildPoints);
                        break;
                    }
                case PrefabIDs.Tower4:
                    {
                        HandleTower(go, prefabId, 4, isPrefab, modifyBuildPoints);
                        break;
                    }
                case PrefabIDs.Tower3:
                    {
                        HandleTower(go, prefabId, 3, isPrefab, modifyBuildPoints);
                        break;
                    }
                case PrefabIDs.Tower2:
                    {
                        HandleTower(go, prefabId, 2, isPrefab, modifyBuildPoints);
                        break;
                    }
                case PrefabIDs.Tower1:
                    {
                        HandleTower(go, prefabId, 1, isPrefab, modifyBuildPoints);
                        break;
                    }
                case PrefabIDs.Tower0:
                    {
                        HandleTower(go, prefabId, 0, isPrefab, modifyBuildPoints);
                        break;
                    }
            }
        }

        private static void HandleTower(GameObject go, PrefabIDs prefabId, int index, bool isPrefab, bool modifyBuildPoints = true)
        {
            var prefabs = SingletonMonoBehaviour<Managers>.Inst.prefabs;
            if (prefabs == null) return;

            var modifyData = ModifyDataDict[prefabId];

            if (modifyBuildPoints)
            {
                var workable = go.GetComponent<WorkableBuilding>();
                if (workable != null)
                {
                    log.LogDebug($"Change {go.name} buildPoints from {workable.buildPoints} to {modifyData.BuildPoints}");
                    workable.buildPoints = modifyData.BuildPoints;
                }
            }

            var payable = go.GetComponent<PayableUpgrade>();
            if (payable != null)
            {
                log.LogDebug($"Change {go.name} price from {payable.price} to {modifyData.Price}");
                payable.price = modifyData.Price;

                if (modifyData.NextPrefab != PrefabIDs.None)
                {
                    payable.nextPrefab = prefabs.GetPrefabById((int)modifyData.NextPrefab);

                    log.LogDebug($"Change {prefabId} nextPrefab to {modifyData.NextPrefab}");
                }

                // if (payable.nextPrefab != null)
                // {
                //     HandlePayable(payable.nextPrefab, true);
                // }
            }
        }

        public struct ModifyData
        {
            public int Price;
            public int BuildPoints;
            public PrefabIDs NextPrefab;
            public PrefabIDs LastPrefab;

            public ModifyData(int price, int buildPoints = 0, PrefabIDs nextPrefab = PrefabIDs.None, PrefabIDs lastPrefab = PrefabIDs.None)
            {
                Price = price; BuildPoints = buildPoints; NextPrefab = nextPrefab; LastPrefab = lastPrefab;
            }
        }

        public enum PrefabIDs
        {
            None = -1,
            Castle0 = 0,
            Castle1 = 1,
            Castle2 = 2,
            Castle3 = 3,
            Castle4 = 4,
            Castle5 = 5,
            Castle6 = 6,
            Farmhouse0 = 7,
            Farmhouse1 = 8,
            Farmhouse2 = 9,
            Tower0 = 10,
            Tower1 = 11,
            Tower2 = 12,
            Tower3 = 13,
            Tower4 = 14,
            Wall0 = 15,
            Wall1 = 16,
            Wall2 = 17,
            Wall3 = 18,
            Wall4 = 19,
            Wreck = 20,
            Quarry_undeveloped = 21,
            Quarry = 22,
            Tree = 23,
            Chest = 24,
            Wall1_Wreck = 25,
            Wall2_Wreck = 26,
            Wall3_Wreck = 27,
            Wall4_Wreck = 28,
            Wall5_Wreck = 29,
            Wall4_horn = 30,
            Wall5_horn = 31,
            Wall5 = 32,
            Tower_Baker = 33,
            Tower_Ballista = 34,
            Tower_Knight = 35,
            Lighthouse_undeveloped = 36,
            Beach = 37,
            Wharf = 38,
            Beggar_Camp = 39,
            Portal = 40,
            Teleporter = 41,
            TeleporterRift = 42,
            Cliff_Portal = 43,
            BoatSailPosition = 44,
            Lighthouse_Stone = 45,
            Lighthouse_Iron = 46,
            Lighthouse_Wood = 47,
            Castle7 = 48,
            Mine_undeveloped = 49,
            Mine = 50,
            oakTree = 51,
            Forge = 52,
            ShopPike = 53,
            Workshop = 54,
            ShopScythe = 55,
            CaveSpawnerTree = 56,
            Title = 57,
            Tower5 = 58,
            Tower6 = 59,
            MerchantHouse = 60,
            FarmhouseStable = 61,
            BeachPortal = 62,
            BoatSailPosition_Stone = 63,
            Citizen_House = 64
        }
    }
}
