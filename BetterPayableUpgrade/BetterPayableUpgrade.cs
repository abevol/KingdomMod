using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace KingdomMod
{
    public class BetterPayableUpgrade : MonoBehaviour
    {
        private static ManualLogSource log;
        private bool enableStaminaBar = true;

        public static void Initialize(BetterPayableUpgradePlugin plugin)
        {
            log = plugin.Log;
            var component = plugin.AddComponent<BetterPayableUpgrade>();
            component.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(component.gameObject);
        }

        public BetterPayableUpgrade()
        {

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

            AdjustCosts();
        }

        public void AdjustCosts()
        {
            // TODO: try to hook Resources.Load
            // TODO: try to hook Pool.SpawnOrInstantiateGO
            // TODO: try to hook UnityEngine.Object.Instantiate

            var prefabs = Managers.Inst.prefabs;
            if (prefabs != null)
            {
                log.LogMessage("Handle prefabs start.");

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
                    var prefab = go.GetComponent<PrefabID>();
                    if (prefab == null) continue;

                    HandlePayable(go, (PrefabIDs)prefab.prefabID);
                }

                log.LogMessage("Handle prefabs end.");
            }

            var payables = Managers.Inst.payables;
            if (payables != null)
            {
                foreach (var obj in payables.AllPayables)
                {
                    if (obj == null) continue;
                    var go = obj.gameObject;
                    if (go == null) continue;
                    var prefab = go.GetComponent<PrefabID>();
                    if (prefab == null) continue;

                    HandlePayable(go, (PrefabIDs)prefab.prefabID);
                }

                foreach (var obj in payables._allBlockers)
                {
                    if (obj == null) continue;
                    var scaffolding = obj.GetComponent<Scaffolding>();
                    if (scaffolding == null) continue;
                    var go = scaffolding.building;
                    if (go == null) continue;
                    var prefab = go.GetComponent<PrefabID>();
                    if (prefab == null) continue;

                    HandlePayable(go, (PrefabIDs)prefab.prefabID);
                }
            }

        }

        private void HandlePayable(GameObject go, PrefabIDs prefabId)
        {
            var prefabs = Managers.Inst.prefabs;
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
                        var payable = go.GetComponent<PayableUpgrade>();
                        if (payable != null)
                        {
                            log.LogMessage($"Change {go.name} price from {payable.price} to 16");
                            payable.price = 16;
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 100");
                            workable.buildPoints = 100;
                        }
                        break;
                    }
                case PrefabIDs.Tower5:
                    {
                        var payable = go.GetComponent<PayableUpgrade>();
                        if (payable != null)
                        {
                            log.LogMessage($"Change {go.name} price from {payable.price} to 12");
                            payable.price = 12;
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 100");
                            workable.buildPoints = 70;
                        }
                        break;
                    }
                case PrefabIDs.Tower4:
                    {
                        var payable = go.GetComponent<PayableUpgrade>();
                        if (payable != null)
                        {
                            log.LogMessage($"Change {go.name} price from {payable.price} to 8");
                            payable.price = 8;
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 70");
                            workable.buildPoints = 70;
                        }
                        break;
                    }
                case PrefabIDs.Tower3:
                    {
                        var payable = go.GetComponent<PayableUpgrade>();
                        if (payable != null)
                        {
                            log.LogMessage($"Change {go.name} price from {payable.price} to 8");
                            payable.price = 8;
                            payable.nextPrefab = prefabs.GetPrefabById((int)PrefabIDs.Tower5);
                            // payable.SetNextPrefab((int)PrefabIDs.Tower5);
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 50");
                            workable.buildPoints = 50;
                        }
                        break;
                    }
                case PrefabIDs.Tower2:
                    {
                        var payable = go.GetComponent<PayableUpgrade>();
                        if (payable != null)
                        {
                            log.LogMessage($"Change {go.name} price from {payable.price} to 5");
                            payable.price = 5;
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 30");
                            workable.buildPoints = 30;
                        }
                        break;
                    }
                case PrefabIDs.Tower1:
                    {
                        var payable = go.GetComponent<PayableUpgrade>();
                        if (payable != null)
                        {
                            log.LogMessage($"Change {go.name} price from {payable.price} to 4");
                            payable.price = 4;
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 20");
                            workable.buildPoints = 20;
                        }
                        break;
                    }
                case PrefabIDs.Tower0:
                    {
                        var payable = go.GetComponent<PayableUpgrade>();
                        if (payable != null)
                        {
                            log.LogMessage($"Change {go.name} price from {payable.price} to 2");
                            payable.price = 2;
                            //payable.SetNextPrefab((int)PrefabIDs.Tower2);
                            payable.nextPrefab = prefabs.GetPrefabById((int)PrefabIDs.Tower2);
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 10");
                            workable.buildPoints = 10;
                        }
                        break;
                    }
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
