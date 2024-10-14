using System;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;
using static KingdomMod.OverlayMap.Config;

#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
#else
using System.Collections.Generic;
#endif

namespace KingdomMod
{
    public partial class OverlayMap : MonoBehaviour
    {
        public static OverlayMap Instance { get; private set; }
        private static ManualLogSource log;
        private readonly GUIStyle guiStyle = new();
        private float timeSinceLastGuiUpdate = 0;
        private bool enabledOverlayMap = true;
        private System.Collections.Generic.List<MarkInfo> minimapMarkList = new();
        private System.Collections.Generic.List<LineInfo> drawLineList = new();
        private readonly StatsInfo statsInfo = new();
        private bool showFullMap = false;
        private GameObject gameLayer = null;
        private static int _campaignIndex = 0;
        private static int _land = 0;
        private static int _challengeId = 0;
        private static string _archiveFilename;
        private static readonly ExploredRegion _exploredRegion = new ();
        private static PersephoneCage _persephoneCage;
        private static CachePrefabID _cachePrefabID = new CachePrefabID();

        public static void LogMessage(string message, 
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath]   string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            log.LogMessage($"[{sourceLineNumber}][{memberName}] {message}");
        }

        public static void LogError(string message,
            [System.Runtime.CompilerServices.CallerMemberName]
            string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath]
            string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber]
            int sourceLineNumber = 0)
        {
            log.LogError($"[{sourceLineNumber}][{memberName}] {message}");
        }

        public static void LogWarning(string message,
            [System.Runtime.CompilerServices.CallerMemberName]
            string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath]
            string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber]
            int sourceLineNumber = 0)
        {
            log.LogWarning($"[{sourceLineNumber}][{memberName}] {message}");
        }

        public OverlayMap()
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

        public static void Initialize(OverlayMapPlugin plugin)
        {
            log = plugin.LogSource;
            Global.ConfigBind(plugin.Config);
#if IL2CPP
            ClassInjector.RegisterTypeInIl2Cpp<OverlayMap>();
#endif
            GameObject obj = new(nameof(OverlayMap));
            DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<OverlayMap>();
        }

        private void Start()
        {
            log.LogMessage($"{this.GetType().Name} Start.");
            Patcher.PatchAll(this);
            Game.OnGameStart += (Action)OnGameStart;
            NetworkBigBoss.Instance.OnClientCaughtUp += (Action)this.OnClientCaughtUp;

            // GlobalSaveData.add_OnCurrentCampaignSwitch((Action)OnCurrentCampaignSwitch);

            // log.LogMessage($"resSet test Alfred: {Strings.Alfred}");
            // log.LogMessage($"resSet test Culture: {Strings.Culture?.Name}");
            // var resSet = Strings.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, false, true);
            // if (resSet != null)
            // {
            //     var dict = new SortedDictionary<string, string>();
            //     log.LogMessage($"resSet: ");
            //     var defines = "";
            //     var binds = "";
            //     foreach (DictionaryEntry dictionaryEntry in resSet)
            //     {
            //         dict.Add(dictionaryEntry.Key.ToString() ?? "", dictionaryEntry.Value?.ToString() ?? "");
            //         // defines += $"public static ConfigEntry<string> {dictionaryEntry.Key};\r\n";
            //         // binds += $"{dictionaryEntry.Key} = config.Bind(\"Strings\", \"{dictionaryEntry.Key}\", \"{dictionaryEntry.Value}\", \"\");\r\n";
            //         
            //         // log.LogMessage($"resSet: {dictionaryEntry.Key}, {dictionaryEntry.Value}");
            //     }
            //
            //     foreach (var dictionaryEntry in dict)
            //     {
            //         defines += $"public static ConfigEntry<string> {dictionaryEntry.Key};\r\n";
            //         binds += $"{dictionaryEntry.Key} = config.Bind(\"Strings\", \"{dictionaryEntry.Key}\", \"{dictionaryEntry.Value}\", \"\");\r\n";
            //
            //     }
            //     // log.LogMessage($"defines: {defines}");
            //     // log.LogMessage($"binds: {binds}");
            // }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                log.LogMessage("M key pressed.");
                enabledOverlayMap = !enabledOverlayMap;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                log.LogMessage("F key pressed.");
                showFullMap = !showFullMap;
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

            timeSinceLastGuiUpdate += Time.deltaTime;

            if (timeSinceLastGuiUpdate > (1.0 / Global.GUIUpdatesPerSecond))
            {
                timeSinceLastGuiUpdate = 0;

                if (!IsPlaying()) return;

                if (enabledOverlayMap)
                {
                    UpdateMinimapMarkList();
                    UpdateStatsInfo();
                }
            }
        }

        private static bool IsPlaying()
        {
            var game = Managers.Inst?.game;
            if (game == null) return false;
            return game.state is Game.State.Playing or Game.State.NetworkClientPlaying;
        }

        private void OnGUI()
        {
            if (!IsPlaying()) return;

            if (enabledOverlayMap)
            {
                DrawGuiForPlayer(0);
                DrawGuiForPlayer(1);
            }
        }

        private void DrawGuiForPlayer(int playerId)
        {
            var player = Managers.Inst.kingdom.GetPlayer(playerId);
            if (player == null) return;
            if (player.isActiveAndEnabled == false) return;
            if (player.hasLocalAuthority == false && NetworkBigBoss.IsOnline) return;

            var groupY = 0.0f;
            var groupHeight = Screen.height * 1.0f;

            if (Managers.COOP_ENABLED)
            {
                groupHeight = Screen.height / 2.0f;
                if (playerId == 1)
                    groupY = Screen.height / 2.0f;
            }

            GUI.BeginGroup(new Rect(0, groupY, Screen.width, groupHeight));
            DrawMinimap(playerId);
            DrawStatsInfo(playerId);
            DrawExtraInfo(playerId);
            GUI.EndGroup();
        }

        private void OnClientCaughtUp()
        {
            log.LogMessage("host_OnClientCaughtUp.");

            // OnGameStart();
        }

        public class ExploredRegion
        {
            private float _exploredLeft;
            private float _exploredRight;

            public float ExploredLeft
            {
                get { return _exploredLeft; }
                set
                {
                    _exploredLeft = value;
                    SetExploredLeft(value);
                }
            }

            public float ExploredRight
            {
                get { return _exploredRight; }
                set
                {
                    _exploredRight = value;
                    SetExploredRight(value);
                }
            }

            private static void SetExploredLeft(float value)
            {
                ExploredRegions.ExploredLeft.Value = value;
                ExploredRegions.Time.Value = Managers.Inst.director.currentTime;
                ExploredRegions.Days.Value = Managers.Inst.director.CurrentDayForSpawning;
            }

            private static void SetExploredRight(float value)
            {
                ExploredRegions.ExploredRight.Value = value;
                ExploredRegions.Time.Value = Managers.Inst.director.currentTime;
                ExploredRegions.Days.Value = Managers.Inst.director.CurrentDayForSpawning;
            }

            private static bool HasAvailableConfig()
            {
                if (ExploredRegions.ExploredLeft == 0 && ExploredRegions.ExploredRight == 0)
                    return false;

                if (ExploredRegions.Days > Managers.Inst.director.CurrentDayForSpawning)
                    return false;

                if (ExploredRegions.Days == Managers.Inst.director.CurrentDayForSpawning)
                    if (ExploredRegions.Time > Managers.Inst.director.currentTime)
                        return false;

                return true;
            }

            public void Init()
            {
                ExploredRegions.ConfigBind(_archiveFilename);
                if (HasAvailableConfig())
                {
                    _exploredLeft = ExploredRegions.ExploredLeft;
                    _exploredRight = ExploredRegions.ExploredRight;
                }
                else
                {
                    var player = GetLocalPlayer();
                    _exploredLeft = player.transform.localPosition.x;
                    _exploredRight = player.transform.localPosition.x;
                }
            }
        }

        private void OnGameStart()
        {
            log.LogMessage("OnGameStart.");

            gameLayer = GameObject.FindGameObjectWithTag(Tags.GameLayer);
            _persephoneCage = UnityEngine.Object.FindAnyObjectByType<PersephoneCage>();
            _cachePrefabID.CachePrefabIDs();

            _campaignIndex = GlobalSaveData.loaded.currentCampaign;
            _land = CampaignSaveData.current.CurrentLand;
            _challengeId = GlobalSaveData.loaded.currentChallenge;
            _archiveFilename = IslandSaveData.GetFilePropsForLand(_campaignIndex, _land, _challengeId).filename;

            log.LogMessage($"OnGameStart: _archiveFilename {_archiveFilename}, Campaign {_campaignIndex}, CurrentLand {_land}, currentChallenge {_challengeId}");

            _exploredRegion.Init();
        }

        private void OnCurrentCampaignSwitch()
        {
            log.LogMessage($"OnCurrentCampaignSwitch: {GlobalSaveData.loaded.currentCampaign}");

        }

        private static Player GetLocalPlayer()
        {
            return Managers.Inst.kingdom.GetPlayer(NetworkBigBoss.HasWorldAuth ? 0 : 1);
        }

        private void UpdateMinimapMarkList()
        {
            var world = Managers.Inst.world;
            if (world == null) return;
            var level = Managers.Inst.level;
            if (level == null) return;
            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return;
            var payables = Managers.Inst.payables;
            if (payables == null) return;

            minimapMarkList.Clear();
            var poiList = new System.Collections.Generic.List<MarkInfo>();
            var leftWalls = new System.Collections.Generic.List<WallPoint>();
            var rightWalls = new System.Collections.Generic.List<WallPoint>();

            Portal dock = null;
            foreach (var obj in kingdom.AllPortals)
            {
                if (obj.type == Portal.Type.Regular)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.Portal.Color, Style.Portal.Sign, Strings.Portal));
                else if (obj.type == Portal.Type.Cliff)
                    poiList.Add(new MarkInfo(obj.transform.position.x, obj.state switch{ Portal.State.Destroyed => Style.Cliff.Destroyed.Color, Portal.State.Rebuilding => Style.Cliff.Rebuilding.Color, _=> Style.Cliff.Color }, Style.Cliff.Sign, Strings.Cliff));
                else if (obj.type == Portal.Type.Dock)
                    dock = obj;
            }

            var beach = gameLayer.GetComponentInChildren<Beach>();
            if (beach != null)
                poiList.Add(new MarkInfo(beach.transform.position.x, (dock && (dock.state != Portal.State.Destroyed)) ? Style.Beach.Color : Style.Beach.Destroyed.Color, Style.Beach.Sign, Strings.Beach));

            foreach (var beggarCamp in kingdom.BeggarCamps)
            {
                int count = 0;
                foreach (var beggar in beggarCamp._beggars)
                {
                    if (beggar != null && beggar.isActiveAndEnabled)
                        count++;
                }
                poiList.Add(new MarkInfo(beggarCamp.transform.position.x, Style.BeggarCamp.Color, Style.BeggarCamp.Sign, Strings.BeggarCamp, count));
            }

            foreach (var beggar in kingdom.Beggars)
            {
                if (beggar == null) continue;

                if (beggar.hasFoundBaker)
                {
                    poiList.Add(new MarkInfo(beggar.transform.position.x, Style.Beggar.Color, Style.Beggar.Sign, Strings.Beggar, 0, MarkRow.Movable));
                }
            }

            foreach (var player in new System.Collections.Generic.List<Player>{ kingdom.playerOne, kingdom.playerTwo })
            {
                if (player == null) continue;
                if (player.isActiveAndEnabled == false) continue;
                var mover = player.mover;
                if (mover == null) continue;

                poiList.Add(new MarkInfo(mover.transform.position.x, Style.Player.Color, Style.Player.Sign, player.playerId == 0 ? Strings.P1 : Strings.P2, 0, MarkRow.Movable));
                float l = mover.transform.position.x - 12;
                float r = mover.transform.position.x + 12;
                if (l < _exploredRegion.ExploredLeft)
                    _exploredRegion.ExploredLeft = l;
                if (r > _exploredRegion.ExploredRight)
                    _exploredRegion.ExploredRight = r;
            }

            if (kingdom.teleExitP1)
                poiList.Add(new MarkInfo(kingdom.teleExitP1.transform.position.x, Style.TeleExitP1.Color, Style.TeleExitP1.Sign, Strings.TeleExitP1, 0, MarkRow.Movable));

            if (kingdom.teleExitP2)
                poiList.Add(new MarkInfo(kingdom.teleExitP2.transform.position.x, Style.TeleExitP2.Color, Style.TeleExitP2.Sign, Strings.TeleExitP2, 0, MarkRow.Movable));

            var deers = GameExtensions.FindObjectsWithTagOfType<Deer>(Tags.Wildlife);
            foreach (var deer in deers)
            {
                if (!deer._damageable.isDead)
                    poiList.Add(new MarkInfo(deer.transform.position.x, deer._fsm.Current == 5 ? Style.DeerFollowing.Color : Style.Deer.Color, Style.Deer.Sign, Strings.Deer, 0, MarkRow.Movable));
            }

            var enemies = Managers.Inst.enemies._enemies;
            if (enemies != null && enemies.Count > 0)
            {
                var leftEnemies = new System.Collections.Generic.List<float>();
                var rightEnemies = new System.Collections.Generic.List<float>();
                var leftBosses = new System.Collections.Generic.List<float>();
                var rightBosses = new System.Collections.Generic.List<float>();
                foreach (var enemy in enemies)
                {
                    if (enemy == null) continue;
                    var damageable = enemy.GetComponent<Damageable>();
                    if (damageable != null && damageable.isDead)
                        continue;

                    var enemyX = enemy.transform.position.x;
                    if (kingdom.GetBorderSideForPosition(enemyX) == Side.Left)
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
                        poiList.Add(new MarkInfo(leftBosses[0], Style.Boss.Color, Style.Boss.Sign, Strings.Boss, leftBosses.Count, MarkRow.Movable));
                        if (leftEnemies[0] - leftBosses[0] < 6)
                            drawEnemies = false;
                    }

                    if (drawEnemies)
                        poiList.Add(new MarkInfo(leftEnemies[0], Style.Enemy.Color, Style.Enemy.Sign, Strings.Enemy, leftEnemies.Count, MarkRow.Movable));
                }

                if (rightEnemies.Count > 0)
                {
                    var drawEnemies = true;
                    rightEnemies.Sort((a, b) => a.CompareTo(b));

                    if (rightBosses.Count > 0)
                    {
                        rightBosses.Sort((a, b) => a.CompareTo(b));
                        poiList.Add(new MarkInfo(rightBosses[0], Style.Boss.Color, Style.Boss.Sign, Strings.Boss, rightBosses.Count, MarkRow.Movable));
                        if (rightBosses[0] - rightEnemies[0] < 6)
                            drawEnemies = false;
                    }

                    if (drawEnemies)
                        poiList.Add(new MarkInfo(rightEnemies[0], Style.Enemy.Color, Style.Enemy.Sign, Strings.Enemy, rightEnemies.Count, MarkRow.Movable));
                }
            }

            var castle = kingdom.castle;
            if (castle != null)
            {
                var payable = castle._payableUpgrade;
                bool canPay = payable.IsLocked(GetLocalPlayer(), out var reason);
                bool isLocked = reason != LockIndicator.LockReason.NotLocked && reason != LockIndicator.LockReason.NoUpgrade;
                bool isLockedForInvalidTime = reason == LockIndicator.LockReason.InvalidTime;
                var price = isLockedForInvalidTime ? (int)(payable.timeAvailableFrom - Time.time) : canPay ? payable.Price : 0;
                var color = isLocked ? Style.Castle.Locked.Color : Style.Castle.Color;
                poiList.Add(new MarkInfo(castle.transform.position.x, color, Style.Castle.Sign, Strings.Castle, price));

                leftWalls.Add(new WallPoint(castle.transform.position, Style.WallLine.Color));
                rightWalls.Add(new WallPoint(castle.transform.position, Style.WallLine.Color));
            }

            var campfire = kingdom.campfire;
            if (campfire !=  null)
            {
                poiList.Add(new MarkInfo(campfire.transform.position.x, Style.Campfire.Color, Style.Campfire.Sign, Strings.Campfire));
            }

            var chestList = gameLayer.GetComponentsInChildren<Chest>();
            foreach (var obj in chestList)
            {
                if (obj.currencyAmount == 0) continue;

                if (obj.currencyType == CurrencyType.Gems)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.GemChest.Color, Style.GemChest.Sign, Strings.GemChest, obj.currencyAmount));
                else
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.Chest.Color, Style.Chest.Sign, Strings.Chest, obj.currencyAmount));
            }

            foreach (var obj in kingdom._walls)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Style.Wall.Color, Style.Wall.Sign, ""));
                if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(obj.transform.position, Style.WallLine.Color));
                else
                    rightWalls.Add(new WallPoint(obj.transform.position, Style.WallLine.Color));
            }

            var shopForge = GameObject.FindGameObjectWithTag(Tags.ShopForge);
            if (shopForge != null)
            {
                poiList.Add(new MarkInfo(shopForge.transform.position.x, Style.ShopForge.Color, Style.ShopForge.Sign, Strings.ShopForge));
            }

            var citizenHouses = GameObject.FindGameObjectsWithTag(Tags.CitizenHouse);
            foreach (var obj in citizenHouses)
            {
                var citizenHouse = obj.GetComponent<CitizenHousePayable>();
                if (citizenHouse != null)
                {
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.CitizenHouse.Color, Style.CitizenHouse.Sign, Strings.CitizenHouse, citizenHouse._numberOfAvailableCitizens));
                }
            }

            var wallWreckList = GameObject.FindGameObjectsWithTag(Tags.WallWreck);
            foreach (var obj in wallWreckList)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Style.Wall.Wrecked.Color, Style.Wall.Sign, ""));
                if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(obj.transform.position, Style.WallLine.Wrecked.Color));
                else
                    rightWalls.Add(new WallPoint(obj.transform.position, Style.WallLine.Wrecked.Color));
            }

            var wallFoundation = GameObject.FindGameObjectsWithTag(Tags.WallFoundation);
            foreach (var obj in wallFoundation)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Style.WallFoundation.Color, Style.WallFoundation.Sign, ""));
            }

            var riverList = gameLayer.GetComponentsInChildren<River>();
            foreach (var obj in riverList)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Style.River.Color, Style.River.Sign, ""));
            }

            foreach (var obj in Managers.Inst.world._berryBushes)
            {
                if (obj.paid)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.BerryBushPaid.Color, Style.BerryBushPaid.Sign, ""));
                else
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.BerryBush.Color, Style.BerryBush.Sign, ""));
            }

            var payableGemChest = GameExtensions.GetPayableOfType<PayableGemChest>();
            if (payableGemChest != null)
            {
                var gemsCount = payableGemChest.infiniteGems ? payableGemChest.guardRef.Price : payableGemChest.gemsStored;
                poiList.Add(new MarkInfo(payableGemChest.transform.position.x, Style.GemMerchant.Color, Style.GemMerchant.Sign, Strings.GemMerchant, gemsCount));
            }

            var dogSpawn = GameExtensions.GetPayableBlockerOfType<DogSpawn>();
            if (dogSpawn != null && !dogSpawn._dogFreed)
                poiList.Add(new MarkInfo(dogSpawn.transform.position.x, Style.DogSpawn.Color, Style.DogSpawn.Sign, Strings.DogSpawn));

            var boarSpawn = world.boarSpawnGroup;
            if (boarSpawn != null)
            {
                poiList.Add(new MarkInfo(boarSpawn.transform.position.x, Style.BoarSpawn.Color, Style.BoarSpawn.Sign,
                    Strings.BoarSpawn, boarSpawn._spawnedBoar ? 0 : 1));
            }

            var caveHelper = Managers.Inst.caveHelper;
            if (caveHelper != null && caveHelper.CurrentlyBombingPortal != null)
            {
                var bomb = caveHelper.Getbomb(caveHelper.CurrentlyBombingPortal.side);
                if (bomb != null)
                {
                    poiList.Add(new MarkInfo(bomb.transform.position.x, Style.Bomb.Color, Style.Bomb.Sign, Strings.Bomb, 0, MarkRow.Movable));
                }
            }

            foreach (var obj in kingdom.GetFarmHouses())
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Style.Farmhouse.Color, Style.Farmhouse.Sign, Strings.Farmhouse));
            }

            var steedNames = new System.Collections.Generic.Dictionary<SteedType, string>
                    {
                        { SteedType.INVALID,               Strings.Invalid },
                        { SteedType.Bear,                  Strings.Bear },
                        { SteedType.P1Griffin,             Strings.Griffin },
                        { SteedType.Lizard,                Strings.Lizard },
                        { SteedType.Reindeer,              Strings.Reindeer },
                        { SteedType.Spookyhorse,           Strings.Spookyhorse },
                        { SteedType.Stag,                  Strings.Stag },
                        { SteedType.Unicorn,               Strings.Unicorn },
                        { SteedType.P1Warhorse,            Strings.Warhorse },
                        { SteedType.P1Default,             Strings.DefaultSteed },
                        { SteedType.P2Default,             Strings.DefaultSteed },
                        { SteedType.HorseStamina,          Strings.HorseStamina },
                        { SteedType.HorseBurst,            Strings.HorseBurst },
                        { SteedType.HorseFast,             Strings.HorseFast },
                        { SteedType.P1Wolf,                Strings.Wolf },
                        { SteedType.Trap,                  Strings.Trap },
                        { SteedType.Barrier,               Strings.Barrier },
                        { SteedType.Bloodstained,          Strings.Bloodstained },
                        { SteedType.P2Wolf,                Strings.Wolf },
                        { SteedType.P2Griffin,             Strings.Griffin },
                        { SteedType.P2Warhorse,            Strings.Warhorse },
                        { SteedType.P2Stag,                Strings.Stag },
                        { SteedType.Gullinbursti,          Strings.Gullinbursti },
                        { SteedType.Sleipnir,              Strings.Sleipnir },
                        { SteedType.Reindeer_Norselands,   Strings.Reindeer },
                        { SteedType.CatCart,               Strings.CatCart },
                        { SteedType.Kelpie,                Strings.Kelpie },
                        { SteedType.DayNight,              Strings.DayNight },
                        { SteedType.P2Kelpie,              Strings.Kelpie },
                        { SteedType.P2Reindeer_Norselands, Strings.Reindeer },
                        { SteedType.Hippocampus,           Strings.Hippocampus },
                        { SteedType.Cerberus,              Strings.Cerberus },
                        { SteedType.Spider,                Strings.Spider },
                        { SteedType.TheChariotDay,         Strings.TheChariotDay },
                        { SteedType.TheChariotNight,       Strings.TheChariotNight },
                        { SteedType.Pegasus,               Strings.Pegasus },
                        { SteedType.Donkey,                Strings.Donkey },
                        { SteedType.MolossianHound,        Strings.MolossianHound },
                        { SteedType.Chimera,               Strings.Chimera },
                        { SteedType.Total,                 Strings.Total }
                    };

            foreach (var obj in kingdom.spawnedSteeds)
            {
                if (obj.CurrentMode != SteedMode.Player)
                {
                    if (!steedNames.TryGetValue(obj.steedType, out var steedName))
                        steedName = obj.steedType.ToString();
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.Steeds.Color, Style.Steeds.Sign, steedName, obj.Price));
                }
            }

            foreach (var obj in kingdom.steedSpawns)
            {
                var info = "";
                foreach (var steedTmp in obj.steedPool)
                {
                    if (!steedNames.TryGetValue(steedTmp.steedType, out var steedName))
                        continue;
                    info = steedName;
                }

                if (!obj._hasSpawned)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.SteedSpawns.Color, Style.SteedSpawns.Sign, info, obj.Price));
            }

            string LogUnknownHermitType(Hermit.HermitType hermitType)
            {
                log.LogWarning($"Unknown hermit type: {hermitType}");
                return hermitType.ToString();
            }

            var cabinList = GameExtensions.GetPayablesOfType<Cabin>();
            foreach (var obj in cabinList)
            {
                var info = obj.hermitType switch
                {
                    Hermit.HermitType.Baker => Strings.HermitBaker,
                    Hermit.HermitType.Ballista => Strings.HermitBallista,
                    Hermit.HermitType.Horn => Strings.HermitHorn,
                    Hermit.HermitType.Horse => Strings.HermitHorse,
                    Hermit.HermitType.Knight => Strings.HermitKnight,
                    Hermit.HermitType.Persephone => Strings.HermitPersephone,
                    Hermit.HermitType.Fire => Strings.HermitFire,
                    _ => LogUnknownHermitType(obj.hermitType)
                };

                var color = obj.canPay ? Style.HermitCabins.Locked.Color : Style.HermitCabins.Unlocked.Color;
                var price = obj.canPay ? obj.Price : 0;
                poiList.Add(new MarkInfo(obj.transform.position.x, color, Style.HermitCabins.Sign, info, price));
            }

            if (_persephoneCage)
            {
                var color = PersephoneCage.State.IsPersephoneLocked(_persephoneCage._fsm.Current) ? Style.PersephoneCage.Locked.Color : Style.PersephoneCage.Unlocked.Color;
                poiList.Add(new MarkInfo(_persephoneCage.transform.position.x, color, Style.PersephoneCage.Sign, Strings.HermitPersephone, 0));
            }

            var statueList = GameExtensions.GetPayablesOfType<Statue>();
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

                bool isLocked = obj.deityStatus != Statue.DeityStatus.Activated;
                var color = isLocked ? Style.Statues.Locked.Color : Style.Statues.Unlocked.Color;
                var price = isLocked ? obj.Price : 0;
                poiList.Add(new MarkInfo(obj.transform.position.x, color, Style.Statues.Sign, info, price));
            }

            var timeStatue = kingdom.timeStatue;
            if (timeStatue)
                poiList.Add(new MarkInfo(timeStatue.transform.position.x, Style.StatueTime.Color, Style.StatueTime.Sign, Strings.StatueTime, timeStatue.daysRemaining));

            // var wharf = kingdom.wharf;
            var boat = kingdom.boat;
            if (boat)
                poiList.Add(new MarkInfo(boat.transform.position.x, Style.Boat.Color, Style.Boat.Sign, Strings.Boat));
            else
            {
                var wreck = kingdom.wreckPlaceholder;
                if (wreck)
                    poiList.Add(new MarkInfo(wreck.transform.position.x, Style.Boat.Wrecked.Color, Style.Boat.Sign, Strings.BoatWreck));
            }

            var summonBell = kingdom.boatSailPosition?.GetComponentInChildren<BoatSummoningBell>();
            if (summonBell)
                poiList.Add(new MarkInfo(summonBell.transform.position.x, Style.SummonBell.Color, Style.SummonBell.Sign, Strings.SummonBell));

            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;

                if (prefab.prefabID == (int)GamePrefabID.Quarry_undeveloped)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Quarry.Locked.Color, Style.Quarry.Sign, Strings.Quarry, obj.Price));
                }
                else if (prefab.prefabID == (int)GamePrefabID.Mine_undeveloped)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Mine.Locked.Color, Style.Mine.Sign, Strings.Mine, obj.Price));
                }
                else
                {
                    var unlockNewRulerStatue = go.GetComponent<UnlockNewRulerStatue>();
                    if (unlockNewRulerStatue != null)
                    {
                        var color = unlockNewRulerStatue.status switch
                        {
                            UnlockNewRulerStatue.Status.Locked => Style.RulerSpawns.Locked.Color,
                            UnlockNewRulerStatue.Status.WaitingForArcher => Style.RulerSpawns.Building.Color,
                            _ => Style.RulerSpawns.Unlocked.Color
                        };
                        if (color != Style.RulerSpawns.Unlocked.Color)
                        {
                            var markName = unlockNewRulerStatue.rulerToUnlock switch
                            {
                                MonarchType.King => Strings.King,
                                MonarchType.Queen => Strings.Queen,
                                MonarchType.Prince => Strings.Prince,
                                MonarchType.Princess => Strings.Princess,
                                MonarchType.Hooded => Strings.Hooded,
                                MonarchType.Zangetsu => Strings.Zangetsu,
                                MonarchType.Alfred => Strings.Alfred,
                                MonarchType.Gebel => Strings.Gebel,
                                MonarchType.Miriam => Strings.Miriam,
                                MonarchType.Total => "",
                                _ => ""
                            };
                            poiList.Add(new MarkInfo(go.transform.position.x, color, Style.RulerSpawns.Sign, markName, obj.Price));
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

                if (prefab.prefabID == (int)GamePrefabID.Quarry)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Quarry.Unlocked.Color, Style.Quarry.Sign, Strings.Quarry));
                }
                else if (prefab.prefabID == (int)GamePrefabID.Mine)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Mine.Unlocked.Color, Style.Mine.Sign, Strings.Mine));
                }
                else if (prefab.prefabID == (int)GamePrefabID.MerchantHouse)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.MerchantHouse.Color, Style.MerchantHouse.Sign, Strings.MerchantHouse));
                }
                else
                {
                    var thorPuzzleController = go.GetComponent<ThorPuzzleController>();
                    if (thorPuzzleController != null)
                    {
                        var color = thorPuzzleController.State == 0 ? Style.ThorPuzzleStatue.Locked.Color : Style.ThorPuzzleStatue.Unlocked.Color;
                        poiList.Add(new MarkInfo(thorPuzzleController.transform.position.x, color, Style.ThorPuzzleStatue.Sign, Strings.ThorPuzzleStatue));
                    }

                    var helPuzzleController = go.GetComponent<HelPuzzleController>();
                    if (helPuzzleController != null)
                    {
                        var color = helPuzzleController.State == 0 ? Style.HelPuzzleStatue.Locked.Color : Style.HelPuzzleStatue.Unlocked.Color;
                        poiList.Add(new MarkInfo(helPuzzleController.transform.position.x, color, Style.HelPuzzleStatue.Sign, Strings.HelPuzzleStatue));
                    }
                }
            }

            foreach (var blocker in payables._allBlockers)
            {
                if (blocker == null) continue;
                var scaffolding = blocker.GetComponent<Scaffolding>();
                if (scaffolding == null) continue;
                var go = scaffolding.Building;
                if (go == null) continue;

                var wall = go.GetComponent<Wall>();
                if (wall)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Wall.Building.Color, Style.Wall.Sign, ""));
                    if (kingdom.GetBorderSideForPosition(go.transform.position.x) == Side.Left)
                        leftWalls.Add(new WallPoint(go.transform.position, Style.WallLine.Building.Color));
                    else
                        rightWalls.Add(new WallPoint(go.transform.position, Style.WallLine.Building.Color));
                }

                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;
                if (prefab.prefabID == (int)GamePrefabID.Quarry)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Quarry.Building.Color, Style.Quarry.Sign, Strings.Quarry));
                }
                else if (prefab.prefabID == (int)GamePrefabID.Mine)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Mine.Building.Color, Style.Mine.Sign, Strings.Mine));
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
                    poi.Visible = true;
                else if(poi.WorldPosX >= _exploredRegion.ExploredLeft && poi.WorldPosX <= _exploredRegion.ExploredRight)
                    poi.Visible = true;
                else if (poi.WorldPosX >= wallLeft && poi.WorldPosX <= wallRight)
                    poi.Visible = true;
                else
                    poi.Visible = false;
            }

            // Calc screen pos

            if (poiList.Count == 0)
                return;

            var startPos = poiList[0].WorldPosX;
            var endPos = poiList[0].WorldPosX;

            foreach (var poi in poiList)
            {
                startPos = System.Math.Min(startPos, poi.WorldPosX);
                endPos = System.Math.Max(endPos, poi.WorldPosX);
            }

            var mapWidth = endPos - startPos;
            var clientWidth = Screen.width - 40;
            var scale = clientWidth / mapWidth;

            foreach (var poi in poiList)
            {
                poi.Pos = (poi.WorldPosX - startPos) * scale + 16;
            }
            
            minimapMarkList = poiList;

            // Make wall lines

            var lineList = new System.Collections.Generic.List<LineInfo>();
            if (leftWalls.Count > 1)
            {
                leftWalls.Sort((a, b) => b.Pos.x.CompareTo(a.Pos.x));
                var beginPoint = leftWalls[0];
                for (int i = 1; i < leftWalls.Count; i++)
                {
                    var endPoint = leftWalls[i];
                    var info = new LineInfo
                    {
                        LineStart = new Vector2((beginPoint.Pos.x - startPos) * scale + 16, 6),
                        LineEnd = new Vector2((endPoint.Pos.x - startPos) * scale + 16, 6),
                        Color = endPoint.Color
                    };
                    lineList.Add(info);
                    beginPoint = endPoint;
                }
            }

            if (rightWalls.Count > 1)
            {
                rightWalls.Sort((a, b) => a.Pos.x.CompareTo(b.Pos.x));
                var beginPoint = rightWalls[0];
                for (int i = 1; i < rightWalls.Count; i++)
                {
                    var endPoint = rightWalls[i];
                    var info = new LineInfo
                    {
                        LineStart = new Vector2((beginPoint.Pos.x - startPos) * scale + 16, 6),
                        LineEnd = new Vector2((endPoint.Pos.x - startPos) * scale + 16, 6),
                        Color = endPoint.Color
                    };
                    lineList.Add(info);
                    beginPoint = endPoint;
                }
            }

            drawLineList = lineList;
        }

        private static bool IsYourSelf(int playerId, string name)
        {
            if (name == Strings.P1)
            {
                if (playerId == 0 && NetworkBigBoss.HasWorldAuth)
                {
                    return true;
                }
            }
            else if (name == Strings.P2)
            {
                if (playerId == 1 && (Managers.COOP_ENABLED || ProgramDirector.IsClient))
                {
                    return true;
                }
            }
            return false;
        }

        private void DrawMinimap(int playerId)
        {
            float boxHeight = 150;
            if (Managers.COOP_ENABLED)
                boxHeight = 150 - 56;
            Rect boxRect = new Rect(5, 5, Screen.width - 10, boxHeight);
            GUI.Box(boxRect, "");
            GUI.Box(boxRect, "");

            foreach (var line in drawLineList)
            {
                GuiHelper.DrawLine(line.LineStart, line.LineEnd, line.Color);
            }

            foreach (var markInfo in minimapMarkList)
            {
                if (!markInfo.Visible)
                    continue;

                var markName = markInfo.Name;
                var color = markInfo.Color;
                if (markInfo.NameRow == MarkRow.Movable)
                {
                    if (IsYourSelf(playerId, markName))
                    {
                        markName = Strings.You.Value;
                        color = Style.PlayerSelf.Color;
                    }
                }

                guiStyle.alignment = TextAnchor.UpperCenter;
                guiStyle.normal.textColor = color;

                if (markInfo.Sign != "")
                    GUI.Label(new Rect(markInfo.Pos, 8, 0, 20), markInfo.Sign, guiStyle);

                float namePosY = markInfo.NameRow switch
                {
                    MarkRow.Settled => 24,
                    MarkRow.Movable => 56,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (markInfo.Name != "")
                    GUI.Label(new Rect(markInfo.Pos, namePosY, 0, 20), markName, guiStyle);

                if (markInfo.Count != 0)
                    GUI.Label(new Rect(markInfo.Pos, namePosY + 16, 0, 20), markInfo.Count.ToString(), guiStyle);

                // draw self vec.x

                // if (markInfo.pos.y == 50.0f)
                // {
                //     Rect pos = markInfo.pos;
                //     pos.y = pos.y + 20;
                //     GUI.Label(pos, markInfo.vec.x.ToString(), SpotMarkGUIStyle);
                // }
            }
        }

        private void UpdateStatsInfo()
        {
            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return;

            var peasantList = GameObject.FindGameObjectsWithTag(Tags.Peasant);
            statsInfo.PeasantCount = peasantList.Length;

            var workerList = kingdom._workers;
            statsInfo.WorkerCount = workerList.Count;

            var archerList = kingdom._archers;
            statsInfo.ArcherCount = archerList.Count;

            var farmerList = kingdom.Farmers;
            statsInfo.FarmerCount = farmerList.Count;

            var farmhouseList = kingdom.GetFarmHouses();
            int maxFarmlands = 0;
            foreach (var obj in farmhouseList)
            {
                maxFarmlands += obj.CurrentMaxFarmlands();
            }
            statsInfo.MaxFarmlands = maxFarmlands;
        }

        private void DrawStatsInfo(int playerId)
        {
            guiStyle.normal.textColor = Style.StatsInfo.Color;
            guiStyle.alignment = TextAnchor.UpperLeft;

            float boxTop = 160;
            if (Managers.COOP_ENABLED)
                boxTop = 160 - 56;

            var kingdom = Managers.Inst.kingdom;
            var boxRect = new Rect(5, boxTop, 120, 146);
            GUI.Box(boxRect, "");
            GUI.Box(boxRect, "");

            GUI.Label(new Rect(14, boxTop + 6 + 20 * 0, 120, 20), Strings.Peasant + ": " + statsInfo.PeasantCount, guiStyle);
            GUI.Label(new Rect(14, boxTop + 6 + 20 * 1, 120, 20), Strings.Worker + ": " + statsInfo.WorkerCount, guiStyle);
            GUI.Label(new Rect(14, boxTop + 6 + 20 * 2, 120, 20), $"{Strings.Archer.Value}: {statsInfo.ArcherCount} ({GameExtensions.GetArcherCount(GameExtensions.ArcherType.Free)}|{GameExtensions.GetArcherCount(GameExtensions.ArcherType.GuardSlot)}|{GameExtensions.GetArcherCount(GameExtensions.ArcherType.KnightSoldier)})", guiStyle);
            GUI.Label(new Rect(14, boxTop + 6 + 20 * 3, 120, 20), Strings.Pikeman + ": " + kingdom.Pikemen.Count, guiStyle);
            GUI.Label(new Rect(14, boxTop + 6 + 20 * 4, 120, 20), $"{Strings.Knight.Value}: {kingdom.Knights.Count} ({GameExtensions.GetKnightCount(true)})", guiStyle);
            GUI.Label(new Rect(14, boxTop + 6 + 20 * 5, 120, 20), Strings.Farmer + ": " + statsInfo.FarmerCount, guiStyle);
            GUI.Label(new Rect(14, boxTop + 6 + 20 * 6, 120, 20), Strings.Farmlands + ": " + statsInfo.MaxFarmlands, guiStyle);
        }

        private void DrawExtraInfo(int playerId)
        {
            guiStyle.normal.textColor = Style.ExtraInfo.Color;
            guiStyle.alignment = TextAnchor.UpperLeft;

            var left = Screen.width / 2 - 20;
            var top = 136;
            if (Managers.COOP_ENABLED)
                top = 136 - 56;

            GUI.Label(new Rect(14, top, 60, 20),  Strings.Land + ": " + (Managers.Inst.game.currentLand + 1), guiStyle);
            GUI.Label(new Rect(14 + 60, top, 60, 20), Strings.Days + ": " + (Managers.Inst.director.CurrentDayForSpawning), guiStyle);

            float currentTime = Managers.Inst.director.currentTime;
            var currentHour = Math.Truncate(currentTime);
            var currentMints = Math.Truncate((currentTime - currentHour) * 60);
            GUI.Label(new Rect(left, top + 22, 40, 20), $"{currentHour:00.}:{currentMints:00.}", guiStyle);

            var player = Managers.Inst.kingdom.GetPlayer(playerId);
            if (player != null)
            {
                GUI.Label(new Rect(Screen.width - 126, 136 + 22, 60, 20), Strings.Gems + ": " + player.gems, guiStyle);
                GUI.Label(new Rect(Screen.width - 66, 136 + 22, 60, 20), Strings.Coins + ": " + player.coins, guiStyle);
            }
        }

        private class WallPoint
        {
            public Vector3 Pos;
            public Color Color;

            public WallPoint(Vector3 pos, Color color)
            {
                this.Pos = pos;
                this.Color = color;
            }
        }

        private class LineInfo
        {
            public Vector2 LineStart;
            public Vector2 LineEnd;
            public Color Color;
        }

        public enum MarkRow
        {
            Settled = 0,
            Movable = 1
        }

        private class MarkInfo
        {
            public float WorldPosX;
            public float Pos;
            public Color Color;
            public string Sign;
            public string Name;
            public MarkRow NameRow;
            public int Count;
            public bool Visible;

            public MarkInfo(float worldPosX, Color color, string sign, string name, int count = 0, MarkRow nameRow = MarkRow.Settled)
            {
                this.WorldPosX = worldPosX;
                this.Color = color;
                this.Sign = sign;
                this.Name = name;
                this.NameRow = nameRow;
                this.Count = count;
            }
        }

        private class StatsInfo
        {
            public int PeasantCount;
            public int WorkerCount;
            public int ArcherCount;
            public int FarmerCount;
            public int MaxFarmlands;
        }

    }

    public static class EnumUtil
    {
        public static System.Collections.Generic.IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

}