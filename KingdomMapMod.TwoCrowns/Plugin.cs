using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using File = System.IO.File;

namespace KingdomMapMod.TwoCrowns
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("KingdomTwoCrowns.exe")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            try
            {
                // debug localization

                // string myCulture = "en-US";
                // Strings.Culture = CultureInfo.GetCultureInfo(myCulture);

                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

                PluginBase.Initialize(this);
                StaminaBar.Initialize(this);
            }
            catch (Exception e)
            {
                Log.LogInfo(e);
                throw;
            }
        }
    }

    public class PluginBase : MonoBehaviour
    {
        private static ManualLogSource log;
        private static readonly GUIStyle guiStyle = new();
        private int tick = 0;
        private bool enableDebugInfo = false;
        private bool enableMinimap = true;
        private List<DebugInfo> debugInfoList = new();
        private List<MarkInfo> minimapMarkList = new();
        private List<LineInfo> drawLineList = new();
        private StatsInfo statsInfo = new();
        private float exploredLeft = 0;
        private float exploredRight = 0;
        private bool showFullMap = false;

        public PluginBase()
        {
            try
            {
                guiStyle.alignment = TextAnchor.UpperLeft;
                guiStyle.normal.textColor = Color.white;
                guiStyle.fontSize = 12;
            }
            catch (Exception exception)
            {
                log.LogInfo(exception);
                throw;
            }
        }

        public static void Initialize(Plugin plugin)
        {
            log = plugin.Log;
            var addComponent = plugin.AddComponent<PluginBase>();
            addComponent.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(addComponent.gameObject);
        }

        private void Start()
        {
            log.LogMessage("Start.");
            Game.add_OnGameStart((Action)OnGameStart);
            
            GlobalSaveData.add_OnCurrentCampaignSwitch((Action)OnCurrentCampaignSwitch);

            log.LogMessage($"CoinBag.OverflowLimit: {CoinBag.OverflowLimit}");
            CoinBag.OverflowLimit = 1000;
            log.LogMessage($"CoinBag.OverflowLimit: {CoinBag.OverflowLimit}");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                log.LogMessage("M key pressed.");
                enableMinimap = !enableMinimap;
            }

            if (Input.GetKeyDown(KeyCode.End))
            {
                log.LogMessage("End key pressed.");
                enableDebugInfo = !enableDebugInfo;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                showFullMap = !showFullMap;
            }

            if (Input.GetKeyDown(KeyCode.Insert))
            {
                log.LogMessage("Insert key pressed.");

                var cursorSystem = GameObject.FindObjectOfType<CursorSystem>();
                if (cursorSystem)
                    cursorSystem.SetForceVisibleCursor(true);

                var biomeHelper = GameObject.Find("BiomeHelper");
                if (biomeHelper)
                {
                    var biomeHolder = biomeHelper.GetComponent<BiomeHolder>();
                    if (biomeHolder)
                    {
                        biomeHolder.debugToolsEnabled = true;
                    }
                }

                InterfaceOverlay.DebugDisable = false;

                // DebugTools.Inst.OpenButtonPressed();
                //
                // var debugTools = GameObject.Find("DebugTools");
                // if (debugTools)
                // {
                //     var iDebugTools = new IDebugTools(debugTools.Pointer);
                //     iDebugTools.OpenButtonPressed();
                //
                //     // debugTools.SetState(DebugTools.State.Open);
                //     // var component = debugTools.GetComponent<DebugTools>();
                //     // if (component)
                //     // {
                //     //     component.SetState(DebugTools.State.Open);
                //     //     Plugin.Logger.LogInfo("DebugTools SetState Open");
                //     // }
                // }

                var teleportPanel = GameObject.Find("Teleport Panel");
                if (teleportPanel)
                    teleportPanel.SetActive(true);

            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                List<GameObjectDetails> objectTree = new List<GameObjectDetails>();

                log.LogMessage("Dumping All Objects to JSON using GetAllScenesGameObject()...");
                foreach (var obj in GameObjectDetails.GetAllScenesGameObjects())
                {
                    objectTree.Add(new GameObjectDetails(obj));
                }

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\AllObjectsDump.json",
                    GameObjectDetails.JsonSerialize(objectTree));

                log.LogMessage("Complete!");
                log.LogMessage("JSON written to " +
                               (AppDomain.CurrentDomain.BaseDirectory + "\\AllObjectsDump.json").Replace("\\\\", "\\"));

                string path = (AppDomain.CurrentDomain.BaseDirectory + "\\AllObjectsDump.json").Replace("\\\\", "\\");
                Application.OpenURL("file:///" + path.Replace("\\", "/"));
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                log.LogMessage($"Dump Prefabs:");

                var prefabList = GameObject.FindObjectsOfType<PrefabID>();
                foreach (var prefab in prefabList)
                {
                    log.LogMessage($"PrefabID:{prefab.prefabID}, name: {prefab.name}");
                }

                var prefabs = Managers.Inst.prefabs;
                if (prefabs != null)
                {
                    var dumpStr = "\npublic enum PrefabIDs\n{\n";
                    foreach (var prefab in prefabs.prefabMasterCopies)
                    {
                        var prefabName = prefab.name.Replace(' ', '_');
                        dumpStr = dumpStr + "    " + prefabName + " = " + prefab.prefabID + ",\n";
                    }

                    dumpStr = dumpStr + "}\n";
                    log.LogMessage(dumpStr);
                }
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                log.LogMessage($"Dump LevelBlocks:");

                var levelBlocks = Managers.Inst.level.GetLevelBlocks();
                log.LogMessage($"LevelBlocks: {levelBlocks.Count}");
                foreach (var levelBlock in levelBlocks)
                {
                    log.LogMessage($"groups: {levelBlock.groupOne}, {levelBlock.groupTwo}, {levelBlock.groupThree}");
                }
                
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                log.LogMessage($"Try to reset last day.");

            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                log.LogMessage($"Try to reload game.");

                Managers.Inst.game.Reload();
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                log.LogMessage($"Try to save game.");

                Managers.Inst.game.TriggerSave();
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                var player = Managers.Inst.kingdom.playerOne;
                if (player != null)
                {
                    var payable = player.selectedPayable;
                    if (payable != null)
                    {
                        log.LogMessage($"Try to destroy the game object: {payable.GetGO.name}");
                        Destroy(payable.GetGO);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                var kingdom = Managers.Inst.kingdom;
                if (kingdom != null)
                {
                    log.LogMessage($"Try to add Peasant.");

                    var prefab = Resources.Load<Peasant>("Prefabs/Characters/Peasant").gameObject;
                    Vector3 vector = kingdom.playerOne.transform.TransformPoint(new Vector3(1f, 1f, 0.01f));
                    var lastSpawned = Pool.SpawnOrInstantiateGO(prefab, vector, Quaternion.identity, null);
                    lastSpawned.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
                }
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                var kingdom = Managers.Inst.kingdom;
                if (kingdom != null)
                {
                    log.LogMessage($"Try to add Griffin.");

                    var prefab = Resources.Load<Steed>("Prefabs/Steeds/Griffin P1");
                    prefab.price = 1;
                    Vector3 vector = kingdom.playerOne.transform.TransformPoint(new Vector3(1f, 1f, 0.01f));
                    var lastSpawned = Pool.SpawnOrInstantiateGO(prefab.gameObject, vector, Quaternion.identity, null);
                    lastSpawned.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
                }
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                var kingdom = Managers.Inst.kingdom;
                if (kingdom != null)
                {
                    log.LogMessage($"Try to add Wall0.");

                    // var prefab = Resources.Load<Wall>("prefabs/buildings and interactive/wall0");
                    Vector3 vector = new Vector3(kingdom.playerOne.transform.position.x, 0.0f, 0.1f);

                    Wall wall = Instantiate<Wall>(Managers.Inst.holder.wallPrefabs[0]);
                    wall.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
                    wall.transform.position = vector;

                    // var lastSpawned = Pool.SpawnOrInstantiateGO(prefab.gameObject, vector, Quaternion.identity, GameObject.FindGameObjectWithTag("GameLayer").transform);
                    // lastSpawned.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
                }
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                var kingdom = Managers.Inst.kingdom;
                if (kingdom != null)
                {
                    log.LogMessage($"Try to add Tower0.");

                    var prefab = Resources.Load<Tower>("prefabs/buildings and interactive/tower0");
                    Vector3 vector = new Vector3(kingdom.playerOne.transform.position.x, 0.0f, 1.6f);
                    var lastSpawned = Pool.SpawnOrInstantiateGO(prefab.gameObject, vector, Quaternion.identity, GameObject.FindGameObjectWithTag("GameLayer").transform);
                    // lastSpawned.transform.SetParent(GameObject.FindGameObjectWithTag("GameLayer").transform);
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                var player = Managers.Inst.kingdom.playerOne;
                player.TeleportFlash();
                player.coinLaunchSound.Play(player.transform.position, 1);

                GameObject gameObject = Pool.SpawnGO(Resources.Load<Boulder>("Prefabs/Objects/Boulder").gameObject, Vector3.zero, Quaternion.identity);
                var compCacher = Managers.Inst.compCacher;
                compCacher.GetCachedComponent<Boulder>(gameObject)._launchedByEnemies = false;
                compCacher.GetCachedComponent<Boulder>(gameObject).maxHitCitizens = 1000;
                compCacher.GetCachedComponent<Boulder>(gameObject).StuckProbability = 0f;
                compCacher.GetCachedComponent<Boulder>(gameObject).hitDamage = 300;
                compCacher.GetCachedComponent<SpriteRendererFX>(gameObject).FadeIn(0.5f);
                gameObject.transform.parent = player.transform.parent;
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localRotation = Quaternion.identity;
                compCacher.GetCachedComponent<Boulder>(gameObject).SetFake();
                gameObject.transform.SetParent(player.transform.parent, true);
                gameObject.transform.localScale = new Vector3(3f, 3f, 3f);
                gameObject.transform.position = player.transform.position;
                gameObject.transform.rotation = Quaternion.LookRotation(player.transform.forward);

                Vector2 vector6 = new Vector2((int)player.mover.GetDirection(), 0.2f);
                Vector2 vector7 = vector6.normalized * 14f;
                compCacher.GetCachedComponent<Boulder>(gameObject).Launch(vector7, player.gameObject);
                Pool.Despawn(gameObject, 5f);
            }

            tick = tick + 1;
            if (tick > 60)
                tick = 0;

            if (tick == 0)
            {
                if (!IsPlaying()) return;

                if (enableMinimap)
                {
                    UpdateMinimapMarkList();
                    UpdateStatsInfo();
                }

                if (enableDebugInfo)
                    UpdateDebugInfo();

            }
        }

        private bool IsPlaying()
        {
            if (!Managers._Inst) return false;
            if (!Managers._Inst.game) return false;
            return Managers._Inst.game.playingOrInMenuWithClient;
        }

        private void OnGUI()
        {
            if (!IsPlaying()) return;

            if (enableMinimap)
            {
                DrawMinimap();
                DrawStatsInfo();
                DrawExtraInfo();
            }

            if (enableDebugInfo)
                DrawDebugInfo();
        }

        public void OnGameStart()
        {
            log.LogMessage("OnGameStart.");

            exploredLeft = exploredRight = 0;

            AdjustCosts();
        }

        public Pool GetPoolByPrefabId(PrefabIDs prefabId)
        {
            log.LogMessage($"Pool.poolsByPrefab.Count: {Pool.poolsByPrefab.Count}");
            foreach (var poolPair in Pool.poolsByPrefab)
            {
                var pool = poolPair.value;
                if (pool == null) continue;
                var prefab = poolPair.key;
                if (prefab == null) continue;
                log.LogMessage($"Found prefab {prefab.name}");
                var prefabIdComp = prefab.GetComponent<PrefabID>();
                if (prefabIdComp == null) continue;
                log.LogMessage($"[GetPoolByPrefabId] 3");
                if (prefabIdComp.prefabID == (int)prefabId)
                {
                    log.LogMessage($"Found Pool for prefab {prefab.name}");
                    return pool;
                }
            }
            return null;
        }

        public void AdjustCosts()
        {
            var prefabs = Managers.Inst.prefabs;
            if (prefabs != null)
            {
                log.LogMessage("Handle prefabs start.");

                var prefabIds = new List<PrefabIDs>
                {
                    PrefabIDs.Citizen_House,
                    PrefabIDs.Workshop,
                    PrefabIDs.Tower_Baker,
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

                    switch ((PrefabIDs)prefab.prefabID)
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
                        case PrefabIDs.Tower5:
                            {
                                var payable = go.GetComponent<PayableUpgrade>();
                                if (payable != null)
                                {
                                    log.LogMessage($"Change {go.name} price from {payable.price} to 10");
                                    payable.price = 10;
                                }

                                var workable = go.GetComponent<WorkableBuilding>();
                                if (workable != null)
                                {
                                    log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 100");
                                    workable.buildPoints = 100;
                                }

                                var pool = GetPoolByPrefabId(PrefabIDs.Tower5);
                                if (pool != null)
                                {
                                    pool.prefab.gameObject.GetComponent<PayableUpgrade>().price = 10;
                                    pool.prefab.gameObject.GetComponent<WorkableBuilding>().buildPoints = 100;
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

                                var pool = GetPoolByPrefabId(PrefabIDs.Tower4);
                                if (pool != null)
                                {
                                    pool.prefab.gameObject.GetComponent<PayableUpgrade>().price = 8;
                                    pool.prefab.gameObject.GetComponent<WorkableBuilding>().buildPoints = 70;
                                }
                                break;
                            }
                        case PrefabIDs.Tower3:
                            {
                                var payable = go.GetComponent<PayableUpgrade>();
                                if (payable != null)
                                {
                                    log.LogMessage($"Change {go.name} price from {payable.price} to 8");
                                    // payable.price = 6;
                                    payable.price = 8;
                                    payable.nextPrefab = prefabs.GetPrefabById((int)PrefabIDs.Tower5);
                                }

                                var workable = go.GetComponent<WorkableBuilding>();
                                if (workable != null)
                                {
                                    log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 50");
                                    workable.buildPoints = 50;
                                }

                                var pool = GetPoolByPrefabId(PrefabIDs.Tower3);
                                if (pool != null)
                                {
                                    pool.prefab.gameObject.GetComponent<PayableUpgrade>().price = 8;
                                    pool.prefab.gameObject.GetComponent<WorkableBuilding>().buildPoints = 50;
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

                                var pool = GetPoolByPrefabId(PrefabIDs.Tower2);
                                if (pool != null)
                                {
                                    pool.prefab.gameObject.GetComponent<PayableUpgrade>().price = 5;
                                    pool.prefab.gameObject.GetComponent<WorkableBuilding>().buildPoints = 30;
                                }
                                // Instantiate(new GuardSlot().gameObject, go.transform);
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

                                var pool = GetPoolByPrefabId(PrefabIDs.Tower2);
                                if (pool != null)
                                {
                                    pool.prefab.gameObject.GetComponent<PayableUpgrade>().price = 4;
                                    pool.prefab.gameObject.GetComponent<WorkableBuilding>().buildPoints = 20;
                                }
                                break;
                            }
                        case PrefabIDs.Tower0:
                            {
                                var payable = go.GetComponent<PayableUpgrade>();
                                if (payable != null)
                                {
                                    log.LogMessage($"Change {go.name} price from {payable.price} to 3");
                                    payable.price = 3;
                                    payable.nextPrefab = prefabs.GetPrefabById((int)PrefabIDs.Tower2);
                                }

                                var workable = go.GetComponent<WorkableBuilding>();
                                if (workable != null)
                                {
                                    log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 10");
                                    workable.buildPoints = 10;
                                }

                                var pool = GetPoolByPrefabId(PrefabIDs.Tower2);
                                if (pool != null)
                                {
                                    pool.prefab.gameObject.GetComponent<PayableUpgrade>().price = 2;
                                    pool.prefab.gameObject.GetComponent<WorkableBuilding>().buildPoints = 10;
                                }
                                break;
                            }
                    }
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

                    if (prefab.prefabID == (int)PrefabIDs.Citizen_House)
                    {
                        var citizenHousePayable = go.GetComponent<CitizenHousePayable>();
                        if (citizenHousePayable != null)
                        {
                            log.LogMessage($"Change {prefab.name} price from {citizenHousePayable.price} to 3");
                            citizenHousePayable.price = 3;
                        }
                    }
                    else if (prefab.prefabID == (int)PrefabIDs.Workshop)
                    {
                        var payableWorkshop = go.GetComponent<PayableWorkshop>();
                        if (payableWorkshop != null)
                        {
                            var payableWorkshopBarrel = payableWorkshop.barrelCounterpart;
                            if (payableWorkshopBarrel != null)
                            {
                                log.LogMessage($"Change {prefab.name} price from {payableWorkshopBarrel.price} to 3");
                                payableWorkshopBarrel.price = 3;
                            }
                        }
                    }
                    else if (prefab.prefabID == (int)PrefabIDs.Tower_Baker)
                    {
                        var payableShop = go.GetComponent<PayableShop>();
                        if (payableShop != null)
                        {
                            log.LogMessage($"Change {prefab.name} price from {payableShop.price} to 2");
                            payableShop.price = 2;
                        }
                    }
                    else if (prefab.prefabID == (int)PrefabIDs.Tower0)
                    {
                        var payableUpgrade = go.GetComponent<PayableUpgrade>();
                        if (payableUpgrade != null)
                        {
                            log.LogMessage($"Change {prefab.name} price from {payableUpgrade.price} to 2");
                            payableUpgrade.price = 2;
                            // var nextPrefab = prefabs.GetPrefabById((int)PrefabIDs.Tower2);
                            // nextPrefab.gameObject.GetComponent<PayableUpgrade>().price = 5;
                            // nextPrefab.gameObject.GetComponent<WorkableBuilding>().buildPoints = 30;
                            // payableUpgrade.nextPrefab = nextPrefab;
                            payableUpgrade.SetNextPrefab((int)PrefabIDs.Tower2);
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 10");
                            workable.buildPoints = 10;
                        }
                    }
                    else if (prefab.prefabID == (int)PrefabIDs.Tower1)
                    {
                        var payableUpgrade = go.GetComponent<PayableUpgrade>();
                        if (payableUpgrade != null)
                        {
                            log.LogMessage($"Change {prefab.name} price from {payableUpgrade.price} to 4");
                            payableUpgrade.price = 4;
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 20");
                            workable.buildPoints = 20;
                        }
                    }
                    else if (prefab.prefabID == (int)PrefabIDs.Tower2)
                    {
                        var payableUpgrade = go.GetComponent<PayableUpgrade>();
                        if (payableUpgrade != null)
                        {
                            log.LogMessage($"Change {prefab.name} price from {payableUpgrade.price} to 5");
                            payableUpgrade.price = 5;
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 30");
                            workable.buildPoints = 30;
                        }

                        // Instantiate(new GuardSlot(), go.transform);
                    }
                    else if (prefab.prefabID == (int)PrefabIDs.Tower3)
                    {
                        var payableUpgrade = go.GetComponent<PayableUpgrade>();
                        if (payableUpgrade != null)
                        {
                            log.LogMessage($"Change {prefab.name} price from {payableUpgrade.price} to 6");
                            // payableUpgrade.price = 6;
                            payableUpgrade.price = 8;
                            payableUpgrade.nextPrefab = prefabs.GetPrefabById((int)PrefabIDs.Tower5);
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 50");
                            workable.buildPoints = 50;
                        }
                    }
                    else if (prefab.prefabID == (int)PrefabIDs.Tower4)
                    {
                        var payableUpgrade = go.GetComponent<PayableUpgrade>();
                        if (payableUpgrade != null)
                        {
                            log.LogMessage($"Change {prefab.name} price from {payableUpgrade.price} to 8");
                            payableUpgrade.price = 8;
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 70");
                            workable.buildPoints = 70;
                        }
                    }
                    else if (prefab.prefabID == (int)PrefabIDs.Tower5)
                    {
                        var payableUpgrade = go.GetComponent<PayableUpgrade>();
                        if (payableUpgrade != null)
                        {
                            log.LogMessage($"Change {prefab.name} price from {payableUpgrade.price} to 10");
                            payableUpgrade.price = 10;
                        }

                        var workable = go.GetComponent<WorkableBuilding>();
                        if (workable != null)
                        {
                            log.LogMessage($"Change {go.name} buildPoints from {workable.buildPoints} to 100");
                            workable.buildPoints = 100;
                        }
                    }
                }
            }

        }

        public void OnCurrentCampaignSwitch()
        {
            log.LogMessage($"OnCurrentCampaignSwitch: {GlobalSaveData.loaded.currentCampaign}");

        }

        private void UpdateMinimapMarkList()
        {
            var world = Managers.Inst.world;
            if (world == null) return;
            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return;
            var payables = Managers.Inst.payables;
            if (payables == null) return;

            minimapMarkList.Clear();
            var poiList = new List<MarkInfo>();
            var leftWalls = new List<WallPoint>();
            var rightWalls = new List<WallPoint>();

            Portal dock = null;
            foreach (var obj in kingdom.AllPortals)
            {
                if (obj.type == Portal.Type.Regular)
                    poiList.Add(new MarkInfo(obj.transform.position.x, new Color(0.62f, 0.0f, 1.0f), Strings.Portal));
                else if (obj.type == Portal.Type.Cliff)
                    poiList.Add(new MarkInfo(obj.transform.position.x, obj.state switch{ Portal.State.Destroyed => Color.green, Portal.State.Rebuilding => Color.blue, _=> Color.red}, Strings.Cliff));
                else if (obj.type == Portal.Type.Dock)
                    dock = obj;
            }

            var beach = GameObject.FindObjectOfType<Beach>();
            if (beach != null)
                poiList.Add(new MarkInfo(beach.transform.position.x, (dock && (dock.state != Portal.State.Destroyed)) ? Color.red : Color.green, Strings.Dock));

            foreach (var beggarCamp in kingdom.BeggarCamps)
            {
                int count = 0;
                foreach (var beggar in beggarCamp._beggars)
                {
                    if (beggar != null && beggar.isActiveAndEnabled)
                        count++;
                }
                poiList.Add(new MarkInfo(beggarCamp.transform.position.x, Color.white, Strings.BeggarCamp, count));
            }

            foreach (var beggar in kingdom.beggars)
            {
                if (beggar == null) continue;

                if (beggar.hasFoundBaker)
                {
                    poiList.Add(new MarkInfo(beggar.transform.position.x, Color.red, Strings.Beggar, 0, MarkRow.Movable));
                }
            }

            var mover = kingdom.playerOne.mover;
            if (mover != null)
            {
                poiList.Add(new MarkInfo(mover.transform.position.x, Color.green, Strings.You, 0, MarkRow.Movable));
                float l = mover.transform.position.x - 12;
                float r = mover.transform.position.x + 12;
                if (l < exploredLeft)
                    exploredLeft = l;
                if (r > exploredRight)
                    exploredRight = r;
            }

            var steed = kingdom.playerOne.steed;
            if (steed != null)
            {
                var deerHunters = new List<Steed.SteedType>
                {
                    Steed.SteedType.Stag, 
                    Steed.SteedType.P1Wolf, 
                    Steed.SteedType.Reindeer, 
                    Steed.SteedType.Reindeer_Norselands
                };
                var steedType = steed.steedType;
                if (deerHunters.Contains(steedType))
                {
                    var deers = GameObject.FindObjectsOfType<Deer>();
                    foreach (var deer in deers)
                    {
                        poiList.Add(new MarkInfo(deer.transform.position.x, deer._fsm.current == 5 ? Color.green : Color.blue, Strings.Deer, 0, MarkRow.Movable));
                    }
                }
            }

            var enemies = Managers.Inst.enemies._enemies;
            if (enemies != null && enemies.Count > 0)
            {
                var youX = mover.transform.position.x;
                var leftEnemies = new List<float>();
                var rightEnemies = new List<float>();
                var leftBosses = new List<float>();
                var rightBosses = new List<float>();
                foreach (var enemy in enemies)
                {
                    if (enemy == null) continue;
                    var enemyX = enemy.transform.position.x;
                    if (enemyX < youX)
                    {
                        leftEnemies.Add(enemyX);
                        if (enemy.GetComponent<Boss>() != null)
                            leftBosses.Add(enemyX);
                    }
                    else
                    {
                        rightEnemies.Add(enemyX);
                        if (enemy.GetComponent<Boss>() != null)
                            rightBosses.Add(enemyX);
                    }
                }

                if (leftEnemies.Count > 0)
                {
                    var drawEnemies = true;
                    leftEnemies.Sort((a, b) => b.CompareTo(a));
                    if (leftBosses.Count > 0)
                    {
                        leftBosses.Sort((a, b) => b.CompareTo(a));
                        poiList.Add(new MarkInfo(leftBosses[0], Color.red, Strings.Boss, leftBosses.Count, MarkRow.Movable));
                        if (leftEnemies[0] - leftBosses[0] < 6)
                            drawEnemies = false;
                    }

                    if (drawEnemies)
                        poiList.Add(new MarkInfo(leftEnemies[0], Color.red, Strings.Enemy, leftEnemies.Count, MarkRow.Movable));
                }

                if (rightEnemies.Count > 0)
                {
                    var drawEnemies = true;
                    rightEnemies.Sort((a, b) => a.CompareTo(b));

                    if (rightBosses.Count > 0)
                    {
                        rightBosses.Sort((a, b) => a.CompareTo(b));
                        poiList.Add(new MarkInfo(rightBosses[0], Color.red, Strings.Boss, rightBosses.Count, MarkRow.Movable));
                        if (rightBosses[0] - rightEnemies[0] < 6)
                            drawEnemies = false;
                    }

                    if (drawEnemies)
                        poiList.Add(new MarkInfo(rightEnemies[0], Color.red, Strings.Enemy, rightEnemies.Count, MarkRow.Movable));
                }
            }

            var castle = kingdom.castle;
            if (castle != null)
            {
                var price = castle._payableUpgrade.CanPay(kingdom.playerOne) ? castle._payableUpgrade.price : 0;
                poiList.Add(new MarkInfo(castle.transform.position.x, Color.green, Strings.Castle, price));
                poiList.Add(new MarkInfo(castle.transform.position.x, Color.green, Strings.CastleSign, 0, MarkRow.Sign));
                leftWalls.Add(new WallPoint(castle.transform.position, Color.green));
                rightWalls.Add(new WallPoint(castle.transform.position, Color.green));
            }
            
            var campfire = kingdom.campfire;
            if (campfire !=  null)
            {
                poiList.Add(new MarkInfo(campfire.transform.position.x, Color.white, Strings.Campfire));
            }

            var chestList = GameObject.FindObjectsOfType<Chest>();
            foreach (var obj in chestList)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Color.white, obj.isGems ? Strings.GemChest : Strings.Chest, obj.coins));
            }

            foreach (var obj in kingdom._walls)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Color.green, Strings.Wall, 0, MarkRow.Sign));
                if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(obj.transform.position, Color.green));
                else
                    rightWalls.Add(new WallPoint(obj.transform.position, Color.green));
            }

            var shopForge = GameObject.FindGameObjectWithTag(Tags.ShopForge);
            if (shopForge != null)
            {
                poiList.Add(new MarkInfo(shopForge.transform.position.x, Color.white, Strings.ShopForge));
            }

            var citizenHouses = GameObject.FindGameObjectsWithTag(Tags.CitizenHouse);
            foreach (var obj in citizenHouses)
            {
                var citizenHouse = obj.GetComponent<CitizenHousePayable>();
                if (citizenHouse != null)
                {
                    poiList.Add(new MarkInfo(obj.transform.position.x, Color.white, Strings.CitizenHouse, citizenHouse._numberOfAvaliableCitizens));
                }
            }

            var wallWreckList = GameObject.FindGameObjectsWithTag(Tags.WallWreck);
            foreach (var obj in wallWreckList)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Color.red, Strings.WallWreck, 0, MarkRow.Sign));
                if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(obj.transform.position, Color.red));
                else
                    rightWalls.Add(new WallPoint(obj.transform.position, Color.red));
            }

            var wallFoundation = GameObject.FindGameObjectsWithTag(Tags.WallFoundation);
            foreach (var obj in wallFoundation)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Color.gray, Strings.WallFoundation, 0, MarkRow.Sign));
            }

            var riverList = GameObject.FindObjectsOfType<River>();
            foreach (var obj in riverList)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, new Color(0.46f, 0.84f, 0.92f), Strings.River, 0, MarkRow.Sign));
            }

            foreach (var obj in Managers.Inst.world._berryBushes)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, obj.paid ? Color.green : Color.red, Strings.BerryBush, 0, MarkRow.Sign));
            }

            var payableGemChest = GameObject.FindObjectOfType<PayableGemChest>();
            if (payableGemChest != null)
            {
                var gemsCount = payableGemChest.infiniteGems ? payableGemChest.guardRef.price : payableGemChest.gemsStored;
                poiList.Add(new MarkInfo(payableGemChest.transform.position.x, Color.white, Strings.GemMerchant, gemsCount));
            }

            var dogSpawn = GameObject.FindObjectOfType<DogSpawn>();
            if (dogSpawn != null && !dogSpawn._dogFreed)
                poiList.Add(new MarkInfo(dogSpawn.transform.position.x, Color.red, Strings.DogSpawn));

            var boarSpawn = world.boarSpawnGroup;
            if (boarSpawn != null)
            {
                poiList.Add(new MarkInfo(boarSpawn.transform.position.x, Color.red, Strings.BoarSpawn, boarSpawn._spawnedBoar ? 1 : 0));
            }

            var caveHelper = Managers.Inst.caveHelper;
            if (caveHelper != null && caveHelper.CurrentlyBombingPortal != null)
            {
                var bomb = caveHelper.Getbomb(caveHelper.CurrentlyBombingPortal.side);
                if (bomb != null)
                {
                    poiList.Add(new MarkInfo(bomb.transform.position.x, Color.green, Strings.Bomb, 0, MarkRow.Movable));
                }
            }

            foreach (var obj in kingdom._farmHouses)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Color.green, Strings.Farmhouse));
            }

            var steedNames = new Dictionary<Steed.SteedType, string>
                    {
                        { Steed.SteedType.Bear,                  Strings.Bear },
                        { Steed.SteedType.P1Griffin,             Strings.Griffin },
                        { Steed.SteedType.Lizard,                Strings.Lizard },
                        { Steed.SteedType.Reindeer,              Strings.Reindeer },
                        { Steed.SteedType.Spookyhorse,           Strings.Spookyhorse },
                        { Steed.SteedType.Stag,                  Strings.Stag },
                        { Steed.SteedType.Unicorn,               Strings.Unicorn },
                        { Steed.SteedType.P1Warhorse,            Strings.Warhorse },
                        { Steed.SteedType.P1Default,             Strings.DefaultSteed },
                        { Steed.SteedType.P2Default,             Strings.DefaultSteed },
                        { Steed.SteedType.HorseStamina,          Strings.HorseStamina },
                        { Steed.SteedType.HorseBurst,            Strings.HorseBurst },
                        { Steed.SteedType.HorseFast,             Strings.HorseFast },
                        { Steed.SteedType.P1Wolf,                Strings.Wolf },
                        { Steed.SteedType.Trap,                  Strings.Trap },
                        { Steed.SteedType.Barrier,               Strings.Barrier },
                        { Steed.SteedType.Bloodstained,          Strings.Bloodstained },
                        { Steed.SteedType.P2Wolf,                Strings.Wolf },
                        { Steed.SteedType.P2Griffin,             Strings.Griffin },
                        { Steed.SteedType.P2Warhorse,            Strings.Warhorse },
                        { Steed.SteedType.P2Stag,                Strings.Stag },
                        { Steed.SteedType.Gullinbursti,          Strings.Gullinbursti },
                        { Steed.SteedType.Sleipnir,              Strings.Sleipnir },
                        { Steed.SteedType.Reindeer_Norselands,   Strings.Reindeer },
                        { Steed.SteedType.CatCart,               Strings.CatCart },
                        { Steed.SteedType.Kelpie,                Strings.Kelpie },
                        { Steed.SteedType.DayNight,              Strings.DayNight },
                        { Steed.SteedType.P2Kelpie,              Strings.Kelpie },
                        { Steed.SteedType.P2Reindeer_Norselands, Strings.Reindeer },
                    };

            foreach (var obj in kingdom.spawnedSteeds)
            {
                if (obj.CurrentMode != Steed.Mode.Player)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Color.blue, steedNames[obj.steedType], obj.price));
            }

            foreach (var obj in kingdom.steedSpawns)
            {
                var info = "";
                foreach (var steedTmp in obj.steedPool)
                {
                    info = steedNames[steedTmp.steedType];
                }

                if (!obj._hasSpawned)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Color.red, info, obj.price));
            }
            
            var cabinList = GameObject.FindObjectsOfType<Cabin>();
            foreach (var obj in cabinList)
            {
                var info = obj.hermitType switch
                {
                    Hermit.HermitType.Baker => Strings.HermitBaker,
                    Hermit.HermitType.Ballista => Strings.HermitBallista,
                    Hermit.HermitType.Horn => Strings.HermitHorn,
                    Hermit.HermitType.Horse => Strings.HermitHorse,
                    Hermit.HermitType.Knight => Strings.HermitKnight,
                    _ => ""
                };

                if (obj.canPay)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Color.red, info, obj.price));
            }

            var statueList = GameObject.FindObjectsOfType<Statue>();
            foreach (var obj in statueList)
            {
                var info = obj.deity switch
                {
                    Statue.Deity.Archer => Strings.StatueArcher,
                    Statue.Deity.Worker => Strings.StatueWorker,
                    Statue.Deity.Knight => Strings.StatueKnight,
                    Statue.Deity.Farmer => Strings.StatueFarmer,
                    Statue.Deity.Time => Strings.StatueTime,
                    _ => ""
                };

                if (obj.deityStatus != Statue.DeityStatus.Activated)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Color.red, info, obj.price));
            }

            var timeStatue = kingdom.timeStatue;
            if (timeStatue)
                poiList.Add(new MarkInfo(timeStatue.transform.position.x, Color.green, Strings.StatueTime, timeStatue.daysRemaining));

            // var wharf = kingdom.wharf;
            var boat = kingdom.boat;
            if (boat)
                poiList.Add(new MarkInfo(boat.transform.position.x, Color.green, Strings.Boat));
            else
            {
                var wreck = kingdom.wreckPlaceholder;
                if (wreck)
                    poiList.Add(new MarkInfo(wreck.transform.position.x, Color.red, Strings.Wreck));
            }

            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;

                if (prefab.prefabID == (int)PrefabIDs.Quarry_undeveloped)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Color.red, Strings.Quarry, obj.price));
                }
                else if (prefab.prefabID == (int)PrefabIDs.Mine_undeveloped)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Color.red, Strings.Mine, obj.price));
                }
                else
                {
                    var unlockNewRulerStatue = go.GetComponent<UnlockNewRulerStatue>();
                    if (unlockNewRulerStatue != null)
                    {
                        var color = unlockNewRulerStatue.status switch
                        {
                            UnlockNewRulerStatue.Status.Locked => Color.red,
                            UnlockNewRulerStatue.Status.WaitingForArcher => Color.blue,
                            _ => Color.green
                        };
                        if (color != Color.green)
                        {
                            var markName = unlockNewRulerStatue.rulerToUnlock switch
                            {
                                Player.Model.None => "",
                                Player.Model.King => Strings.King,
                                Player.Model.Queen => Strings.Queen,
                                Player.Model.Prince => Strings.Prince,
                                Player.Model.Princess => Strings.Princess,
                                Player.Model.Hooded => Strings.Hooded,
                                Player.Model.Zangetsu => Strings.Zangetsu,
                                Player.Model.Alfred => Strings.Alfred,
                                Player.Model.Gebel => Strings.Gebel,
                                Player.Model.Miriam => Strings.Miriam,
                                Player.Model.Total => "",
                                _ => ""
                            };
                            poiList.Add(new MarkInfo(go.transform.position.x, color, markName, obj.price));
                        }
                    }
                }
            }

            foreach (var obj in payables._allBlockers)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;

                if (prefab.prefabID == (int)PrefabIDs.Quarry)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Color.green, Strings.Quarry));
                }
                else if (prefab.prefabID == (int)PrefabIDs.Mine)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Color.green, Strings.Mine));
                }
                else if (prefab.prefabID == (int)PrefabIDs.MerchantHouse)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Color.white, Strings.MerchantSpawner));
                }
                else
                {
                    var thorPuzzleController = go.GetComponent<ThorPuzzleController>();
                    if (thorPuzzleController != null)
                    {
                        var color = thorPuzzleController.State == 0 ? Color.red : Color.green;
                        poiList.Add(new MarkInfo(thorPuzzleController.transform.position.x, color, Strings.ThorPuzzleStatue));
                    }

                    var helPuzzleController = go.GetComponent<HelPuzzleController>();
                    if (helPuzzleController != null)
                    {
                        var color = helPuzzleController.State == 0 ? Color.red : Color.green;
                        poiList.Add(new MarkInfo(helPuzzleController.transform.position.x, color, Strings.HelPuzzleStatue));
                    }
                }
            }

            foreach (var blocker in payables._allBlockers)
            {
                if (blocker == null) continue;
                var scaffolding = blocker.GetComponent<Scaffolding>();
                if (scaffolding == null) continue;
                var go = scaffolding.building;
                if (go == null) continue;

                var wall = go.GetComponent<Wall>();
                if (wall)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Color.blue, Strings.Wall, 0, MarkRow.Sign));
                    if (kingdom.GetBorderSideForPosition(go.transform.position.x) == Side.Left)
                        leftWalls.Add(new WallPoint(go.transform.position, Color.blue));
                    else
                        rightWalls.Add(new WallPoint(go.transform.position, Color.blue));
                }

                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;
                if (prefab.prefabID == (int)PrefabIDs.Quarry)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Color.blue, Strings.Quarry));
                }
                else if (prefab.prefabID == (int)PrefabIDs.Mine)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Color.blue, Strings.Mine));
                }
            }

            // var mine = GameObject.Find("Mine_undeveloped(Clone)");
            // if (mine)
            // {
            //     poiList.Add(new MarkInfo(mine.transform.position, Color.red, Strings.Mine));
            //     log.LogMessage($"mine prefabID: {mine.GetComponent<PrefabID>().prefabID}");
            // }
            
            // explored area

            float wallLeft = Managers.Inst.kingdom.GetBorderSide(Side.Left);
            float wallRight = Managers.Inst.kingdom.GetBorderSide(Side.Right);

            foreach (var poi in poiList)
            {
                if (showFullMap)
                    poi.visible = true;
                else if(poi.worldPosX >= exploredLeft && poi.worldPosX <= exploredRight)
                    poi.visible = true;
                else if (poi.worldPosX >= wallLeft && poi.worldPosX <= wallRight)
                    poi.visible = true;
                else
                    poi.visible = false;
            }

            // Calc screen pos

            if (poiList.Count == 0)
                return;

            var startPos = poiList[0].worldPosX;
            var endPos = poiList[0].worldPosX;

            foreach (var poi in poiList)
            {
                startPos = System.Math.Min(startPos, poi.worldPosX);
                endPos = System.Math.Max(endPos, poi.worldPosX);
            }

            var mapWidth = endPos - startPos;
            var clientWidth = Screen.width - 40;
            var scale = clientWidth / mapWidth;

            foreach (var poi in poiList)
            {
                var poiPosX = (poi.worldPosX - startPos) * scale + 6;
                var poiPosY = poi.row switch
                {
                    MarkRow.Sign => 8,
                    MarkRow.Settled => 24,
                    MarkRow.Numeric => 40,
                    MarkRow.Movable => 54,
                    _ => throw new ArgumentOutOfRangeException()
                };
                poi.pos = new Rect(poiPosX, poiPosY, 120, 20);
            }
            
            minimapMarkList = poiList;

            // Make wall lines

            var lineList = new List<LineInfo>();
            if (leftWalls.Count > 1)
            {
                leftWalls.Sort((a, b) => b.pos.x.CompareTo(a.pos.x));
                var beginPoint = leftWalls[0];
                for (int i = 1; i < leftWalls.Count; i++)
                {
                    var endPoint = leftWalls[i];
                    var info = new LineInfo
                    {
                        lineStart = new Vector2((beginPoint.pos.x - startPos) * scale + 6, 6),
                        lineEnd = new Vector2((endPoint.pos.x - startPos) * scale + 6, 6),
                        color = endPoint.color
                    };
                    lineList.Add(info);
                    beginPoint = endPoint;
                }
            }

            if (rightWalls.Count > 1)
            {
                rightWalls.Sort((a, b) => a.pos.x.CompareTo(b.pos.x));
                var beginPoint = rightWalls[0];
                for (int i = 1; i < rightWalls.Count; i++)
                {
                    var endPoint = rightWalls[i];
                    var info = new LineInfo
                    {
                        lineStart = new Vector2((beginPoint.pos.x - startPos) * scale + 6, 6),
                        lineEnd = new Vector2((endPoint.pos.x - startPos) * scale + 6, 6),
                        color = endPoint.color
                    };
                    lineList.Add(info);
                    beginPoint = endPoint;
                }
            }

            drawLineList = lineList;
        }

        public Payable GetPayableWithPrefabID(PrefabIDs prefabID)
        {
            var payables = Managers.Inst.payables;
            if (!payables) return null;

            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;
                if (prefab.prefabID == (int)prefabID)
                    return obj;
            }

            return null;
        }

        public List<Payable> GetPayablesWithPrefabID(PrefabIDs prefabID)
        {
            var result = new List<Payable>();
            var payables = Managers.Inst.payables;
            if (!payables) return result;

            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;
                if (prefab.prefabID == (int)prefabID)
                {
                    result.Add(obj);
                }
            }
            return result;
        }

        public Payable GetPayableWithComponent<T>()
        {
            var payables = Managers.Inst.payables;
            if (!payables) return null;

            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var comp = go.GetComponent<T>();
                if (comp != null)
                    return obj;
            }

            return null;
        }

        public List<Payable> GetPayablesWithComponent<T>()
        {
            var result = new List<Payable>();
            var payables = Managers.Inst.payables;
            if (!payables) return result;

            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var comp = go.GetComponent<T>();
                if (comp != null)
                    result.Add(obj);
            }

            return result;
        }

        private void DrawMinimapWindow(int winId)
        {
        }

        private void DrawMinimap()
        {
            // Rect windowRect = new Rect(5, 5, Screen.width - 10, 150);
            // GUI.Window(1000, windowRect, (GUI.WindowFunction)DrawMinimapWindow, "");

            Rect boxRect = new Rect(5, 5, Screen.width - 10, 150);
            GUI.Box(boxRect, "");

            foreach (var line in drawLineList)
            {
                GuiHelper.DrawLine(line.lineStart, line.lineEnd, line.color);
            }

            foreach (var markInfo in minimapMarkList)
            {
                if (!markInfo.visible)
                    continue;

                guiStyle.normal.textColor = markInfo.color;
                GUI.Label(markInfo.pos, markInfo.info, guiStyle);
                if (markInfo.count != 0)
                {
                    Rect pos = markInfo.pos;
                    pos.y = pos.y + 16;
                    GUI.Label(pos, markInfo.count.ToString(), guiStyle);
                }

                // draw self vec.x

                // if (markInfo.pos.y == 50.0f)
                // {
                //     Rect pos = markInfo.pos;
                //     pos.y = pos.y + 20;
                //     GUI.Label(pos, markInfo.vec.x.ToString(), SpotMarkGUIStyle);
                // }
            }
        }

        private void UpdateDebugInfo()
        {
            var worldCam = Managers.Inst.game._mainCameraComponent;
            if (!worldCam) return;

            debugInfoList.Clear();
            var objects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in objects)
            {
                var renderer = obj.GetComponent<Renderer>();
                if (!renderer || renderer.isVisible == false) continue;
                if (obj.GetComponent<Bird>()) continue;
                if (obj.GetComponent<Cloud>()) continue;
                if (obj.GetComponent<Grass>()) continue;
                if (obj.GetComponent<Flame>()) continue;
                if (obj.GetComponent<Tile>()) continue;
                if (obj.GetComponent<Foliage>()) continue;
                if (obj.GetComponent<ParticleSystem>()) continue;
                if (obj.GetComponent<BackgroundWall>()) continue;
                if (obj.GetComponent<ReflectedLight>()) continue;
                if (obj.GetComponent<Monument>()) continue;
                if (obj.GetComponent<Fish>()) continue;
                if (obj.GetComponent<Parallax2DChild>()) continue;
                // if (obj.GetComponent<Rigidbody2D>()) continue;
                // if (obj.GetComponent<PolygonCollider2D>()) continue;
                if (obj.GetComponent<WorkableTree>()) continue;

                if (obj.name.StartsWith("Haze")) continue;
                if (obj.name.StartsWith("castle_back")) continue;
                
                if (obj.name == "Glow") continue;
                if (obj.name == "Puff") continue;
                if (obj.name == "trunk") continue;
                if (obj.name == "sparkles") continue;
                if (obj.name == "highlight") continue;
                if (obj.name == "Roots(Clone)") continue;

                Vector3 screenPos = worldCam.WorldToScreenPoint(obj.transform.position);
                Vector3 uiPos = new Vector3(screenPos.x, Screen.height - screenPos.y, 0);
                if (uiPos.x > 0 && uiPos.x < Screen.width && uiPos.y > 0 && uiPos.y < Screen.height)
                {
                    debugInfoList.Add(new DebugInfo(new Rect(uiPos.x, uiPos.y, 100, 100), obj.transform.position, obj.name));
                    //  + " (" + obj.transform.position.ToString() + ")(" + uiPos.ToString() + ")"
                    log.LogInfo(obj.name);
                }
            }
        }

        private void DrawDebugInfo()
        {
            guiStyle.normal.textColor = Color.white;
            foreach (var obj in debugInfoList)
            {
                GUI.Label(obj.pos, obj.info, guiStyle);
                // var vecPos = obj.pos;
                // vecPos.y += 20;
                // GUI.Label(vecPos, obj.vec.x.ToString(), SpotMarkGUIStyle);
            }

            // var dog = GameObject.FindObjectOfType<Dog>();
            // if (!dog)
            //     return;
            //
            // Vector3 dogScreenPos = worldCam.WorldToScreenPoint(dog.transform.position);
            // Vector3 dogUiPos = new Vector3(dogScreenPos.x, Screen.height - dogScreenPos.y, 0);
            // GUI.Label(new Rect(dogUiPos.x, dogUiPos.y, 100, 100), dog.name, SpotMarkGUIStyle);

        }

        private void UpdateStatsInfo()
        {
            var peasantList = GameObject.FindObjectsOfType<Peasant>();
            statsInfo.PeasantCount = peasantList.Count;

            var workerList = GameObject.FindObjectsOfType<Worker>();
            statsInfo.WorkerCount = workerList.Count;

            var archerList = GameObject.FindObjectsOfType<Archer>();
            statsInfo.ArcherCount = archerList.Count;

            var farmerList = GameObject.FindObjectsOfType<Farmer>();
            statsInfo.FarmerCount = farmerList.Count;

            var farmhouseList = GameObject.FindObjectsOfType<Farmhouse>();
            int maxFarmlands = 0;
            foreach (var obj in farmhouseList)
            {
                maxFarmlands += obj.CurrentMaxFarmlands();
            }
            statsInfo.MaxFarmlands = maxFarmlands;
        }

        private int GetKnightCount(bool needsArmor)
        {
            var knightCount = 0;
            foreach (var knight in Managers.Inst.kingdom._knights)
            {
                if (knight._needsArmor == needsArmor)
                    knightCount++;
            }
            return knightCount;
        }

        private void DrawStatsInfo()
        {
            var kingdom = Managers.Inst.kingdom;
            guiStyle.normal.textColor = Color.white;

            Rect boxRect = new Rect(5, 160, 100, 146);
            GUI.Box(boxRect, "");

            GUI.Label(new Rect(14, 166 + 20 * 0, 120, 20), Strings.Peasant + ": " + statsInfo.PeasantCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 1, 120, 20), Strings.Worker + ": " + statsInfo.WorkerCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 2, 120, 20), Strings.Archer + ": " + statsInfo.ArcherCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 3, 120, 20), Strings.Pikeman + ": " + kingdom.Pikemen.Count, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 4, 120, 20), $"{Strings.Knight}: {kingdom.knights.Count} ({GetKnightCount(true)})", guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 5, 120, 20), Strings.Farmer + ": " + statsInfo.FarmerCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 6, 120, 20), Strings.Farmlands + ": " + statsInfo.MaxFarmlands, guiStyle);

