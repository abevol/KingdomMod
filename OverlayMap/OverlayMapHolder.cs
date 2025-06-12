using System;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;
using System.IO;
using System.Reflection;
using KingdomMod.OverlayMap.Config;

#if IL2CPP
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
#else
using System.Collections.Generic;
#endif

namespace KingdomMod.OverlayMap;

public partial class OverlayMapHolder : MonoBehaviour
{
    public static OverlayMapHolder Instance { get; private set; }
    private static ManualLogSource log;
    private readonly GUIStyle guiStyle = new();
    private GUIStyle _guiBoxStyle = new();
    public bool NeedToReloadGuiBoxStyle = true;
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

    public OverlayMapHolder()
    {
        guiStyle.alignment = TextAnchor.UpperLeft;
        guiStyle.normal.textColor = Color.white;
        guiStyle.fontSize = 12;
    }

    public static void Initialize(OverlayMapPlugin plugin)
    {
        log = plugin.LogSource;
#if IL2CPP
            ClassInjector.RegisterTypeInIl2Cpp<OverlayMapHolder>();
#endif
        GameObject obj = new(nameof(OverlayMapHolder));
        DontDestroyOnLoad(obj);
        obj.hideFlags = HideFlags.HideAndDontSave;
        Instance = obj.AddComponent<OverlayMapHolder>();
        Global.ConfigBind(plugin.Config);
    }

    private void Start()
    {
        log.LogMessage($"{this.GetType().Name} Start.");

        Patchers.Patcher.PatchAll();
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

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
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

        if (timeSinceLastGuiUpdate > (1.0 / Global.GuiUpdatesPerSecond))
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
            if (NeedToReloadGuiBoxStyle)
            {
                NeedToReloadGuiBoxStyle = false;
                ReloadGuiStyle();
            }
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

    public void OnGameStart()
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

        _exploredRegion.Init(GetLocalPlayer(), _archiveFilename);
    }

    public void ReloadGuiStyle()
    {
        _guiBoxStyle = new GUIStyle(GUI.skin.box);
        var guiStylePath = Path.Combine(GetBepInExDir(), "config", "GuiStyle");
        var bgImageFile = Path.Combine(guiStylePath, GuiStyle.BackgroundImageFile);
        LogMessage($"ReloadGuiStyle: \n" +
                   $"bgImageFile={bgImageFile}\n" +
                   $"BackgroundImageArea={GuiStyle.BackgroundImageArea.Value}\n" +
                   $"BackgroundImageBorder={GuiStyle.BackgroundImageBorder.Value}");
        if (File.Exists(bgImageFile))
        {
            byte[] imageData = File.ReadAllBytes(bgImageFile);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(imageData);

            var imageArea = (RectInt)GuiStyle.BackgroundImageArea;
            imageArea.y = texture.height - imageArea.y - imageArea.height;
            Texture2D subTexture = new Texture2D(imageArea.width, imageArea.height, TextureFormat.RGBA32, false);
            Color[] pixels = texture.GetPixels(imageArea.x, imageArea.y, imageArea.width, imageArea.height);
            if (pixels != null)
            {
                subTexture.SetPixels(pixels);
                subTexture.Apply();

                _guiBoxStyle.normal.background = MakeColoredTexture(subTexture, GuiStyle.BackgroundColor);
                _guiBoxStyle.stretchWidth = false;
                _guiBoxStyle.stretchHeight = false;
                _guiBoxStyle.border = GuiStyle.BackgroundImageBorder;
            }
            else
            {
                LogError("ReloadGuiStyle, failed to get pixels from texture.");
            }
        }
        else
        {
            LogError("ReloadGuiStyle, bgImageFile not exist.");
        }
    }

    private Texture2D MakeColoredTexture(Texture2D source, Color color)
    {
        Texture2D result = new Texture2D(source.width, source.height);

        Color[] pixels = source.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            float alpha = color.a;

            pixels[i] = new Color(
                pixels[i].r * (1 - alpha) + color.r * alpha,
                pixels[i].g * (1 - alpha) + color.g * alpha,
                pixels[i].b * (1 - alpha) + color.b * alpha,
                pixels[i].a * (1 - alpha) + color.a * alpha
            );
        }

