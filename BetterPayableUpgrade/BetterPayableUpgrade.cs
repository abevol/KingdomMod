using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace KingdomMod
{
    public class BetterPayableUpgrade : MonoBehaviour
    {
        private static ManualLogSource log;
        private bool enableStaminaBar = true;
        private static GameObject[] TowerPrefabs = new GameObject[7];
        private static Dictionary<PrefabIDs, ModifyData> ModifyDataDict = null;

        public static void Initialize(BetterPayableUpgradePlugin plugin)
        {
            log = plugin.Log;
            log.LogMessage($"BetterPayableUpgrade Initialize");
            Patcher.PatchAll();

            var component = plugin.AddComponent<BetterPayableUpgrade>();
            component.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(component.gameObject);
        }

        public BetterPayableUpgrade()
        {
            log.LogMessage($"{this.GetType().Name} constructor");
        }

        private void Start()
        {
            log.LogMessage($"{this.GetType().Name} Start.");
            Game.add_OnGameStart((Action)OnGameStart);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                log.LogMessage("N key pressed.");
                enableStaminaBar = !enableStaminaBar;
            }
        }

        private void OnGUI()
        {
            if (!IsPlaying()) return;
            if (!enableStaminaBar) return;

            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return;
        }

        private bool IsPlaying()
        {
            if (!Managers.Inst) return false;
            if (!Managers.Inst.game) return false;
            return Managers.Inst.game.state is Game.State.Playing or Game.State.Menu;
        }

        public void OnGameStart()
        {
            log.LogMessage("OnGameStart.");

            var holder = Managers.Inst.holder;
            if (holder != null)
            {
                log.LogMessage($"towerPrefabs: {holder.towerPrefabs.Count}");
                foreach (var obj in holder.towerPrefabs)
                {
                    log.LogMessage($"{obj.name}, {obj.GetComponent<PayableUpgrade>()?.price}, {obj.GetComponent<WorkableBuilding>()?.buildPoints}");
                }
                log.LogMessage("towerPrefabs end.");

                log.LogMessage($"campaignMapLandPrefabs: {holder.campaignMapLandPrefabs.Count}");
                foreach (var obj in holder.campaignMapLandPrefabs)
                {
                    log.LogMessage($"{obj.name}, {obj.GetComponent<PayableUpgrade>()?.price}, {obj.GetComponent<WorkableBuilding>()?.buildPoints}");
                }
                log.LogMessage("campaignMapLandPrefabs end.");
            }

            AdjustCosts();
        }

        public class Patcher
        {
            // TODO: try to hook Resources.Load
            // TODO: try to hook Pool.SpawnOrInstantiateGO
            // TODO: try to hook UnityEngine.Object.Instantiate

            public static void PatchAll()
            {
                // Managers;
                // Pool.SpawnOrInstantiateGO
                try
                {
                    // var Load1 = typeof(Resources).GetMethod("Load", 0, new Type[] { typeof(string) });
                    // var Load2 = typeof(Resources).GetMethod("Load", 1, new Type[] { typeof(string) });
                    // log.LogMessage($"Load1: {Load1}");
                    // log.LogMessage($"Load2: {Load2}");

                    var harmony = new Harmony("KingdomMod.BetterPayableUpgrade.Patcher");
                    harmony.PatchAll();
                }
                catch (Exception ex)
                {
                    log.LogMessage($"[Patcher] => {ex}");
                }
            }

            [HarmonyPatch(typeof(Resources), nameof(Resources.Load), new Type[] { typeof(string), typeof(Il2CppSystem.Type) })]
            public class ResourcesLoadPatcher
            {
                public static void Postfix(ref UnityEngine.Object __result, string path, Il2CppSystem.Type systemTypeInstance)
                {
                    log.LogMessage($"[ResourcesLoadPatcher] Postfix: path: {path}, name: {__result.name}, type: {systemTypeInstance.FullName}");
                    if (__result != null)
                    {
                        var biomeData = __result.TryCast<BiomeData>();
                        if (biomeData != null)
                        {
                            if (__result.name == "NorselandsBiomeData")
                                HandleBiomeData(biomeData);
                        }

                        var biomeObjectPools = __result.TryCast<BiomeObjectPools>();
                        if (biomeObjectPools != null)
                        {
                            HandleBiomeObjectPools(biomeObjectPools);
                        }

                        var go = __result.TryCast<GameObject>();
                        if (go == null)
                        {
                            var comp = __result.TryCast<Component>();
                            if (comp != null)
                            {
                                go = comp.gameObject;
                            }
                        }
                        if (go != null)
                        {
                            HandlePayable(go, true);
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(Resources), nameof(Resources.LoadAsync),
                new Type[] { typeof(string), typeof(Il2CppSystem.Type) })]
            public class ResourcesLoadAsyncPatcher
            {
                public static void Postfix(ref ResourceRequest __result,
                    string path, Il2CppSystem.Type type)
                {
                    log.LogMessage($"[ResourcesLoadAsyncPatcher] path:{path}, {type.FullName}");

                    __result.add_completed((Il2CppSystem.Action<AsyncOperation>)ResourcesLoadAsyncCompleted);
                }

                private static void ResourcesLoadAsyncCompleted(AsyncOperation op)
                {
                    var res = op.TryCast<ResourceRequest>();
                    if (res == null)
                        return;

                    log.LogMessage($"[ResourcesLoadAsyncCompleted] path:{res.m_Path}, {res.m_Type.FullName}, {res.asset.name}");
                    var biomeData = res.asset.TryCast<BiomeData>();
                    if (biomeData != null)
                    {
                        HandleBiomeData(biomeData);
                    }
                }
            }

            private static void HandleBiomeData(BiomeData biomeData)
            {
                log.LogMessage($"blockName: {biomeData.blockName}");
                log.LogMessage($"mapLandPrefabs: {biomeData.campaignData.mapLandPrefabs.Count}");
                foreach (var obj in biomeData.campaignData.mapLandPrefabs)
                {
                    log.LogMessage($"obj: {obj.name}");
                }

                log.LogMessage($"objectDatas: {biomeData.objectDatas.Count}");
                foreach (var obj in biomeData.objectDatas)
                {
                    log.LogMessage($"objectName: {obj.objectName}");
                    log.LogMessage($"objectOverrides: {obj.objectOverrides.Count}");
                    foreach (var o in obj.objectOverrides)
                    {
                        log.LogMessage($"o: {o.name}");
                        HandlePayable(o.gameObject, true);
                    }
                }

                log.LogMessage($"uniqueShopPrefabs: {biomeData.biomeSpecificAssets.uniqueShopPrefabs.Count}");
                foreach (var obj in biomeData.biomeSpecificAssets.uniqueShopPrefabs)
                {
                    log.LogMessage($"obj: {obj.name}, type: {obj.type}");
                }
            }

            private static void HandleBiomeObjectPools(BiomeObjectPools biomeObjectPools)
            {
                log.LogMessage($"biomeObjectPools: {biomeObjectPools.biomeObjectPools.Count}");
                foreach (var pool in biomeObjectPools.biomeObjectPools)
                {
                    log.LogMessage($"pool: {pool.name}, {pool.prefab.name}");
                }
            }

            [HarmonyPatch(typeof(Resources), nameof(Resources.LoadAll),
                new Type[] { typeof(string), typeof(Il2CppSystem.Type) })]
            public class ResourcesLoadAllPatcher
            {
                public static void Postfix(ref Il2CppReferenceArray<UnityEngine.Object> __result,
                    string path, Il2CppSystem.Type systemTypeInstance)
                {
                    log.LogMessage($"[ResourcesLoadAllPatcher] path:{path}, {systemTypeInstance.FullName}");
                }
            }

            // [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate),
            //     new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
            // public class ObjectInstantiatePatcher01
            // {
            //     public static void Postfix(ref UnityEngine.Object __result,
            //         UnityEngine.Object original, Vector3 position, Quaternion rotation)
            //     {
            //         log.LogMessage($"[Object.Instantiate01] {original.name}");
            //     }
            // }
        }
        
        public void AdjustCosts()
        {
            // var prefabs = Managers.Inst.prefabs;
            // if (prefabs != null)
            // {
            //     log.LogMessage("Handle prefabs start.");
            //
            //     var prefabIds = new List<PrefabIDs>
            //     {
            //         PrefabIDs.Citizen_House,
            //         PrefabIDs.Workshop,
            //         PrefabIDs.Tower_Baker,
            //         PrefabIDs.Tower6,
            //         PrefabIDs.Tower5,
            //         PrefabIDs.Tower4,
            //         PrefabIDs.Tower3,
            //         PrefabIDs.Tower2,
            //         PrefabIDs.Tower1,
            //         PrefabIDs.Tower0
            //     };
            //     foreach (var prefabId in prefabIds)
            //     {
            //         if (prefabId == PrefabIDs.MerchantHouse) continue;
            //         var go = prefabs.GetPrefabById((int)prefabId);
            //         if (go == null) continue;
            //         var prefab = go.GetComponent<PrefabID>();
            //         if (prefab == null) continue;
            //
            //         HandlePayable(go, (PrefabIDs)prefab.prefabID, true);
            //     }
            //
            //     log.LogMessage("Handle prefabs end.");
            // }

            var payables = Managers.Inst.payables;
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
                { PrefabIDs.Tower5, new ModifyData(12, 80, -1, 3) },
                { PrefabIDs.Tower4, new ModifyData(8, 70) },
                { PrefabIDs.Tower3, new ModifyData(8, 50, 5) },
                { PrefabIDs.Tower2, new ModifyData(5, 30, -1, 0) },
                { PrefabIDs.Tower1, new ModifyData(4, 20) },
                { PrefabIDs.Tower0, new ModifyData(2, 10, 2) },
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
                            log.LogMessage($"Change {go.name} price from {payable.price} to 3");
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
                                log.LogMessage($"Change {go.name} price from {payable.price} to 3");
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
                            log.LogMessage($"Change {go.name} price from {payable.price} to 2");
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
            if (isPrefab)
            {
                if (TowerPrefabs[index] != null)
                    return;
                TowerPrefabs[index] = go;
            }

            var modifyData = ModifyDataDict[prefabId];

            if (modifyBuildPoints)
            {
                var workable = go.GetComponent<WorkableBuilding>();
                if (workable != null)
                {
                    log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to {modifyData.BuildPoints}");
                    workable.buildPoints = modifyData.BuildPoints;
                }
            }

            var payable = go.GetComponent<PayableUpgrade>();
            if (payable != null)
            {
                log.LogMessage($"Change {go.name} price from {payable.price} to {modifyData.Price}");
                payable.price = modifyData.Price;

                if (modifyData.NextPrefab != -1 && TowerPrefabs[modifyData.NextPrefab] != null)
                {
                    payable.nextPrefab = TowerPrefabs[modifyData.NextPrefab];
                    log.LogMessage($"Change Tower{index} nextPrefab to Tower{modifyData.NextPrefab}");
                }

                if (isPrefab && modifyData.LastPrefab != -1 && TowerPrefabs[modifyData.LastPrefab] != null)
                {
                    var payableUpgrade = TowerPrefabs[modifyData.LastPrefab].GetComponent<PayableUpgrade>();
                    if (payableUpgrade != null)
                    {
                        if (payableUpgrade.nextPrefab != go)
                        {
                            payableUpgrade.nextPrefab = go;

                            log.LogMessage($"Change Tower{modifyData.LastPrefab} nextPrefab to Tower{index}");
                        }
                    }
                }

                if (payable.nextPrefab != null)
                {
                    HandlePayable(payable.nextPrefab, true);
                }
            }
        }

        public struct ModifyData
        {
            public int Price;
            public int BuildPoints;
            public int NextPrefab;
            public int LastPrefab;

            public ModifyData(int price, int buildPoints = 0, int nextPrefab = -1, int lastPrefab = -1)
            {
                Price = price; BuildPoints = buildPoints; NextPrefab = nextPrefab; LastPrefab = lastPrefab;
            }
        }

        public enum PrefabIDs
        {
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