#if DEBUG
            var player = Managers.Inst.kingdom.playerOne;
            GUI.Label(new Rect(14, 166 + 20 * 7, 120, 20), "forward" + ": " + player.mover.GetDirection(), guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 8, 120, 20), "steedType" + ": " + kingdom.playerOne.steed.steedType, guiStyle);

            if (kingdom.boat != null)
            {
                // GUI.Label(new Rect(14, 166 + 20 * 9, 120, 20), $"Boat Workers: {kingdom.boat.numWorkers[0]}/{kingdom.boat.maxWorkers}", guiStyle);
                // GUI.Label(new Rect(14, 166 + 20 * 10, 120, 20), $"Boat Archers: {kingdom.boat.numArchers[0]}/{kingdom.boat.numArchers[0]}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 10, 120, 20), $"Boat Farmers: {kingdom.boat.numFarmers}/{kingdom.boat.maxFarmers}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 11, 120, 20), $"Boat Knights: {kingdom.boat.numKnights}/{kingdom.boat.maxKnights}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 12, 120, 20), $"Boat Pikemen: {kingdom.boat.numPikemen}/{kingdom.boat.maxPikemen}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 13, 120, 20), $"Boat Squires: {kingdom.boat.numSquires}/{kingdom.boat.numSquires}", guiStyle);
            }

            var steed = kingdom.playerOne.steed;
            if (steed != null)
            {
                GUI.Label(new Rect(14, 166 + 20 * 14, 120, 20), $"stamina: {steed.stamina}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 15, 120, 20), $"reserveStamina: {steed.reserveStamina}, {steed.reserveProbability}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 16, 120, 20), $"eatDelay: {steed.eatDelay}, {steed.eatFullStaminaDelay}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 17, 120, 20), $"runStaminaRate: {steed.runStaminaRate}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 18, 120, 20), $"standStaminaRate: {steed.standStaminaRate}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 19, 120, 20), $"walkStaminaRate: {steed.walkStaminaRate}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 20, 120, 20), $"tiredTimer: {steed.tiredTimer}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 21, 120, 20), $"tiredDuration: {steed.tiredDuration}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 22, 120, 20), $"wellFedTimer: {steed.wellFedTimer}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 23, 120, 20), $"wellFedDuration: {steed.wellFedDuration}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 24, 120, 20), $"walkSpeed: {steed.walkSpeed}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 25, 120, 20), $"runSpeed: {steed.runSpeed}", guiStyle);
                GUI.Label(new Rect(14, 166 + 20 * 26, 120, 20), $"forestSpeedMultiplier: {steed.forestSpeedMultiplier}", guiStyle);
            }

