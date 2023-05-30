using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

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

                // string myCulture = "zh-CN";
                // Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(myCulture);
                // Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(myCulture);
                // CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(myCulture);
                // CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(myCulture);
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
        private static readonly GUIStyle SpotMarkGUIStyle = new();
        private Player player;
        private Camera worldCam;
        private Camera screenCam;
        private int tick = 0;
        private bool enableDebugInfo = false;
        private bool enableMinimap = true;
        private List<DebugInfo> debugInfoList = new();
        private List<MarkInfo> minimapMarkList = new();
        private StatsInfo statsInfo = new();

        public PluginBase()
        {
            try
            {
                SpotMarkGUIStyle.alignment = TextAnchor.UpperLeft;
                SpotMarkGUIStyle.normal.textColor = Color.white;
                SpotMarkGUIStyle.fontSize = 12;
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

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                enableMinimap = !enableMinimap;
            }

            if (Input.GetKeyDown(KeyCode.End))
            {
                enableDebugInfo = !enableDebugInfo;
            }

            if (Input.GetKeyDown(KeyCode.Insert))
            {
                log.LogMessage("Insert pressed.");

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
                // DebugTools._Inst.SetState(DebugTools.State.Open);
                var debugTools = GameObject.Find("DebugTools");
                if (debugTools)
                {
                    debugTools.SetActive(true);
                    // var component = debugTools.GetComponent<DebugTools>();
                    // if (component)
                    // {
                    //     component.SetState(DebugTools.State.Open);
                    //     Plugin.Logger.LogInfo("DebugTools SetState Open");
                    // }
                }

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

            tick = tick + 1;
            if (tick > 60)
                tick = 0;

            if (tick == 0)
            {
                UpdateCamera();

                UpdateStatsInfo();

                if (enableMinimap)
                    UpdateMinimapMarkList();

                if (enableDebugInfo)
                    UpdateDebugInfo();

            }
        }

        private void OnGUI()
        {
            DrawStatsInfo();

            if (enableMinimap)
                DrawMinimap();

            if (enableDebugInfo)
                DrawDebugInfo();
        }

        private void UpdateCamera()
        {
            if (!player)
                player = GameObject.FindObjectOfType<Player>();
            if (!worldCam)
                worldCam = GameObject.Find("MainCamera")?.GetComponent<Camera>();
            if (!screenCam)
                screenCam = GameObject.Find("Screen Camera")?.GetComponent<Camera>();
        }

        private void UpdateMinimapMarkList()
        {
            if (!worldCam) return;

            minimapMarkList.Clear();
            List<MarkInfo> poiList = new List<MarkInfo>();

            // var cliff = GameObject.Find("Cliff Stones Back(Clone)");
            // poiList.Add(new MarkInfo(cliff.transform.position, Color.white, "悬崖"));
            var dock = GameObject.FindObjectOfType<Beach>();
            if (dock != null)
                poiList.Add(new MarkInfo(dock.transform.position, Color.white, Strings.Dock));

            var portalList = GameObject.FindObjectsOfType<Portal>();
            foreach (var obj in portalList)
            {
                if (obj.type == Portal.Type.Regular)
                    poiList.Add(new MarkInfo(obj.transform.position, Color.white, Strings.Portal));
                else if (obj.type == Portal.Type.Cliff)
                    poiList.Add(new MarkInfo(obj.transform.position, Color.white, Strings.Cliff));
            }

            var beggarCampList = GameObject.FindObjectsOfType<BeggarCamp>();
            foreach (var beggarCamp in beggarCampList)
            {
                int count = 0;
                foreach (var beggar in beggarCamp._beggars)
                {
                    if (beggar != null && beggar.isActiveAndEnabled)
                        count++;
                }
                poiList.Add(new MarkInfo(beggarCamp.transform.position, Color.white, Strings.BeggarCamp, count));
            }

            var riderList = GameObject.FindObjectsOfType<Rider>();
            foreach (var rider in riderList)
            {
                poiList.Add(new MarkInfo(rider.transform.position, Color.green, Strings.You));
            }

            var castleList = GameObject.FindObjectsOfType<Castle>();
            foreach (var obj in castleList)
            {
                poiList.Add(new MarkInfo(obj.transform.position, Color.white, Strings.Castle));
            }
            
            var campfireList = GameObject.FindObjectsOfType<Campfire>();
            foreach (var obj in campfireList)
            {
                poiList.Add(new MarkInfo(obj.transform.position, Color.white, Strings.Campfire));
            }

            var chestList = GameObject.FindObjectsOfType<Chest>();
            foreach (var obj in chestList)
            {
                poiList.Add(new MarkInfo(obj.transform.position, Color.white, obj.isGems ? Strings.GemChest : Strings.Chest, obj.coins));
            }

            var dogSpawn = GameObject.FindObjectOfType<DogSpawn>();
            if (dogSpawn != null && !dogSpawn._dogFreed)
                poiList.Add(new MarkInfo(dogSpawn.transform.position, Color.green, Strings.DogSpawn));

            var steedSpawnList = GameObject.FindObjectsOfType<SteedSpawn>();
            foreach (var obj in steedSpawnList)
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

                Color col = obj.CanPay(player) ? Color.red : Color.green;
                poiList.Add(new MarkInfo(obj.transform.position, col, info));
            }
            
            // GameLayer
            var cabinList = GameObject.FindObjectsOfType<Cabin>();
            foreach (var obj in cabinList)
            {
                var info = "";
                switch (obj.hermitType)
                {
                    case Hermit.HermitType.Baker:
                        info = Strings.HermitBaker;
                        break;
                    case Hermit.HermitType.Ballista:
                        info = Strings.HermitBallista;
                        break;
                    case Hermit.HermitType.Horn:
                        info = Strings.HermitHorn;
                        break;
                    case Hermit.HermitType.Horse:
                        info = Strings.HermitHorse;
                        break;
                    case Hermit.HermitType.Knight:
                        info = Strings.HermitKnight;
                        break;
                }

                Color col = obj.canPay ? Color.red : Color.green;
                poiList.Add(new MarkInfo(obj.transform.position, col, info));
            }

            var statueList = GameObject.FindObjectsOfType<Statue>();
            foreach (var obj in statueList)
            {
                var info = "";
                switch (obj.deity)
                {
                    case Statue.Deity.Archer:
                        info = Strings.StatueArcher;
                        break;
                    case Statue.Deity.Worker:
                        info = Strings.StatueWorker;
                        break;
                    case Statue.Deity.Knight:
                        info = Strings.StatueKnight;
                        break;
                    case Statue.Deity.Farmer:
                        info = Strings.StatueFarmer;
                        break;
                    case Statue.Deity.Time:
                        info = Strings.StatueTime;
                        break;
                }

                Color col = obj.deityStatus == Statue.DeityStatus.Activated ? Color.green : Color.red;
                poiList.Add(new MarkInfo(obj.transform.position, col, info));
            }

            var timeStatue = GameObject.FindObjectOfType<TimeStatue>();
            if (timeStatue)
                poiList.Add(new MarkInfo(timeStatue.transform.position, Color.red, Strings.StatueTime));
            
            // var upgradeList = GameObject.FindObjectsOfType<PayableUpgrade>();
            // foreach (var obj in upgradeList)
            // {
            //     var info = obj.IsLocked(player).ToString();
            //     Color col = obj.CanPay(player) ? Color.red : Color.green;
            //     poiList.Add(new MarkInfo(obj.transform.position, col, info));
            // }

            var wreck = GameObject.FindObjectOfType<WreckPlaceholder>();
            if (wreck)
                poiList.Add(new MarkInfo(wreck.transform.position, Color.red, Strings.Wreck));

            // var riverList = GameObject.FindObjectsOfType<River>();
            // foreach (var obj in riverList)
            // {
            //     poiList.Add(new MarkInfo(obj.transform.position, "河流"));
            // }
            
            var quarry = GameObject.Find("Quarry_undeveloped(Clone)");
            if (quarry)
                poiList.Add(new MarkInfo(quarry.transform.position, Color.red, Strings.Quarry));

            var mine = GameObject.Find("Mine_undeveloped(Clone)");
            if (mine)
                poiList.Add(new MarkInfo(mine.transform.position, Color.red, Strings.Mine));

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
                var poiPosX = (poi.vec.x - startPos) * scale;
                poi.pos = new Rect(poiPosX + 6, 20, 120, 30);
                if (poi.info == Strings.You)
                    poi.pos.y = 50;
            }
            
            minimapMarkList = poiList;
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

            foreach (var markInfo in minimapMarkList)
            {
                SpotMarkGUIStyle.normal.textColor = markInfo.color;
                GUI.Label(markInfo.pos, markInfo.info, SpotMarkGUIStyle);
                if (markInfo.count != 0)
                {
                    Rect pos = markInfo.pos;
                    pos.y = pos.y + 20;
                    GUI.Label(pos, markInfo.count.ToString(), SpotMarkGUIStyle);
                }
            }
        }

        private void UpdateDebugInfo()
        {
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
                    debugInfoList.Add(new DebugInfo(new Rect(uiPos.x, uiPos.y, 100, 100), obj.name));
                    //  + " (" + obj.transform.position.ToString() + ")(" + uiPos.ToString() + ")"
                    log.LogInfo(obj.name);
                }
            }
        }

        private void DrawDebugInfo()
        {
            SpotMarkGUIStyle.normal.textColor = Color.white;
            foreach (var obj in debugInfoList)
            {
                GUI.Label(obj.pos, obj.info, SpotMarkGUIStyle);
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

        private void DrawStatsInfo()
        {
            SpotMarkGUIStyle.normal.textColor = Color.white;

            Rect boxRect = new Rect(5, 160, 150, 120);
            GUI.Box(boxRect, "");

            GUI.Label(new Rect(14, 166 + 20 * 0, 120, 24), Strings.Peasant + ": " + statsInfo.PeasantCount, SpotMarkGUIStyle);
            GUI.Label(new Rect(14, 166 + 20 * 1, 120, 24), Strings.Worker + ": " + statsInfo.WorkerCount, SpotMarkGUIStyle);
            GUI.Label(new Rect(14, 166 + 20 * 2, 120, 24), Strings.Archer + ": " + statsInfo.ArcherCount, SpotMarkGUIStyle);
            GUI.Label(new Rect(14, 166 + 20 * 3, 120, 24), Strings.Farmer + ": " + statsInfo.FarmerCount, SpotMarkGUIStyle);
            GUI.Label(new Rect(14, 166 + 20 * 4, 120, 24), Strings.Farmlands + ": " + statsInfo.MaxFarmlands, SpotMarkGUIStyle);
        }
    }

    public class MarkInfo
    {
        public Vector3 vec;
        public Rect pos;
        public Color color;
        public string info;
        public int count;

        public MarkInfo(Vector3 vec, Rect pos, Color color, string info)
        {
            this.vec = vec;
            this.pos = pos;
            this.color = color;
            this.info = info;
        }

        public MarkInfo(Vector3 vec, Color color, string info, int count = 0)
        {
            this.vec = vec;
            this.color = color;
            this.info = info;
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
        public string info;

        public DebugInfo(Rect pos, string info)
        {
            this.pos = pos;
            this.info = info;
        }
    }
}