        result.SetPixels(pixels);
        result.Apply();

        return result;
    }


    private void OnCurrentCampaignSwitch()
    {
        log.LogMessage($"OnCurrentCampaignSwitch: {GlobalSaveData.loaded.currentCampaign}");

    }

    public static Player GetLocalPlayer()
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

        foreach (var obj in kingdom.AllPortals)
        {
            if (obj.type == Portal.Type.Regular)
                poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.Portal.Color, MarkerStyle.Portal.Sign, Strings.Portal));
            else if (obj.type == Portal.Type.Cliff)
                poiList.Add(new MarkInfo(obj.transform.position.x, obj.state switch{ Portal.State.Destroyed => MarkerStyle.PortalCliff.Destroyed.Color, Portal.State.Rebuilding => MarkerStyle.PortalCliff.Rebuilding.Color, _=> MarkerStyle.PortalCliff.Color }, MarkerStyle.PortalCliff.Sign, Strings.PortalCliff));
            else if (obj.type == Portal.Type.Dock)
                poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.PortalDock.Color, MarkerStyle.PortalDock.Sign, Strings.PortalDock));
        }

        var beach = gameLayer.GetComponentInChildren<Beach>();
        if (beach != null)
            poiList.Add(new MarkInfo(beach.transform.position.x, MarkerStyle.Beach.Color, MarkerStyle.Beach.Sign, Strings.Beach));

        foreach (var beggarCamp in kingdom.BeggarCamps)
        {
            int count = 0;
            foreach (var beggar in beggarCamp._beggars)
            {
                if (beggar != null && beggar.isActiveAndEnabled)
                    count++;
            }
            poiList.Add(new MarkInfo(beggarCamp.transform.position.x, MarkerStyle.BeggarCamp.Color, MarkerStyle.BeggarCamp.Sign, Strings.BeggarCamp, count));
        }

        foreach (var beggar in kingdom.Beggars)
        {
            if (beggar == null) continue;

            if (beggar.hasFoundBaker)
            {
                poiList.Add(new MarkInfo(beggar.transform.position.x, MarkerStyle.Beggar.Color, MarkerStyle.Beggar.Sign, Strings.Beggar, 0, MarkRow.Movable));
            }
        }

        foreach (var player in new System.Collections.Generic.List<Player>{ kingdom.playerOne, kingdom.playerTwo })
        {
            if (player == null) continue;
            if (player.isActiveAndEnabled == false) continue;
            var mover = player.mover;
            if (mover == null) continue;

            poiList.Add(new MarkInfo(mover.transform.position.x, MarkerStyle.Player.Color, MarkerStyle.Player.Sign, player.playerId == 0 ? Strings.P1 : Strings.P2, 0, MarkRow.Movable));
            float l = mover.transform.position.x - 12;
            float r = mover.transform.position.x + 12;
            if (l < _exploredRegion.ExploredLeft)
                _exploredRegion.ExploredLeft = l;
            if (r > _exploredRegion.ExploredRight)
                _exploredRegion.ExploredRight = r;
        }

        if (kingdom.teleExitP1)
            poiList.Add(new MarkInfo(kingdom.teleExitP1.transform.position.x, MarkerStyle.TeleExitP1.Color, MarkerStyle.TeleExitP1.Sign, Strings.TeleExitP1, 0, MarkRow.Movable));

        if (kingdom.teleExitP2)
            poiList.Add(new MarkInfo(kingdom.teleExitP2.transform.position.x, MarkerStyle.TeleExitP2.Color, MarkerStyle.TeleExitP2.Sign, Strings.TeleExitP2, 0, MarkRow.Movable));

        var deers = GameExtensions.FindObjectsWithTagOfType<Deer>(Tags.Wildlife);
        foreach (var deer in deers)
        {
            if (!deer._damageable.isDead)
                poiList.Add(new MarkInfo(deer.transform.position.x, deer._fsm.Current == 5 ? MarkerStyle.DeerFollowing.Color : MarkerStyle.Deer.Color, MarkerStyle.Deer.Sign, Strings.Deer, 0, MarkRow.Movable));
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
                    poiList.Add(new MarkInfo(leftBosses[0], MarkerStyle.Boss.Color, MarkerStyle.Boss.Sign, Strings.Boss, leftBosses.Count, MarkRow.Movable));
                    if (leftEnemies[0] - leftBosses[0] < 6)
                        drawEnemies = false;
                }

                if (drawEnemies)
                    poiList.Add(new MarkInfo(leftEnemies[0], MarkerStyle.Enemy.Color, MarkerStyle.Enemy.Sign, Strings.Enemy, leftEnemies.Count, MarkRow.Movable));
            }

            if (rightEnemies.Count > 0)
            {
                var drawEnemies = true;
                rightEnemies.Sort((a, b) => a.CompareTo(b));

                if (rightBosses.Count > 0)
                {
                    rightBosses.Sort((a, b) => a.CompareTo(b));
                    poiList.Add(new MarkInfo(rightBosses[0], MarkerStyle.Boss.Color, MarkerStyle.Boss.Sign, Strings.Boss, rightBosses.Count, MarkRow.Movable));
                    if (rightBosses[0] - rightEnemies[0] < 6)
                        drawEnemies = false;
                }

                if (drawEnemies)
                    poiList.Add(new MarkInfo(rightEnemies[0], MarkerStyle.Enemy.Color, MarkerStyle.Enemy.Sign, Strings.Enemy, rightEnemies.Count, MarkRow.Movable));
            }
        }

        var castle = kingdom.castle;
        if (castle != null)
        {
            var payable = castle._payableUpgrade;
            bool canPay = !payable.IsLocked(GetLocalPlayer(), out var reason);
            bool isLocked = reason != LockIndicator.LockReason.NotLocked && reason != LockIndicator.LockReason.NoUpgrade;
            bool isLockedForInvalidTime = reason == LockIndicator.LockReason.InvalidTime;
            var price = isLockedForInvalidTime ? (int)(payable.timeAvailableFrom - Time.time) : canPay ? payable.Price : 0;
            var color = isLocked ? MarkerStyle.Castle.Locked.Color : MarkerStyle.Castle.Color;
            poiList.Add(new MarkInfo(castle.transform.position.x, color, MarkerStyle.Castle.Sign, Strings.Castle, price));

            leftWalls.Add(new WallPoint(castle.transform.position, MarkerStyle.WallLine.Color));
            rightWalls.Add(new WallPoint(castle.transform.position, MarkerStyle.WallLine.Color));
        }

        var campfire = kingdom.campfire;
        if (campfire !=  null)
        {
            poiList.Add(new MarkInfo(campfire.transform.position.x, MarkerStyle.Campfire.Color, MarkerStyle.Campfire.Sign, Strings.Campfire));
        }

        var chestList = gameLayer.GetComponentsInChildren<Chest>();
        foreach (var obj in chestList)
        {
            if (obj.currencyAmount == 0) continue;

            if (obj.currencyType == CurrencyType.Gems)
                poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.GemChest.Color, MarkerStyle.GemChest.Sign, Strings.GemChest, obj.currencyAmount));
            else
                poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.Chest.Color, MarkerStyle.Chest.Sign, Strings.Chest, obj.currencyAmount));
        }

        foreach (var obj in kingdom._walls)
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.Wall.Color, MarkerStyle.Wall.Sign, ""));
            if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                leftWalls.Add(new WallPoint(obj.transform.position, MarkerStyle.WallLine.Color));
            else
                rightWalls.Add(new WallPoint(obj.transform.position, MarkerStyle.WallLine.Color));
        }

        var shopForge = GameObject.FindGameObjectWithTag(Tags.ShopForge);
        if (shopForge != null)
        {
            poiList.Add(new MarkInfo(shopForge.transform.position.x, MarkerStyle.ShopForge.Color, MarkerStyle.ShopForge.Sign, Strings.ShopForge));
        }

        var citizenHouses = GameObject.FindGameObjectsWithTag(Tags.CitizenHouse);
        foreach (var obj in citizenHouses)
        {
            var citizenHouse = obj.GetComponent<CitizenHousePayable>();
            if (citizenHouse != null)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.CitizenHouse.Color, MarkerStyle.CitizenHouse.Sign, Strings.CitizenHouse, citizenHouse._numberOfAvailableCitizens));
            }
        }

        var lighthouses = GameObject.FindGameObjectsWithTag(Tags.Lighthouse);
        foreach (var obj in lighthouses)
        {
            var payable = obj.GetComponent<PayableUpgrade>();
            LockIndicator.LockReason reason = LockIndicator.LockReason.NotLocked;
            bool canPay = payable != null && !payable.IsLocked(GetLocalPlayer(), out reason);
            bool isLocked = payable != null && (reason != LockIndicator.LockReason.NotLocked && reason != LockIndicator.LockReason.NoUpgrade);
            var price = canPay ? payable.Price : 0;
            var color = isLocked ? MarkerStyle.Lighthouse.Locked.Color : MarkerStyle.Lighthouse.Color;
            poiList.Add(new MarkInfo(obj.transform.position.x, color, MarkerStyle.Lighthouse.Sign, Strings.Lighthouse, price));
        }

        var wallWreckList = GameObject.FindGameObjectsWithTag(Tags.WallWreck);
        foreach (var obj in wallWreckList)
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.Wall.Wrecked.Color, MarkerStyle.Wall.Sign, ""));
            if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                leftWalls.Add(new WallPoint(obj.transform.position, MarkerStyle.WallLine.Wrecked.Color));
            else
                rightWalls.Add(new WallPoint(obj.transform.position, MarkerStyle.WallLine.Wrecked.Color));
        }

        var wallFoundation = GameObject.FindGameObjectsWithTag(Tags.WallFoundation);
        foreach (var obj in wallFoundation)
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.WallFoundation.Color, MarkerStyle.WallFoundation.Sign, ""));
        }

        var riverList = gameLayer.GetComponentsInChildren<River>();
        foreach (var obj in riverList)
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.River.Color, MarkerStyle.River.Sign, ""));
        }

        foreach (var obj in Managers.Inst.world._berryBushes)
        {
            if (obj.paid)
                poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.BerryBushPaid.Color, MarkerStyle.BerryBushPaid.Sign, ""));
            else
                poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.BerryBush.Color, MarkerStyle.BerryBush.Sign, ""));
        }

        var payableGemChest = GameExtensions.GetPayableOfType<PayableGemChest>();
        if (payableGemChest != null)
        {
            var gemsCount = payableGemChest.infiniteGems ? payableGemChest.guardRef.Price : payableGemChest.gemsStored;
            poiList.Add(new MarkInfo(payableGemChest.transform.position.x, MarkerStyle.GemMerchant.Color, MarkerStyle.GemMerchant.Sign, Strings.GemMerchant, gemsCount));
        }

        var dogSpawn = GameExtensions.GetPayableBlockerOfType<DogSpawn>();
        if (dogSpawn != null && !dogSpawn._dogFreed)
            poiList.Add(new MarkInfo(dogSpawn.transform.position.x, MarkerStyle.DogSpawn.Color, MarkerStyle.DogSpawn.Sign, Strings.DogSpawn));

        var boarSpawn = world.boarSpawnGroup;
        if (boarSpawn != null)
        {
            poiList.Add(new MarkInfo(boarSpawn.transform.position.x, MarkerStyle.BoarSpawn.Color, MarkerStyle.BoarSpawn.Sign,
                Strings.BoarSpawn, boarSpawn._spawnedBoar ? 0 : 1));
        }

        var caveHelper = Managers.Inst.caveHelper;
        if (caveHelper != null && caveHelper.CurrentlyBombingPortal != null)
        {
            var bomb = caveHelper.Getbomb(caveHelper.CurrentlyBombingPortal.side);
            if (bomb != null)
            {
                poiList.Add(new MarkInfo(bomb.transform.position.x, MarkerStyle.Bomb.Color, MarkerStyle.Bomb.Sign, Strings.Bomb, 0, MarkRow.Movable));
            }
        }

        foreach (var obj in kingdom.GetFarmHouses())
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.Farmhouse.Color, MarkerStyle.Farmhouse.Sign, Strings.Farmhouse));
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
                poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.Steeds.Color, MarkerStyle.Steeds.Sign, steedName, obj.Price));
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
                poiList.Add(new MarkInfo(obj.transform.position.x, MarkerStyle.SteedSpawns.Color, MarkerStyle.SteedSpawns.Sign, info, obj.Price));
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

            var color = obj.canPay ? MarkerStyle.HermitCabins.Locked.Color : MarkerStyle.HermitCabins.Unlocked.Color;
            var price = obj.canPay ? obj.Price : 0;
            poiList.Add(new MarkInfo(obj.transform.position.x, color, MarkerStyle.HermitCabins.Sign, info, price));
        }

        if (_persephoneCage)
        {
            var color = PersephoneCage.State.IsPersephoneLocked(_persephoneCage._fsm.Current) ? MarkerStyle.PersephoneCage.Locked.Color : MarkerStyle.PersephoneCage.Unlocked.Color;
            poiList.Add(new MarkInfo(_persephoneCage.transform.position.x, color, MarkerStyle.PersephoneCage.Sign, Strings.HermitPersephone, 0));
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
                Statue.Deity.Pike => Strings.StatuePike,
                _ => ""
            };

            bool isLocked = obj.deityStatus != Statue.DeityStatus.Activated;
            var color = isLocked ? MarkerStyle.Statues.Locked.Color : MarkerStyle.Statues.Unlocked.Color;
            var price = isLocked ? obj.Price : 0;
            poiList.Add(new MarkInfo(obj.transform.position.x, color, MarkerStyle.Statues.Sign, info, price));
        }

        var timeStatue = kingdom.timeStatue;
        if (timeStatue)
            poiList.Add(new MarkInfo(timeStatue.transform.position.x, MarkerStyle.StatueTime.Color, MarkerStyle.StatueTime.Sign, Strings.StatueTime, timeStatue.daysRemaining));

        // var wharf = kingdom.wharf;
        var boat = kingdom.boat;
        if (boat)
            poiList.Add(new MarkInfo(boat.transform.position.x, MarkerStyle.Boat.Color, MarkerStyle.Boat.Sign, Strings.Boat));
        else
        {
            var wreck = kingdom.wreckPlaceholder;
            if (wreck)
                poiList.Add(new MarkInfo(wreck.transform.position.x, MarkerStyle.Boat.Wrecked.Color, MarkerStyle.Boat.Sign, Strings.BoatWreck));
        }

        var summonBell = kingdom.boatSailPosition?.GetComponentInChildren<BoatSummoningBell>();
        if (summonBell)
            poiList.Add(new MarkInfo(summonBell.transform.position.x, MarkerStyle.SummonBell.Color, MarkerStyle.SummonBell.Sign, Strings.SummonBell));

        var hephaestusForge = gameLayer.GetComponentInChildren<HephaestusForge>();
        if (hephaestusForge)
            poiList.Add(new MarkInfo(hephaestusForge.transform.position.x, MarkerStyle.HephaestusForge.Color, MarkerStyle.HephaestusForge.Sign, Strings.HephaestusForge));

        foreach (var obj in payables.AllPayables)
        {
            if (obj == null) continue;
            var go = obj.gameObject;
            if (go == null) continue;
            var prefab = go.GetComponent<PrefabID>();
            if (prefab == null) continue;

            if (prefab.prefabID == (int)GamePrefabID.Quarry_undeveloped)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.Quarry.Locked.Color, MarkerStyle.Quarry.Sign, Strings.Quarry, obj.Price));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Mine_undeveloped)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.Mine.Locked.Color, MarkerStyle.Mine.Sign, Strings.Mine, obj.Price));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Lighthouse_undeveloped)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.Lighthouse.Unpaid.Color, MarkerStyle.Lighthouse.Sign, Strings.Lighthouse, obj.Price));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Cliff_Portal)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.PortalCliff.Color, MarkerStyle.PortalCliff.Sign, Strings.PortalCliff, obj.Price));
            }
            else
            {
                var unlockNewRulerStatue = go.GetComponent<UnlockNewRulerStatue>();
                if (unlockNewRulerStatue != null)
                {
                    var color = unlockNewRulerStatue.status switch
                    {
                        UnlockNewRulerStatue.Status.Locked => MarkerStyle.RulerSpawns.Locked.Color,
                        UnlockNewRulerStatue.Status.WaitingForArcher => MarkerStyle.RulerSpawns.Building.Color,
                        _ => MarkerStyle.RulerSpawns.Unlocked.Color
                    };
                    if (color != MarkerStyle.RulerSpawns.Unlocked.Color)
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
                        poiList.Add(new MarkInfo(go.transform.position.x, color, MarkerStyle.RulerSpawns.Sign, markName, obj.Price));
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
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.Quarry.Unlocked.Color, MarkerStyle.Quarry.Sign, Strings.Quarry));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Mine)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.Mine.Unlocked.Color, MarkerStyle.Mine.Sign, Strings.Mine));
            }
            else if (prefab.prefabID == (int)GamePrefabID.MerchantHouse)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.MerchantHouse.Color, MarkerStyle.MerchantHouse.Sign, Strings.MerchantHouse));
            }
            else
            {
                var thorPuzzleController = go.GetComponent<ThorPuzzleController>();
                if (thorPuzzleController != null)
                {
                    var color = thorPuzzleController.State == 0 ? MarkerStyle.ThorPuzzleStatue.Locked.Color : MarkerStyle.ThorPuzzleStatue.Unlocked.Color;
                    poiList.Add(new MarkInfo(thorPuzzleController.transform.position.x, color, MarkerStyle.ThorPuzzleStatue.Sign, Strings.ThorPuzzleStatue));
                }

                var helPuzzleController = go.GetComponent<HelPuzzleController>();
                if (helPuzzleController != null)
                {
                    var color = helPuzzleController.State == 0 ? MarkerStyle.HelPuzzleStatue.Locked.Color : MarkerStyle.HelPuzzleStatue.Unlocked.Color;
                    poiList.Add(new MarkInfo(helPuzzleController.transform.position.x, color, MarkerStyle.HelPuzzleStatue.Sign, Strings.HelPuzzleStatue));
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
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.Wall.Building.Color, MarkerStyle.Wall.Sign, ""));
                if (kingdom.GetBorderSideForPosition(go.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(go.transform.position, MarkerStyle.WallLine.Building.Color));
                else
                    rightWalls.Add(new WallPoint(go.transform.position, MarkerStyle.WallLine.Building.Color));
            }

            var prefab = go.GetComponent<PrefabID>();
            if (prefab == null) continue;
            if (prefab.prefabID == (int)GamePrefabID.Quarry)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.Quarry.Building.Color, MarkerStyle.Quarry.Sign, Strings.Quarry));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Mine)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, MarkerStyle.Mine.Building.Color, MarkerStyle.Mine.Sign, Strings.Mine));
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
                    LineStart = new Vector2((beginPoint.Pos.x - startPos) * scale + 16, 7),
                    LineEnd = new Vector2((endPoint.Pos.x - startPos) * scale + 16, 7),
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
                    LineStart = new Vector2((beginPoint.Pos.x - startPos) * scale + 16, 7),
                    LineEnd = new Vector2((endPoint.Pos.x - startPos) * scale + 16, 7),
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
        GUI.Box(boxRect, "", _guiBoxStyle);

        foreach (var line in drawLineList)
        {
            GuiHelper.DrawLine(line.LineStart, line.LineEnd, line.Color, 2);
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
                    color = MarkerStyle.PlayerSelf.Color;
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
        guiStyle.normal.textColor = MarkerStyle.StatsInfo.Color;
        guiStyle.alignment = TextAnchor.UpperLeft;

        float boxTop = 160;
        if (Managers.COOP_ENABLED)
            boxTop = 160 - 56;

        var kingdom = Managers.Inst.kingdom;
        var boxRect = new Rect(5, boxTop, 120, 146);
        GUI.Box(boxRect, "", _guiBoxStyle);

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
        guiStyle.normal.textColor = MarkerStyle.ExtraInfo.Color;
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

    public static string GetBepInExDir()
    {
        var baseDir = Assembly.GetExecutingAssembly().Location;
        var bepInExDir = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName;

        bepInExDir ??= "BepInEx\\";
        return bepInExDir;
    }
}

public static class EnumUtil
{
    public static System.Collections.Generic.IEnumerable<T> GetValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }
}