#endif   
        }

        private void DrawExtraInfo()
        {
            guiStyle.normal.textColor = Color.white;

            var left = Screen.width / 2 - 20;
            var top = 136;

            GUI.Label(new Rect(14, top, 60, 20),  Strings.Land + ": " + (Managers.Inst.game.currentLand + 1), guiStyle);
            GUI.Label(new Rect(14 + 60, top, 60, 20), Strings.Days + ": " + (Managers.Inst.director.CurrentDaysSinceFirstLandingThisReign), guiStyle);

            float currentTime = Managers.Inst.director.currentTime;
            var currentHour = Math.Truncate(currentTime);
            var currentMints = Math.Truncate((currentTime - currentHour) * 60);
            GUI.Label(new Rect(left, top + 22, 40, 20), $"{currentHour:00.}:{currentMints:00.}", guiStyle);

            var player = Managers.Inst.kingdom.playerOne;
            if (player != null)
            {
                GUI.Label(new Rect(Screen.width - 126, top + 22, 60, 20), Strings.Gems + ": " + player.gems, guiStyle);
                GUI.Label(new Rect(Screen.width - 66, top + 22, 60, 20), Strings.Coins + ": " + player.coins, guiStyle);
            }
        }
    }

    public class WallPoint
    {
        public Vector3 pos;
        public Color color;

        public WallPoint(Vector3 pos, Color color)
        {
            this.pos = pos;
            this.color = color;
        }
    }

    public class LineInfo
    {
        public Vector2 lineStart;
        public Vector2 lineEnd;
        public Color color;
    }

    public enum MarkRow
    {
        Sign = -1,
        Settled = 0,
        Numeric = 1,
        Movable = 2
    }

    public class MarkInfo
    {
        public float worldPosX;
        public Rect pos;
        public Color color;
        public string info;
        public MarkRow row;
        public int count;
        public bool visible;

        public MarkInfo(float worldPosX, Rect pos, Color color, string info)
        {
            this.worldPosX = worldPosX;
            this.pos = pos;
            this.color = color;
            this.info = info;
        }

        public MarkInfo(float worldPosX, Color color, string info, int count = 0, MarkRow row = MarkRow.Settled)
        {
            this.worldPosX = worldPosX;
            this.color = color;
            this.info = info;
            this.row = row;
            this.count = count;
        }
    }

    public class StatsInfo
    {
        public int PeasantCount;
        public int WorkerCount;
        public int ArcherCount;
        public int FarmerCount;
        public int MaxFarmlands;
    }

    public class DebugInfo
    {
        public Rect pos;
        public Vector3 vec;
        public string info;

        public DebugInfo(Rect pos, Vector3 vec, string info)
        {
            this.pos = pos;
            this.vec = vec;
            this.info = info;
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

    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

}