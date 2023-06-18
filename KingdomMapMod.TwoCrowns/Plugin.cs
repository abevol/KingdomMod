using System;
using System.Collections.Generic;
using System.Globalization;
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
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                log.LogMessage("Home key pressed.");
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

                var prefabs = GameObject.FindObjectsOfType<PrefabID>();
                foreach (var prefab in prefabs)
                {
                    log.LogMessage($"PrefabID:{prefab.prefabID}, name: {prefab.name}");
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
            log.LogMessage($"GlobalSaveDataFilename: {PlayfabManager.Inst.GlobalSaveDataFilename}");

            exploredLeft = exploredRight = 0;
        }

        public void OnCurrentCampaignSwitch()
        {
            log.LogMessage($"OnCurrentCampaignSwitch: {GlobalSaveData.loaded.currentCampaign}");

        }

        private void UpdateMinimapMarkList()
        {
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
                    poiList.Add(new MarkInfo(obj.transform.position, new Color(0.62f, 0.0f, 1.0f), Strings.Portal));
                else if (obj.type == Portal.Type.Cliff)
                    poiList.Add(new MarkInfo(obj.transform.position, obj.state == Portal.State.Destroyed ? Color.green : Color.red, Strings.Cliff));
                else if (obj.type == Portal.Type.Dock)
                    dock = obj;
            }

            var beach = GameObject.FindObjectOfType<Beach>();
            if (beach != null)
                poiList.Add(new MarkInfo(beach.transform.position, (dock && (dock.state != Portal.State.Destroyed)) ? Color.red : Color.green, Strings.Dock));

            foreach (var beggarCamp in kingdom.BeggarCamps)
            {
                int count = 0;
                foreach (var beggar in beggarCamp._beggars)
                {
                    if (beggar != null && beggar.isActiveAndEnabled)
                        count++;
                }
                poiList.Add(new MarkInfo(beggarCamp.transform.position, Color.white, Strings.BeggarCamp, count));
            }

            foreach (var beggar in kingdom.beggars)
            {
                if (beggar == null) continue;

                if (beggar.hasFoundBaker)
                {
                    poiList.Add(new MarkInfo(beggar.transform.position, Color.red, Strings.Beggar, 0, MarkRow.Movable));
                }
            }

            var mover = kingdom.playerOne.mover;
            if (mover != null)
            {
                poiList.Add(new MarkInfo(mover.transform.position, Color.green, Strings.You, 0, MarkRow.Movable));
                float l = mover.transform.position.x - 12;
                float r = mover.transform.position.x + 12;
                if (l < exploredLeft)
                    exploredLeft = l;
                if (r > exploredRight)
                    exploredRight = r;
            }

            var castle = kingdom.castle;
            if (castle != null)
            {
                poiList.Add(new MarkInfo(castle.transform.position, Color.green, Strings.Castle));
                poiList.Add(new MarkInfo(castle.transform.position, Color.green, Strings.CastleSign, 0, MarkRow.Sign));
                leftWalls.Add(new WallPoint(castle.transform.position, Color.green));
                rightWalls.Add(new WallPoint(castle.transform.position, Color.green));
            }
            
            var campfire = kingdom.campfire;
            if (campfire !=  null)
            {
                poiList.Add(new MarkInfo(campfire.transform.position, Color.white, Strings.Campfire));
            }

            var chestList = GameObject.FindObjectsOfType<Chest>();
            foreach (var obj in chestList)
            {
                poiList.Add(new MarkInfo(obj.transform.position, Color.white, obj.isGems ? Strings.GemChest : Strings.Chest, obj.coins));
            }

            foreach (var obj in kingdom._walls)
            {
                poiList.Add(new MarkInfo(obj.transform.position, Color.green, Strings.Wall, 0, MarkRow.Sign));
                if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(obj.transform.position, Color.green));
                else
                    rightWalls.Add(new WallPoint(obj.transform.position, Color.green));
            }

            var wallWreckList = GameObject.FindGameObjectsWithTag(Tags.WallWreck);
            foreach (var obj in wallWreckList)
            {
                poiList.Add(new MarkInfo(obj.transform.position, Color.red, Strings.WallWreck, 0, MarkRow.Sign));
                if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(obj.transform.position, Color.red));
                else
                    rightWalls.Add(new WallPoint(obj.transform.position, Color.red));
            }

            // var scaffoldingWallList = GameObject.FindGameObjectsWithTag(Tags.ScaffoldingWall);
            // foreach (var obj in scaffoldingWallList)
            // {
            //     poiList.Add(new MarkInfo(obj.transform.position, Color.blue, Strings.Wall, 0, true));
            //     if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
            //         leftWalls.Add(new WallPoint(obj.transform.position, Color.blue));
            //     else
            //         rightWalls.Add(new WallPoint(obj.transform.position, Color.blue));
            // }

            var wallFoundation = GameObject.FindGameObjectsWithTag(Tags.WallFoundation);
            foreach (var obj in wallFoundation)
            {
                poiList.Add(new MarkInfo(obj.transform.position, Color.gray, Strings.WallFoundation, 0, MarkRow.Sign));
            }

            var riverList = GameObject.FindObjectsOfType<River>();
            foreach (var obj in riverList)
            {
                poiList.Add(new MarkInfo(obj.transform.position, new Color(0.46f, 0.84f, 0.92f), Strings.River, 0, MarkRow.Sign));
            }

            foreach (var obj in Managers.Inst.world._berryBushes)
            {
                poiList.Add(new MarkInfo(obj.transform.position, obj.paid ? Color.green : Color.red, Strings.BerryBush, 0, MarkRow.Sign));
            }

            var dogSpawn = GameObject.FindObjectOfType<DogSpawn>();
            if (dogSpawn != null && !dogSpawn._dogFreed)
                poiList.Add(new MarkInfo(dogSpawn.transform.position, Color.red, Strings.DogSpawn));

            // var farmhouseList = GameObject.FindObjectsOfType<Farmhouse>();
            foreach (var obj in kingdom._farmHouses)
            {
                poiList.Add(new MarkInfo(obj.transform.position, Color.green, Strings.Farmhouse));
            }

            // var steedSpawnList = GameObject.FindObjectsOfType<SteedSpawn>();
            foreach (var obj in kingdom.steedSpawns)
            {
                var steedTypeDict = new Dictionary<Steed.SteedType, string>
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
                var info = "";
                foreach (var steed in obj.steedPool)
                {
                    info = steedTypeDict[steed.steedType];
                }

                if (!obj._hasSpawned)
                    poiList.Add(new MarkInfo(obj.transform.position, Color.red, info, obj.price));
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
                    poiList.Add(new MarkInfo(obj.transform.position, Color.red, info, obj.price));
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
                    poiList.Add(new MarkInfo(obj.transform.position, Color.red, info, obj.price));
            }

            var timeStatue = kingdom.timeStatue;
            if (timeStatue)
                poiList.Add(new MarkInfo(timeStatue.transform.position, Color.red, Strings.StatueTime));

            // var wharf = kingdom.wharf;
            var boat = kingdom.boat;
            if (boat)
                poiList.Add(new MarkInfo(boat.transform.position, Color.green, Strings.Boat));
            else
            {
                var wreck = kingdom.wreckPlaceholder;
                if (wreck)
                    poiList.Add(new MarkInfo(wreck.transform.position, Color.red, Strings.Wreck));
            }

            // var quarry = GameObject.Find("Quarry_undeveloped(Clone)");

            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;

                if (prefab.prefabID == PrefabIDs.QuarryUndeveloped)
                {
                    poiList.Add(new MarkInfo(go.transform.position, Color.red, Strings.Quarry, obj.price));
                }
                else if (prefab.prefabID == PrefabIDs.MineUndeveloped)
                {
                    poiList.Add(new MarkInfo(go.transform.position, Color.red, Strings.Mine, obj.price));
                }
            }
            
            foreach (var obj in payables._allBlockers)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;

                if (prefab.prefabID == PrefabIDs.Quarry)
                {
                    poiList.Add(new MarkInfo(go.transform.position, Color.green, Strings.Quarry));
                }
                else if (prefab.prefabID == PrefabIDs.Mine)
                {
                    poiList.Add(new MarkInfo(go.transform.position, Color.green, Strings.Mine));
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
                    poiList.Add(new MarkInfo(go.transform.position, Color.blue, Strings.Wall, 0, MarkRow.Sign));
                    if (kingdom.GetBorderSideForPosition(go.transform.position.x) == Side.Left)
                        leftWalls.Add(new WallPoint(go.transform.position, Color.blue));
                    else
                        rightWalls.Add(new WallPoint(go.transform.position, Color.blue));
                }

                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;
                if (prefab.prefabID == PrefabIDs.Quarry)
                {
                    poiList.Add(new MarkInfo(go.transform.position, Color.blue, Strings.Quarry));
                }
                else if (prefab.prefabID == PrefabIDs.Mine)
                {
                    poiList.Add(new MarkInfo(go.transform.position, Color.blue, Strings.Mine));
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
                else if(poi.vec.x >= exploredLeft && poi.vec.x <= exploredRight)
                    poi.visible = true;
                else if (poi.vec.x >= wallLeft && poi.vec.x <= wallRight)
                    poi.visible = true;
                else
                    poi.visible = false;
            }

            // Calc screen pos

            if (poiList.Count == 0)
                return;

            var startPos = poiList[0].vec.x;
            var endPos = poiList[0].vec.x;

            foreach (var poi in poiList)
            {
                startPos = System.Math.Min(startPos, poi.vec.x);
                endPos = System.Math.Max(endPos, poi.vec.x);
            }

            var mapWidth = endPos - startPos;
            var clientWidth = Screen.width - 40;
            var scale = clientWidth / mapWidth;

            foreach (var poi in poiList)
            {
                var poiPosX = (poi.vec.x - startPos) * scale + 6;
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
            guiStyle.normal.textColor = Color.white;

            Rect boxRect = new Rect(5, 160, 100, 146);
            GUI.Box(boxRect, "");

            GUI.Label(new Rect(14, 166 + 20 * 0, 120, 20), Strings.Peasant + ": " + statsInfo.PeasantCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 1, 120, 20), Strings.Worker + ": " + statsInfo.WorkerCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 2, 120, 20), Strings.Archer + ": " + statsInfo.ArcherCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 3, 120, 20), Strings.Pikeman + ": " + Managers.Inst.kingdom.Pikemen.Count, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 4, 120, 20), $"{Strings.Knight}: {Managers.Inst.kingdom.knights.Count} ({GetKnightCount(true)})", guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 5, 120, 20), Strings.Farmer + ": " + statsInfo.FarmerCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 6, 120, 20), Strings.Farmlands + ": " + statsInfo.MaxFarmlands, guiStyle);
        }

        private void DrawExtraInfo()
        {
            guiStyle.normal.textColor = Color.white;

            var left = Screen.width / 2 - 20;
            var top = 136;

            GUI.Label(new Rect(14, top, 60, 20),  Strings.Land + ": " + (Managers.Inst.game.currentLand + 1), guiStyle);

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
        public Vector3 vec;
        public Rect pos;
        public Color color;
        public string info;
        public MarkRow row;
        public int count;
        public bool visible;

        public MarkInfo(Vector3 vec, Rect pos, Color color, string info)
        {
            this.vec = vec;
            this.pos = pos;
            this.color = color;
            this.info = info;
        }

        public MarkInfo(Vector3 vec, Color color, string info, int count = 0, MarkRow row = MarkRow.Settled)
        {
            this.vec = vec;
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

    public static class PrefabIDs
    {
        public static int QuarryUndeveloped = 21;
        public static int Quarry = 22;
        public static int MineUndeveloped = 49;
        public static int Mine = 50;
    }
}