using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;
using static KingdomMod.OverlayMap.Config;

namespace KingdomMod
{
    public partial class OverlayMap : MonoBehaviour
    {
        private static ManualLogSource log;
        private readonly GUIStyle guiStyle = new();
        private int tick = 0;
        private bool enabledOverlayMap = true;
        private List<MarkInfo> minimapMarkList = new();
        private List<LineInfo> drawLineList = new();
        private readonly StatsInfo statsInfo = new();
        private float exploredLeft = 0;
        private float exploredRight = 0;
        private bool showFullMap = false;
        private GameObject gameLayer = null;

        public static void LogMessage(string message, 
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath]   string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            log.LogMessage($"[{sourceLineNumber}][{memberName}] {message}");
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
            log = plugin.Log;
            Config.Global.ConfigBind(plugin.Config);
            var component = plugin.AddComponent<OverlayMap>();
            component.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(component.gameObject);
        }

        private void Start()
        {
            log.LogMessage($"{this.GetType().Name} Start.");
            Patcher.PatchAll(this);
            Game.add_OnGameStart((Action)OnGameStart);
            NetworkBigBoss.Instance._postCatchupEvent += (Action)this.OnClientCaughtUp;

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

            tick = tick + 1;
            if (tick > 60)
                tick = 0;

            if (tick == 0)
            {
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
            return game.state is Game.State.Playing or Game.State.NetworkClientPlaying or Game.State.Menu;
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

        private void OnGameStart()
        {
            log.LogMessage("OnGameStart.");

            exploredLeft = exploredRight = 0;
            gameLayer = GameObject.FindGameObjectWithTag(Tags.GameLayer);
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
            var poiList = new List<MarkInfo>();
            var leftWalls = new List<WallPoint>();
            var rightWalls = new List<WallPoint>();

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

            foreach (var beggar in kingdom.beggars)
            {
                if (beggar == null) continue;

                if (beggar.hasFoundBaker)
                {
                    poiList.Add(new MarkInfo(beggar.transform.position.x, Style.Beggar.Color, Style.Beggar.Sign, Strings.Beggar, 0, MarkRow.Movable));
                }
            }

            foreach (var player in new List<Player>{ kingdom.playerOne, kingdom.playerTwo })
            {
                if (player == null) continue;
                if (player.isActiveAndEnabled == false) continue;
                var mover = player.mover;
                if (mover == null) continue;

                poiList.Add(new MarkInfo(mover.transform.position.x, Style.Player.Color, Style.Player.Sign, player.playerId == 0 ? Strings.P1 : Strings.P2, 0, MarkRow.Movable));
                float l = mover.transform.position.x - 12;
                float r = mover.transform.position.x + 12;
                if (l < exploredLeft)
                    exploredLeft = l;
                if (r > exploredRight)
                    exploredRight = r;
            }

            var deers = FindObjectsWithTagOfType<Deer>(Tags.Wildlife);
            foreach (var deer in deers)
            {
                if (!deer._damageable.isDead)
                    poiList.Add(new MarkInfo(deer.transform.position.x, deer._fsm.current == 5 ? Style.DeerFollowing.Color : Style.Deer.Color, Style.Deer.Sign, Strings.Deer, 0, MarkRow.Movable));
            }

            var enemies = Managers.Inst.enemies._enemies;
            if (enemies != null && enemies.Count > 0)
            {
                var leftEnemies = new List<float>();
                var rightEnemies = new List<float>();
                var leftBosses = new List<float>();
                var rightBosses = new List<float>();
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
                var reason = payable.IsLocked(GetLocalPlayer());
                bool canPay = reason == PayableUpgrade.LockedReason.NotLocked;
                bool isLocked = reason != PayableUpgrade.LockedReason.NotLocked && reason != PayableUpgrade.LockedReason.NoUpgrade;
                bool isLockedForInvalidTime = reason == PayableUpgrade.LockedReason.InvalidTime;
                var price = isLockedForInvalidTime ? (int)(payable.timeAvailableFrom - Time.time) : canPay ? payable.price : 0;
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
                if (obj.coins == 0) continue;

                if (obj.isGems)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.GemChest.Color, Style.GemChest.Sign, Strings.GemChest, obj.coins));
                else
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.Chest.Color, Style.Chest.Sign, Strings.Chest, obj.coins));
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
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.CitizenHouse.Color, Style.CitizenHouse.Sign, Strings.CitizenHouse, citizenHouse._numberOfAvaliableCitizens));
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

            var payableGemChest = GetPayableOfType<PayableGemChest>();
            if (payableGemChest != null)
            {
                var gemsCount = payableGemChest.infiniteGems ? payableGemChest.guardRef.price : payableGemChest.gemsStored;
                poiList.Add(new MarkInfo(payableGemChest.transform.position.x, Style.GemMerchant.Color, Style.GemMerchant.Sign, Strings.GemMerchant, gemsCount));
            }

            var dogSpawn = GetPayableBlockerOfType<DogSpawn>();
            if (dogSpawn != null && !dogSpawn._dogFreed)
                poiList.Add(new MarkInfo(dogSpawn.transform.position.x, Style.DogSpawn.Color, Style.DogSpawn.Sign, Strings.DogSpawn));

            var boarSpawn = world.boarSpawnGroup;
            if (boarSpawn != null)
            {
                poiList.Add(new MarkInfo(boarSpawn.transform.position.x, Style.BoarSpawn.Color, Style.BoarSpawn.Sign, Strings.BoarSpawn, boarSpawn._spawnedBoar ? 0 : 1));
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

            foreach (var obj in kingdom._farmHouses)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Style.Farmhouse.Color, Style.Farmhouse.Sign, Strings.Farmhouse));
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
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.Steeds.Color, Style.Steeds.Sign, steedNames[obj.steedType], obj.price));
            }

            foreach (var obj in kingdom.steedSpawns)
            {
                var info = "";
                foreach (var steedTmp in obj.steedPool)
                {
                    info = steedNames[steedTmp.steedType];
                }

                if (!obj._hasSpawned)
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.SteedSpawns.Color, Style.SteedSpawns.Sign, info, obj.price));
            }
            
            var cabinList = GetPayablesOfType<Cabin>();
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
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.HermitCabins.Color, Style.HermitCabins.Sign, info, obj.price));
            }

            var statueList = GetPayablesOfType<Statue>();
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
                    poiList.Add(new MarkInfo(obj.transform.position.x, Style.Statues.Color, Style.Statues.Sign, info, obj.price));
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

            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var go = obj.gameObject;
                if (go == null) continue;
                var prefab = go.GetComponent<PrefabID>();
                if (prefab == null) continue;

                if (prefab.prefabID == (int)PrefabIDs.Quarry_undeveloped)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Quarry.Color, Style.Quarry.Sign, Strings.Quarry, obj.price));
                }
                else if (prefab.prefabID == (int)PrefabIDs.Mine_undeveloped)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Mine.Color, Style.Mine.Sign, Strings.Mine, obj.price));
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
                            poiList.Add(new MarkInfo(go.transform.position.x, color, Style.RulerSpawns.Sign, markName, obj.price));
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
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Quarry.Unlocked.Color, Style.Quarry.Sign, Strings.Quarry));
                }
                else if (prefab.prefabID == (int)PrefabIDs.Mine)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Mine.Unlocked.Color, Style.Mine.Sign, Strings.Mine));
                }
                else if (prefab.prefabID == (int)PrefabIDs.MerchantHouse)
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
                var go = scaffolding.building;
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
                if (prefab.prefabID == (int)PrefabIDs.Quarry)
                {
                    poiList.Add(new MarkInfo(go.transform.position.x, Style.Quarry.Building.Color, Style.Quarry.Sign, Strings.Quarry));
                }
                else if (prefab.prefabID == (int)PrefabIDs.Mine)
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
                else if(poi.WorldPosX >= exploredLeft && poi.WorldPosX <= exploredRight)
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

            var lineList = new List<LineInfo>();
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

        private T GetPayableOfType<T>() where T : Component
        {
            var payables = Managers.Inst.payables;
            if (!payables) return null;
            
            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    return comp;
            }

            return null;
        }

        private List<T> GetPayablesOfType<T>() where T : Component
        {
            var result = new List<T>();
            var payables = Managers.Inst.payables;
            if (!payables) return result;

            foreach (var obj in payables.AllPayables)
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    result.Add(comp);
            }

            return result;
        }

        private T GetPayableBlockerOfType<T>() where T : Component
        {
            var payables = Managers.Inst.payables;
            if (!payables) return null;

            foreach (var obj in payables._allBlockers)
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    return comp;
            }

            return null;
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
            Rect boxRect = new Rect(5, 5, Screen.width - 10, 150);
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

        private static List<T> FindObjectsWithTagOfType<T>(string tagName)
        {
            var list = new List<T>();
            foreach (var obj in GameObject.FindGameObjectsWithTag(tagName))
            {
                if (obj == null) continue;
                var comp = obj.GetComponent<T>();
                if (comp != null)
                    list.Add(comp);
            }

            return list;
        }

        private static List<Character> FindCharactersOfType<T>()
        {
            var list = new List<Character>();
            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return list;
            foreach (var character in kingdom._characters)
            {
                if (character == null) continue;
                if (character.GetComponent<T>() != null)
                    list.Add(character);
            }
            return list;
        }

        private void UpdateStatsInfo()
        {
            var kingdom = Managers.Inst.kingdom;
            if (kingdom == null) return;

            var peasantList = GameObject.FindGameObjectsWithTag(Tags.Peasant);
            statsInfo.PeasantCount = peasantList.Count;

            var workerList = kingdom._workers;
            statsInfo.WorkerCount = workerList.Count;

            var archerList = kingdom._archers;
            statsInfo.ArcherCount = archerList.Count;

            var farmerList = kingdom._farmers;
            statsInfo.FarmerCount = farmerList.Count;

            var farmhouseList = kingdom._farmHouses;
            int maxFarmlands = 0;
            foreach (var obj in farmhouseList)
            {
                maxFarmlands += obj.CurrentMaxFarmlands();
            }
            statsInfo.MaxFarmlands = maxFarmlands;
        }

        private int GetArcherCount(ArcherType archerType)
        {
            var result = 0;
            foreach (var obj in Managers.Inst.kingdom._archers)
            {
                if (archerType == ArcherType.Free)
                {
                    if (!obj.inGuardSlot && !obj.isKnightSoldier)
                        result++;
                }
                else if (archerType == ArcherType.GuardSlot)
                {
                    if (obj.inGuardSlot)
                        result++;
                }
                else if (archerType == ArcherType.KnightSoldier)
                {
                    if (obj.isKnightSoldier)
                        result++;
                }
            }

            return result;
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

        private void DrawStatsInfo(int playerId)
        {
            guiStyle.normal.textColor = Style.StatsInfo.Color;
            guiStyle.alignment = TextAnchor.UpperLeft;

            var kingdom = Managers.Inst.kingdom;
            var boxRect = new Rect(5, 160, 120, 146);
            GUI.Box(boxRect, "");
            GUI.Box(boxRect, "");

            GUI.Label(new Rect(14, 166 + 20 * 0, 120, 20), Strings.Peasant + ": " + statsInfo.PeasantCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 1, 120, 20), Strings.Worker + ": " + statsInfo.WorkerCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 2, 120, 20), $"{Strings.Archer.Value}: {statsInfo.ArcherCount} ({GetArcherCount(ArcherType.Free)}|{GetArcherCount(ArcherType.GuardSlot)}|{GetArcherCount(ArcherType.KnightSoldier)})", guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 3, 120, 20), Strings.Pikeman + ": " + kingdom.Pikemen.Count, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 4, 120, 20), $"{Strings.Knight.Value}: {kingdom.knights.Count} ({GetKnightCount(true)})", guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 5, 120, 20), Strings.Farmer + ": " + statsInfo.FarmerCount, guiStyle);
            GUI.Label(new Rect(14, 166 + 20 * 6, 120, 20), Strings.Farmlands + ": " + statsInfo.MaxFarmlands, guiStyle);
        }

        private void DrawExtraInfo(int playerId)
        {
            guiStyle.normal.textColor = Style.ExtraInfo.Color;
            guiStyle.alignment = TextAnchor.UpperLeft;

            var left = Screen.width / 2 - 20;
            var top = 136;

            GUI.Label(new Rect(14, top, 60, 20),  Strings.Land + ": " + (Managers.Inst.game.currentLand + 1), guiStyle);
            GUI.Label(new Rect(14 + 60, top, 60, 20), Strings.Days + ": " + (Managers.Inst.director.CurrentDaysSinceFirstLandingThisReign), guiStyle);

            float currentTime = Managers.Inst.director.currentTime;
            var currentHour = Math.Truncate(currentTime);
            var currentMints = Math.Truncate((currentTime - currentHour) * 60);
            GUI.Label(new Rect(left, top + 22, 40, 20), $"{currentHour:00.}:{currentMints:00.}", guiStyle);

            var player = Managers.Inst.kingdom.GetPlayer(playerId);
            if (player != null)
            {
                GUI.Label(new Rect(Screen.width - 126, top + 22, 60, 20), Strings.Gems + ": " + player.gems, guiStyle);
                GUI.Label(new Rect(Screen.width - 66, top + 22, 60, 20), Strings.Coins + ": " + player.coins, guiStyle);
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

        private enum ArcherType
        {
            Free,
            GuardSlot,
            KnightSoldier
        }

        private enum PrefabIDs
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

    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

}