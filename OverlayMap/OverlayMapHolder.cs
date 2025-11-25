using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;
using System.IO;
using System.Reflection;
using KingdomMod.OverlayMap;
using KingdomMod.OverlayMap.Gui;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Coatsink.Common;
using KingdomMod.OverlayMap.Gui.Debugging;
using KingdomMod.SharedLib;
using KingdomMod.Shared.Attributes;


#if IL2CPP
using Il2CppInterop.Runtime.Injection;
#endif

namespace KingdomMod.OverlayMap;

[RegisterTypeInIl2Cpp]
public class OverlayMapHolder : MonoBehaviour
{
    public delegate void GameStateEventHandler(Game.State state);

    public delegate void ProgramDirectorStateEventHandler(ProgramDirector.State state);

    public static OverlayMapHolder Instance { get; private set; }
    private static ManualLogSource _log;
    private static BepInEx.Configuration.ConfigFile _config;
    private readonly GUIStyle _guiStyle = new();
    private GUIStyle _guiBoxStyle = new();
    private float _timeSinceLastGuiUpdate = 0;
    public bool NeedToReloadGuiBoxStyle = true;
    public static bool EnabledOverlayMap = true;
    private System.Collections.Generic.List<MarkInfo> _minimapMarkList = new();
    private System.Collections.Generic.List<LineInfo> _drawLineList = new();
    private readonly StatsInfo _statsInfo = new();
    public static bool ShowFullMap = false;
    private GameObject _gameLayer = null;
    private static PersephoneCage _persephoneCage;
    private static readonly CachePrefabID _cachePrefabID = new CachePrefabID();
    private Canvas _canvas;
    public GuiStyle guiStyle;
    public (PlayerOverlay P1, PlayerOverlay P2) PlayerOverlays = (null, null);
    public static event GameStateEventHandler OnGameStateChanged;
    public static event ProgramDirectorStateEventHandler OnProgramDirectorStateChanged;
    public static string BepInExDir;
    public static string AssetsDir;
    
    // 字体调试面板
    private SimpleFontDebugPanel _fontDebugPanel;
    private static bool _showFontDebugPanel = false;

#if IL2CPP
    public OverlayMapHolder(IntPtr ptr) : base(ptr) { }
#endif

    public static void Initialize(OverlayMapPlugin plugin)
    {
        _log = plugin.LogSource;
        _config = plugin.Config;

        BepInExDir = GetBepInExDir();
        AssetsDir = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap.Assets");

        GameObject obj = new(nameof(OverlayMapHolder));
        DontDestroyOnLoad(obj);
        obj.hideFlags = HideFlags.HideAndDontSave;
        Instance = obj.AddComponent<OverlayMapHolder>(); // Call "Awake" inside.
    }

    private void Awake()
    {
        Config.Global.ConfigBind(_config);
        Patchers.Patcher.PatchAll();

        CreateCanvas();
        CreateFontDebugPanel();
        guiStyle = CreateGuiStyle();
        PlayerOverlays.P1 = CreatePlayerOverlay(PlayerId.P1);
        PlayerOverlays.P2 = CreatePlayerOverlay(PlayerId.P2);

        Game.OnGameStart += (Action)OnGameStart;
        Game.OnGameEnd += (Action)OnGameEnd;

#if IL2CPP
        SceneManager.add_sceneLoaded(new System.Action<Scene, LoadSceneMode>(OnSceneLoaded));
#else
        SceneManager.sceneLoaded += OnSceneLoaded;
#endif
    }

    private void OnDestroy()
    {
        Game.OnGameStart -= (Action)OnGameStart;
        Game.OnGameEnd -= (Action)OnGameEnd;

#if IL2CPP
        Managers.Inst.game.run.GetOverrideMethodDelegate<IHagletCallable, System.Action<Il2CppSystem.Action<int, int>>>("remove_onGoto")
            .Invoke(new System.Action<int, int>(OnGameGoto));
        ProgramDirector.run.GetOverrideMethodDelegate<IHagletCallable, System.Action<Il2CppSystem.Action<int, int>>>("remove_onGoto")
            .Invoke(new System.Action<int, int>(OnProgramDirectorGoto));
        SceneManager.remove_sceneLoaded(new System.Action<Scene, LoadSceneMode>(OnSceneLoaded));
#else
        Managers.Inst.game.run.onGoto -= OnGameGoto;
        ProgramDirector.run.onGoto -= OnProgramDirectorGoto;
        SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
    }

    // 创建 Canvas 并设置 DPI 缩放
    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("UICanvas");
        canvasObj.transform.SetParent(this.transform, false);
        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    /// <summary>
    /// 创建字体调试面板
    /// </summary>
    private void CreateFontDebugPanel()
    {
        LogTrace("CreateFontDebugPanel");

        var debugPanelObj = new GameObject(nameof(SimpleFontDebugPanel));
        debugPanelObj.transform.SetParent(_canvas.transform, false);
        _fontDebugPanel = debugPanelObj.AddComponent<SimpleFontDebugPanel>();
        _fontDebugPanel.Initialize(_canvas);
    }

    private GuiStyle CreateGuiStyle()
    {
        LogTrace($"CreateGuiStyle");

        var guiObj = new GameObject(nameof(GuiStyle));
        guiObj.transform.SetParent(_canvas.transform, false);
        var guiComp = guiObj.AddComponent<GuiStyle>();

        return guiComp;
    }

    private PlayerOverlay CreatePlayerOverlay(PlayerId playerId)
    {
        LogTrace($"CreatePlayerOverlay, playerId: {playerId}");

        var guiObj = new GameObject(nameof(PlayerOverlay));
        guiObj.transform.SetParent(_canvas.transform, false);
        var guiComp = guiObj.AddComponent<PlayerOverlay>();
        guiComp.Init(playerId);

        return guiComp;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LogTrace($"Scene: {scene.name}, {scene.buildIndex}");

    }

    private void OnGamePlaying()
    {
    }

    private void OnGameQuitting()
    {
    }

    private void OnGameGoto(int pre, int next)
    {
        LogTrace($"OnGameGoto: pre:{pre}({(Game.State)pre}), {next}({(Game.State)next})");

        var state = (Game.State)next;
        switch (state)
        {
            case Game.State.Playing:
                OnGamePlaying();
                break;
            case Game.State.Quitting:
                OnGameQuitting();
                break;
        }

        OnGameStateChanged?.Invoke(state);
    }

    private void OnProgramDirectorGoto(int pre, int next)
    {
        LogTrace($"OnProgramDirectorGoto: pre:{pre}({(ProgramDirector.State)pre}), {next}({(ProgramDirector.State)next})");

        OnProgramDirectorStateChanged?.Invoke((ProgramDirector.State)next);
    }

    private void Start()
    {
        LogTrace($"{this.GetType().Name}.Start");

        NetworkBigBoss.Instance.OnClientCaughtUp += (Action)this.OnClientCaughtUp;

        _guiStyle.alignment = TextAnchor.UpperLeft;
        _guiStyle.normal.textColor = Color.white;
        _guiStyle.fontSize = 12;

        // GlobalSaveData.add_OnCurrentCampaignSwitch((Action)OnCurrentCampaignSwitch);

        // LogMessage($"resSet test Alfred: {Strings.Alfred}");
        // LogMessage($"resSet test Culture: {Strings.Culture?.Name}");
        // var resSet = Config.Strings.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, false, true);
        // if (resSet != null)
        // {
        //     var dict = new SortedDictionary<string, string>();
        //     LogMessage($"resSet: ");
        //     var defines = "";
        //     var binds = "";
        //     foreach (DictionaryEntry dictionaryEntry in resSet)
        //     {
        //         dict.Add(dictionaryEntry.Key.ToString() ?? "", dictionaryEntry.Value?.ToString() ?? "");
        //         // defines += $"public static ConfigEntry<string> {dictionaryEntry.Key};\r\n";
        //         // binds += $"{dictionaryEntry.Key} = config.Bind(\"Strings\", \"{dictionaryEntry.Key}\", \"{dictionaryEntry.Value}\", \"\");\r\n";
        //         
        //         // LogMessage($"resSet: {dictionaryEntry.Key}, {dictionaryEntry.Value}");
        //     }
        //
        //     foreach (var dictionaryEntry in dict)
        //     {
        //         defines += $"public static ConfigEntry<string> {dictionaryEntry.Key};\r\n";
        //         binds += $"{dictionaryEntry.Key} = config.Bind(\"Strings\", \"{dictionaryEntry.Key}\", \"{dictionaryEntry.Value}\", \"\");\r\n";
        //
        //     }
        //     // LogMessage($"defines: {defines}");
        //     // LogMessage($"binds: {binds}");
        // }
    }

    private void DetectHotkeys()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            bool changed = false;
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                LogTrace("UpArrow");
                Config.SaveDataExtras.ZoomScale.Value += 0.01f;
                changed = true;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                LogTrace("DownArrow");
                Config.SaveDataExtras.ZoomScale.Value -= 0.01f;
                changed = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                LogTrace("LeftArrow");
                Config.SaveDataExtras.MapOffset.Value -= 1.0f;
                changed = true;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                LogTrace("RightArrow");
                Config.SaveDataExtras.MapOffset.Value += 1.0f;
                changed = true;
            }

            if (changed)
            {
                if (PlayerOverlays.P1.gameObject.activeSelf)
                    foreach (var pair in PlayerOverlays.P1.TopMapView.MapMarkers)
                    {
                        pair.Value.UpdatePosition(true);
                    }

                if (PlayerOverlays.P2.gameObject.activeSelf)
                    foreach (var pair in PlayerOverlays.P2.TopMapView.MapMarkers)
                    {
                        pair.Value.UpdatePosition(true);
                    }
            }
        }
    }

    private void Update()
    {
        DetectHotkeys();

        if (Input.GetKeyDown(KeyCode.M))
        {
            LogDebug("M key pressed.");
            EnabledOverlayMap = !EnabledOverlayMap;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            LogDebug("F key pressed.");
            ShowFullMap = !ShowFullMap;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
        {
            LogDebug("T key pressed.");
            _showFontDebugPanel = !_showFontDebugPanel;
            if (_fontDebugPanel != null)
            {
                _fontDebugPanel.SetVisible(_showFontDebugPanel);
            }
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            LogDebug($"Try to reload game.");

            Managers.Inst.game.Reload();
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            LogDebug($"Try to save game.");

            Managers.Inst.game.TriggerSave();
        }

        _timeSinceLastGuiUpdate += Time.deltaTime;

        if (_timeSinceLastGuiUpdate > (1.0 / Config.Global.GuiUpdatesPerSecond))
        {
            _timeSinceLastGuiUpdate = 0;

            if (!IsPlaying()) return;

            if (EnabledOverlayMap)
            {
                UpdateMinimapMarkList();
                UpdateStatsInfo();
            }
        }
    }

    public static bool IsPlaying()
    {
        var game = Managers.Inst?.game;
        if (game == null) return false;
        return game.state is Game.State.Playing or Game.State.NetworkClientPlaying;
    }

    private void OnGUI()
    {
        if (!IsPlaying()) return;

        if (EnabledOverlayMap)
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
        LogDebug("host_OnClientCaughtUp.");

        // OnGameStart();
    }

    public void OnP2StateChanged(Game game, bool joined)
    {
        PlayerOverlays.P1?.OnP2StateChanged(game, joined);
        PlayerOverlays.P2?.OnP2StateChanged(game, joined);
    }

    public void OnGameInit(Game game)
    {
        LogTrace("OverlayMapHolder.OnGameInit");

#if IL2CPP
        game.run.GetOverrideMethodDelegate<IHagletCallable, System.Action<Il2CppSystem.Action<int, int>>>("add_onGoto").Invoke(new System.Action<int, int>(OnGameGoto));
#else
        game.run.onGoto += OnGameGoto;
#endif
    }

    public void OnProgramDirectorRun()
    {
        LogTrace("OverlayMapHolder.OnProgramDirectorRun");

#if IL2CPP
        ProgramDirector.run.GetOverrideMethodDelegate<IHagletCallable, System.Action<Il2CppSystem.Action<int, int>>>("add_onGoto").Invoke(new System.Action<int, int>(OnProgramDirectorGoto));
#else
        ProgramDirector.run.onGoto += OnProgramDirectorGoto;
#endif
    }

    public void OnGameStart()
    {
        LogDebug("OverlayMapHolder.OnGameStart");

        _gameLayer = GameObject.FindGameObjectWithTag(Tags.GameLayer);
        _persephoneCage = UnityEngine.Object.FindAnyObjectByType<PersephoneCage>();
        _cachePrefabID.CachePrefabIDs();

        Config.SaveDataExtras.Init();

        // 在场景切换后重新加载GUI样式
        NeedToReloadGuiBoxStyle = true;
    }

    public void OnGameEnd()
    {
        LogDebug("OverlayMapHolder.OnGameEnd");


    }

    public void OnGameSaved()
    {
        LogTrace("OverlayMapHolder.OnGameSaved");

        Config.SaveDataExtras.Save();
    }

    public void ReloadGuiStyle()
    {
        _guiBoxStyle = new GUIStyle(GUI.skin.box);
        var bgImageFile = Path.Combine(AssetsDir, Config.GuiStyle.TopMap.BackgroundImageFile);
        LogDebug($"ReloadGuiStyle: \n" +
                 $"bgImageFile={bgImageFile}\n" +
                 $"BackgroundImageArea={Config.GuiStyle.TopMap.BackgroundImageArea.Value}\n" +
                 $"BackgroundImageBorder={Config.GuiStyle.TopMap.BackgroundImageBorder.Value}");
        if (File.Exists(bgImageFile))
        {
            byte[] imageData = File.ReadAllBytes(bgImageFile);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(imageData);

            var imageArea = (RectInt)Config.GuiStyle.TopMap.BackgroundImageArea;
            imageArea.y = texture.height - imageArea.y - imageArea.height;
            Texture2D subTexture = new Texture2D(imageArea.width, imageArea.height, TextureFormat.RGBA32, false);
            Color[] pixels = texture.GetPixels(imageArea.x, imageArea.y, imageArea.width, imageArea.height);
            if (pixels != null)
            {
                subTexture.SetPixels(pixels);
                subTexture.Apply();

                //  _guiBoxStyle.normal.background = subTexture;
                _guiBoxStyle.normal.background = MakeColoredTexture(subTexture, Config.GuiStyle.TopMap.BackgroundColor);
                _guiBoxStyle.stretchWidth = false;
                _guiBoxStyle.stretchHeight = false;
                _guiBoxStyle.border = Config.GuiStyle.TopMap.BackgroundImageBorder;
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

    public static Texture2D MakeColoredTexture(Texture2D source, Color color)
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
        LogDebug($"OnCurrentCampaignSwitch: {GlobalSaveData.loaded.currentCampaign}");

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

        _minimapMarkList.Clear();
        var poiList = new System.Collections.Generic.List<MarkInfo>();
        var leftWalls = new System.Collections.Generic.List<WallPoint>();
        var rightWalls = new System.Collections.Generic.List<WallPoint>();

        foreach (var obj in kingdom.AllPortals)
        {
            if (obj.type == Portal.Type.Regular)
                poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.Portal.Color, Config.MarkerStyle.Portal.Sign, Config.Strings.Portal));
            else if (obj.type == Portal.Type.Cliff)
                poiList.Add(new MarkInfo(obj.transform.position.x, obj.state switch{ Portal.State.Destroyed => Config.MarkerStyle.PortalCliff.Destroyed.Color, Portal.State.Rebuilding => Config.MarkerStyle.PortalCliff.Rebuilding.Color, _=> Config.MarkerStyle.PortalCliff.Color }, Config.MarkerStyle.PortalCliff.Sign, Config.Strings.PortalCliff));
            else if (obj.type == Portal.Type.Dock)
                poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.PortalDock.Color, Config.MarkerStyle.PortalDock.Sign, Config.Strings.PortalDock));
        }

        var beach = _gameLayer.GetComponentInChildren<Beach>();
        if (beach != null)
            poiList.Add(new MarkInfo(beach.transform.position.x, Config.MarkerStyle.Beach.Color, Config.MarkerStyle.Beach.Sign, Config.Strings.Beach));

        foreach (var beggarCamp in kingdom.BeggarCamps)
        {
            int count = 0;
            foreach (var beggar in beggarCamp._beggars)
            {
                if (beggar != null && beggar.isActiveAndEnabled)
                    count++;
            }
            poiList.Add(new MarkInfo(beggarCamp.transform.position.x, Config.MarkerStyle.BeggarCamp.Color, Config.MarkerStyle.BeggarCamp.Sign, Config.Strings.BeggarCamp, count));
        }

        foreach (var beggar in kingdom.Beggars)
        {
            if (beggar == null) continue;

            if (beggar.hasFoundBaker)
            {
                poiList.Add(new MarkInfo(beggar.transform.position.x, Config.MarkerStyle.Beggar.Color, Config.MarkerStyle.Beggar.Sign, Config.Strings.Beggar, 0, MarkRow.Movable));
            }
        }

        foreach (var player in new System.Collections.Generic.List<Player>{ kingdom.playerOne, kingdom.playerTwo })
        {
            if (player == null) continue;
            if (player.isActiveAndEnabled == false) continue;
            var mover = player.mover;
            if (mover == null) continue;

            poiList.Add(new MarkInfo(mover.transform.position.x, Config.MarkerStyle.Player.Color, Config.MarkerStyle.Player.Sign, player.playerId == 0 ? Config.Strings.P1 : Config.Strings.P2, 0, MarkRow.Movable));
            float l = mover.transform.position.x - 12;
            float r = mover.transform.position.x + 12;
            Config.SaveDataExtras.ExploredLeft.Value = Math.Min(Config.SaveDataExtras.ExploredLeft, l);
            Config.SaveDataExtras.ExploredRight.Value = Math.Max(Config.SaveDataExtras.ExploredRight, r);
        }

        if (kingdom.teleExitP1)
            poiList.Add(new MarkInfo(kingdom.teleExitP1.transform.position.x, Config.MarkerStyle.TeleExitP1.Color, Config.MarkerStyle.TeleExitP1.Sign, Config.Strings.TeleExitP1, 0, MarkRow.Movable));

        if (kingdom.teleExitP2)
            poiList.Add(new MarkInfo(kingdom.teleExitP2.transform.position.x, Config.MarkerStyle.TeleExitP2.Color, Config.MarkerStyle.TeleExitP2.Sign, Config.Strings.TeleExitP2, 0, MarkRow.Movable));

        var deers = GameExtensions.FindObjectsWithTagOfType<Deer>(Tags.Wildlife);
        foreach (var deer in deers)
        {
            if (!deer._damageable.isDead)
                poiList.Add(new MarkInfo(deer.transform.position.x, deer._fsm.Current == 5 ? Config.MarkerStyle.DeerFollowing.Color : Config.MarkerStyle.Deer.Color, Config.MarkerStyle.Deer.Sign, Config.Strings.Deer, 0, MarkRow.Movable));
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
                    poiList.Add(new MarkInfo(leftBosses[0], Config.MarkerStyle.Boss.Color, Config.MarkerStyle.Boss.Sign, Config.Strings.Boss, leftBosses.Count, MarkRow.Movable));
                    if (leftEnemies[0] - leftBosses[0] < 6)
                        drawEnemies = false;
                }

                if (drawEnemies)
                    poiList.Add(new MarkInfo(leftEnemies[0], Config.MarkerStyle.Enemy.Color, Config.MarkerStyle.Enemy.Sign, Config.Strings.Enemy, leftEnemies.Count, MarkRow.Movable));
            }

            if (rightEnemies.Count > 0)
            {
                var drawEnemies = true;
                rightEnemies.Sort((a, b) => a.CompareTo(b));

                if (rightBosses.Count > 0)
                {
                    rightBosses.Sort((a, b) => a.CompareTo(b));
                    poiList.Add(new MarkInfo(rightBosses[0], Config.MarkerStyle.Boss.Color, Config.MarkerStyle.Boss.Sign, Config.Strings.Boss, rightBosses.Count, MarkRow.Movable));
                    if (rightBosses[0] - rightEnemies[0] < 6)
                        drawEnemies = false;
                }

                if (drawEnemies)
                    poiList.Add(new MarkInfo(rightEnemies[0], Config.MarkerStyle.Enemy.Color, Config.MarkerStyle.Enemy.Sign, Config.Strings.Enemy, rightEnemies.Count, MarkRow.Movable));
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
            var color = isLocked ? Config.MarkerStyle.Castle.Locked.Color : Config.MarkerStyle.Castle.Color;
            poiList.Add(new MarkInfo(castle.transform.position.x, color, Config.MarkerStyle.Castle.Sign, Config.Strings.Castle, price));

            leftWalls.Add(new WallPoint(castle.transform.position, Config.MarkerStyle.WallLine.Color));
            rightWalls.Add(new WallPoint(castle.transform.position, Config.MarkerStyle.WallLine.Color));
        }

        var campfire = kingdom.campfire;
        if (campfire !=  null)
        {
            poiList.Add(new MarkInfo(campfire.transform.position.x, Config.MarkerStyle.Campfire.Color, Config.MarkerStyle.Campfire.Sign, Config.Strings.Campfire));
        }

        var chestList = _gameLayer.GetComponentsInChildren<Chest>();
        foreach (var obj in chestList)
        {
            if (obj.currencyAmount == 0) continue;

            if (obj.currencyType == CurrencyType.Gems)
                poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.GemChest.Color, Config.MarkerStyle.GemChest.Sign, Config.Strings.GemChest, obj.currencyAmount));
            else
                poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.Chest.Color, Config.MarkerStyle.Chest.Sign, Config.Strings.Chest, obj.currencyAmount));
        }

        foreach (var obj in kingdom._walls)
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.Wall.Color, Config.MarkerStyle.Wall.Sign, ""));
            if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                leftWalls.Add(new WallPoint(obj.transform.position, Config.MarkerStyle.WallLine.Color));
            else
                rightWalls.Add(new WallPoint(obj.transform.position, Config.MarkerStyle.WallLine.Color));
        }

        var shopForge = GameObject.FindGameObjectWithTag(Tags.ShopForge);
        if (shopForge != null)
        {
            poiList.Add(new MarkInfo(shopForge.transform.position.x, Config.MarkerStyle.ShopForge.Color, Config.MarkerStyle.ShopForge.Sign, Config.Strings.ShopForge));
        }

        var citizenHouses = GameObject.FindGameObjectsWithTag(Tags.CitizenHouse);
        foreach (var obj in citizenHouses)
        {
            var citizenHouse = obj.GetComponent<CitizenHousePayable>();
            if (citizenHouse != null)
            {
                poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.CitizenHouse.Color, Config.MarkerStyle.CitizenHouse.Sign, Config.Strings.CitizenHouse, citizenHouse._numberOfAvailableCitizens));
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
            var color = isLocked ? Config.MarkerStyle.Lighthouse.Locked.Color : Config.MarkerStyle.Lighthouse.Color;
            poiList.Add(new MarkInfo(obj.transform.position.x, color, Config.MarkerStyle.Lighthouse.Sign, Config.Strings.Lighthouse, price));
        }

        var wallWreckList = GameObject.FindGameObjectsWithTag(Tags.WallWreck);
        foreach (var obj in wallWreckList)
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.Wall.Wrecked.Color, Config.MarkerStyle.Wall.Sign, ""));
            if (kingdom.GetBorderSideForPosition(obj.transform.position.x) == Side.Left)
                leftWalls.Add(new WallPoint(obj.transform.position, Config.MarkerStyle.WallLine.Wrecked.Color));
            else
                rightWalls.Add(new WallPoint(obj.transform.position, Config.MarkerStyle.WallLine.Wrecked.Color));
        }

        var wallFoundation = GameObject.FindGameObjectsWithTag(Tags.WallFoundation);
        foreach (var obj in wallFoundation)
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.WallFoundation.Color, Config.MarkerStyle.WallFoundation.Sign, ""));
        }

        var riverList = _gameLayer.GetComponentsInChildren<River>();
        foreach (var obj in riverList)
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.River.Color, Config.MarkerStyle.River.Sign, ""));
        }

        foreach (var obj in Managers.Inst.world._berryBushes)
        {
            if (obj.paid)
                poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.BerryBushPaid.Color, Config.MarkerStyle.BerryBushPaid.Sign, ""));
            else
                poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.BerryBush.Color, Config.MarkerStyle.BerryBush.Sign, ""));
        }

        var payableGemChest = GameExtensions.GetPayableOfType<PayableGemChest>();
        if (payableGemChest != null)
        {
            var gemsCount = payableGemChest.infiniteGems ? payableGemChest.guardRef.Price : payableGemChest.gemsStored;
            poiList.Add(new MarkInfo(payableGemChest.transform.position.x, Config.MarkerStyle.GemMerchant.Color, Config.MarkerStyle.GemMerchant.Sign, Config.Strings.GemMerchant, gemsCount));
        }

        var dogSpawn = GameExtensions.GetPayableBlockerOfType<DogSpawn>();
        if (dogSpawn != null && !dogSpawn._dogFreed)
            poiList.Add(new MarkInfo(dogSpawn.transform.position.x, Config.MarkerStyle.DogSpawn.Color, Config.MarkerStyle.DogSpawn.Sign, Config.Strings.DogSpawn));

        var boarSpawn = world.boarSpawnGroup;
        if (boarSpawn != null)
        {
            poiList.Add(new MarkInfo(boarSpawn.transform.position.x, Config.MarkerStyle.BoarSpawn.Color, Config.MarkerStyle.BoarSpawn.Sign,
                Config.Strings.BoarSpawn, boarSpawn._spawnedBoar ? 0 : 1));
        }

        var caveHelper = Managers.Inst.caveHelper;
        if (caveHelper != null && caveHelper.CurrentlyBombingPortal != null)
        {
            var bomb = caveHelper.Getbomb(caveHelper.CurrentlyBombingPortal.Side);
            if (bomb != null)
            {
                poiList.Add(new MarkInfo(bomb.transform.position.x, Config.MarkerStyle.Bomb.Color, Config.MarkerStyle.Bomb.Sign, Config.Strings.Bomb, 0, MarkRow.Movable));
            }
        }

        foreach (var obj in kingdom.GetFarmHouses())
        {
            poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.Farmhouse.Color, Config.MarkerStyle.Farmhouse.Sign, Config.Strings.Farmhouse));
        }

        var steedNames = new System.Collections.Generic.Dictionary<SteedType, string>
        {
            { SteedType.INVALID,               Config.Strings.Invalid },
            { SteedType.Bear,                  Config.Strings.Bear },
            { SteedType.P1Griffin,             Config.Strings.Griffin },
            { SteedType.Lizard,                Config.Strings.Lizard },
            { SteedType.Reindeer,              Config.Strings.Reindeer },
            { SteedType.Spookyhorse,           Config.Strings.Spookyhorse },
            { SteedType.Stag,                  Config.Strings.Stag },
            { SteedType.Unicorn,               Config.Strings.Unicorn },
            { SteedType.P1Warhorse,            Config.Strings.Warhorse },
            { SteedType.P1Default,             Config.Strings.DefaultSteed },
            { SteedType.P2Default,             Config.Strings.DefaultSteed },
            { SteedType.HorseStamina,          Config.Strings.HorseStamina },
            { SteedType.HorseBurst,            Config.Strings.HorseBurst },
            { SteedType.HorseFast,             Config.Strings.HorseFast },
            { SteedType.P1Wolf,                Config.Strings.Wolf },
            { SteedType.Trap,                  Config.Strings.Trap },
            { SteedType.Barrier,               Config.Strings.Barrier },
            { SteedType.Bloodstained,          Config.Strings.Bloodstained },
            { SteedType.P2Wolf,                Config.Strings.Wolf },
            { SteedType.P2Griffin,             Config.Strings.Griffin },
            { SteedType.P2Warhorse,            Config.Strings.Warhorse },
            { SteedType.P2Stag,                Config.Strings.Stag },
            { SteedType.Gullinbursti,          Config.Strings.Gullinbursti },
            { SteedType.Sleipnir,              Config.Strings.Sleipnir },
            { SteedType.Reindeer_Norselands,   Config.Strings.Reindeer },
            { SteedType.CatCart,               Config.Strings.CatCart },
            { SteedType.Kelpie,                Config.Strings.Kelpie },
            { SteedType.DayNight,              Config.Strings.DayNight },
            { SteedType.P2Kelpie,              Config.Strings.Kelpie },
            { SteedType.P2Reindeer_Norselands, Config.Strings.Reindeer },
            { SteedType.Hippocampus,           Config.Strings.Hippocampus },
            { SteedType.Cerberus,              Config.Strings.Cerberus },
            { SteedType.Spider,                Config.Strings.Spider },
            { SteedType.TheChariotDay,         Config.Strings.TheChariotDay },
            { SteedType.TheChariotNight,       Config.Strings.TheChariotNight },
            { SteedType.Pegasus,               Config.Strings.Pegasus },
            { SteedType.Donkey,                Config.Strings.Donkey },
            { SteedType.MolossianHound,        Config.Strings.MolossianHound },
            { SteedType.Chimera,               Config.Strings.Chimera },
            { SteedType.Total,                 Config.Strings.Total }
        };

        foreach (var obj in kingdom.spawnedSteeds)
        {
            if (obj.CurrentMode != SteedMode.Player)
            {
                if (!steedNames.TryGetValue(obj.steedType, out var steedName))
                    steedName = obj.steedType.ToString();
                poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.Steeds.Color, Config.MarkerStyle.Steeds.Sign, steedName, obj.Price));
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
                poiList.Add(new MarkInfo(obj.transform.position.x, Config.MarkerStyle.SteedSpawns.Color, Config.MarkerStyle.SteedSpawns.Sign, info, obj.Price));
        }

        string LogUnknownHermitType(Hermit.HermitType hermitType)
        {
            LogWarning($"Unknown hermit type: {hermitType}");
            return hermitType.ToString();
        }

        var cabinList = GameExtensions.GetPayablesOfType<Cabin>();
        foreach (var obj in cabinList)
        {
            var info = obj.hermitType switch
            {
                Hermit.HermitType.Horse => Config.Strings.HermitHorse,
                Hermit.HermitType.Horn => Config.Strings.HermitHorn,
                Hermit.HermitType.Ballista => Config.Strings.HermitBallista,
                Hermit.HermitType.Baker => Config.Strings.HermitBaker,
                Hermit.HermitType.Knight => Config.Strings.HermitKnight,
                Hermit.HermitType.Persephone => Config.Strings.HermitPersephone,
                Hermit.HermitType.Fire => Config.Strings.HermitFire,
                _ => LogUnknownHermitType(obj.hermitType)
            };

            var color = obj.canPay ? Config.MarkerStyle.HermitCabins.Locked.Color : Config.MarkerStyle.HermitCabins.Unlocked.Color;
            var price = obj.canPay ? obj.Price : 0;
            poiList.Add(new MarkInfo(obj.transform.position.x, color, Config.MarkerStyle.HermitCabins.Sign, info, price));
        }

        if (_persephoneCage)
        {
            var color = PersephoneCage.State.IsPersephoneLocked(_persephoneCage._fsm.Current) ? Config.MarkerStyle.PersephoneCage.Locked.Color : Config.MarkerStyle.PersephoneCage.Unlocked.Color;
            poiList.Add(new MarkInfo(_persephoneCage.transform.position.x, color, Config.MarkerStyle.PersephoneCage.Sign, Config.Strings.HermitPersephone, 0));
        }

        var statueList = GameExtensions.GetPayablesOfType<Statue>();
        foreach (var obj in statueList)
        {
            var info = obj.deity switch
            {
                Statue.Deity.Archer => Config.Strings.StatueArcher,
                Statue.Deity.Worker => Config.Strings.StatueWorker,
                Statue.Deity.Knight => Config.Strings.StatueKnight,
                Statue.Deity.Farmer => Config.Strings.StatueFarmer,
                Statue.Deity.Time => Config.Strings.StatueTime,
                Statue.Deity.Pike => Config.Strings.StatuePike,
                _ => ""
            };

            bool isLocked = obj.deityStatus != Statue.DeityStatus.Activated;
            var color = isLocked ? Config.MarkerStyle.Statues.Locked.Color : Config.MarkerStyle.Statues.Unlocked.Color;
            var price = isLocked ? obj.Price : 0;
            poiList.Add(new MarkInfo(obj.transform.position.x, color, Config.MarkerStyle.Statues.Sign, info, price));
        }

        var timeStatue = kingdom.timeStatue;
        if (timeStatue)
            poiList.Add(new MarkInfo(timeStatue.transform.position.x, Config.MarkerStyle.StatueTime.Color, Config.MarkerStyle.StatueTime.Sign, Config.Strings.StatueTime, timeStatue._daysRemaining));

        // var wharf = kingdom.wharf;
        var boat = kingdom.boat;
        if (boat)
            poiList.Add(new MarkInfo(boat.transform.position.x, Config.MarkerStyle.Boat.Color, Config.MarkerStyle.Boat.Sign, Config.Strings.Boat));
        else
        {
            var wreck = kingdom.wreckPlaceholder;
            if (wreck)
                poiList.Add(new MarkInfo(wreck.transform.position.x, Config.MarkerStyle.Boat.Wrecked.Color, Config.MarkerStyle.Boat.Sign, Config.Strings.BoatWreck));
        }

        var summonBell = kingdom.boatSailPosition?.GetComponentInChildren<BoatSummoningBell>();
        if (summonBell)
            poiList.Add(new MarkInfo(summonBell.transform.position.x, Config.MarkerStyle.SummonBell.Color, Config.MarkerStyle.SummonBell.Sign, Config.Strings.SummonBell));

        var hephaestusForge = _gameLayer.GetComponentInChildren<HephaestusForge>();
        if (hephaestusForge)
            poiList.Add(new MarkInfo(hephaestusForge.transform.position.x, Config.MarkerStyle.HephaestusForge.Color, Config.MarkerStyle.HephaestusForge.Sign, Config.Strings.HephaestusForge));

        foreach (var obj in payables.AllPayables)
        {
            if (obj == null) continue;
            var go = obj.gameObject;
            if (go == null) continue;
            var prefab = go.GetComponent<PrefabID>();
            if (prefab == null) continue;

            if (prefab.prefabID == (int)GamePrefabID.Quarry_undeveloped)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.Quarry.Locked.Color, Config.MarkerStyle.Quarry.Sign, Config.Strings.Quarry, obj.Price));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Mine_undeveloped)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.Mine.Locked.Color, Config.MarkerStyle.Mine.Sign, Config.Strings.Mine, obj.Price));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Lighthouse_undeveloped)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.Lighthouse.Unpaid.Color, Config.MarkerStyle.Lighthouse.Sign, Config.Strings.Lighthouse, obj.Price));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Cliff_Portal)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.PortalCliff.Color, Config.MarkerStyle.PortalCliff.Sign, Config.Strings.PortalCliff, obj.Price));
            }
            else
            {
                var unlockNewRulerStatue = go.GetComponent<UnlockNewRulerStatue>();
                if (unlockNewRulerStatue != null)
                {
                    var color = unlockNewRulerStatue.status switch
                    {
                        UnlockNewRulerStatue.Status.Locked => Config.MarkerStyle.RulerSpawns.Locked.Color,
                        UnlockNewRulerStatue.Status.WaitingForArcher => Config.MarkerStyle.RulerSpawns.Building.Color,
                        _ => Config.MarkerStyle.RulerSpawns.Unlocked.Color
                    };
                    if (color != Config.MarkerStyle.RulerSpawns.Unlocked.Color)
                    {
                        var markName = unlockNewRulerStatue.rulerToUnlock switch
                        {
                            MonarchType.King => Config.Strings.King,
                            MonarchType.Queen => Config.Strings.Queen,
                            MonarchType.Prince => Config.Strings.Prince,
                            MonarchType.Princess => Config.Strings.Princess,
                            MonarchType.Hooded => Config.Strings.Hooded,
                            MonarchType.Zangetsu => Config.Strings.Zangetsu,
                            MonarchType.Alfred => Config.Strings.Alfred,
                            MonarchType.Gebel => Config.Strings.Gebel,
                            MonarchType.Miriam => Config.Strings.Miriam,
                            MonarchType.Total => "",
                            _ => ""
                        };
                        poiList.Add(new MarkInfo(go.transform.position.x, color, Config.MarkerStyle.RulerSpawns.Sign, markName, obj.Price));
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
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.Quarry.Unlocked.Color, Config.MarkerStyle.Quarry.Sign, Config.Strings.Quarry));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Mine)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.Mine.Unlocked.Color, Config.MarkerStyle.Mine.Sign, Config.Strings.Mine));
            }
            else if (prefab.prefabID == (int)GamePrefabID.MerchantHouse)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.MerchantHouse.Color, Config.MarkerStyle.MerchantHouse.Sign, Config.Strings.MerchantHouse));
            }
            else
            {
                var thorPuzzleController = go.GetComponent<ThorPuzzleController>();
                if (thorPuzzleController != null)
                {
                    var color = thorPuzzleController.State == 0 ? Config.MarkerStyle.ThorPuzzleStatue.Locked.Color : Config.MarkerStyle.ThorPuzzleStatue.Unlocked.Color;
                    poiList.Add(new MarkInfo(thorPuzzleController.transform.position.x, color, Config.MarkerStyle.ThorPuzzleStatue.Sign, Config.Strings.ThorPuzzleStatue));
                }

                var helPuzzleController = go.GetComponent<HelPuzzleController>();
                if (helPuzzleController != null)
                {
                    var color = helPuzzleController.State == 0 ? Config.MarkerStyle.HelPuzzleStatue.Locked.Color : Config.MarkerStyle.HelPuzzleStatue.Unlocked.Color;
                    poiList.Add(new MarkInfo(helPuzzleController.transform.position.x, color, Config.MarkerStyle.HelPuzzleStatue.Sign, Config.Strings.HelPuzzleStatue));
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
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.Wall.Building.Color, Config.MarkerStyle.Wall.Sign, ""));
                if (kingdom.GetBorderSideForPosition(go.transform.position.x) == Side.Left)
                    leftWalls.Add(new WallPoint(go.transform.position, Config.MarkerStyle.WallLine.Building.Color));
                else
                    rightWalls.Add(new WallPoint(go.transform.position, Config.MarkerStyle.WallLine.Building.Color));
            }

            var prefab = go.GetComponent<PrefabID>();
            if (prefab == null) continue;
            if (prefab.prefabID == (int)GamePrefabID.Quarry)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.Quarry.Building.Color, Config.MarkerStyle.Quarry.Sign, Config.Strings.Quarry));
            }
            else if (prefab.prefabID == (int)GamePrefabID.Mine)
            {
                poiList.Add(new MarkInfo(go.transform.position.x, Config.MarkerStyle.Mine.Building.Color, Config.MarkerStyle.Mine.Sign, Config.Strings.Mine));
            }
        }

        // var mine = GameObject.Find("Mine_undeveloped(Clone)");
        // if (mine)
        // {
        //     poiList.Add(new MarkInfo(mine.transform.position, Color.red, Config.Strings.Mine));
        //     LogMessage($"mine prefabID: {mine.GetComponent<PrefabID>().prefabID}");
        // }
            
        // explored area

        float wallLeft = Managers.Inst.kingdom.GetBorderSide(Side.Left);
        float wallRight = Managers.Inst.kingdom.GetBorderSide(Side.Right);

        foreach (var poi in poiList)
        {
            if (ShowFullMap)
                poi.Visible = true;
            else if(poi.WorldPosX >= Config.SaveDataExtras.ExploredLeft && poi.WorldPosX <= Config.SaveDataExtras.ExploredRight)
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

        _minimapMarkList = poiList;

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

        _drawLineList = lineList;
    }

    private static bool IsYourSelf(int playerId, string name)
    {
        if (name == Config.Strings.P1)
        {
            if (playerId == 0 && NetworkBigBoss.HasWorldAuth)
            {
                return true;
            }
        }
        else if (name == Config.Strings.P2)
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

        foreach (var line in _drawLineList)
        {
            GuiHelper.DrawLine(line.LineStart, line.LineEnd, line.Color, 2);
        }

        foreach (var markInfo in _minimapMarkList)
        {
            if (!markInfo.Visible)
                continue;

            var markName = markInfo.Name;
            var color = markInfo.Color;
            if (markInfo.NameRow == MarkRow.Movable)
            {
                if (IsYourSelf(playerId, markName))
                {
                    markName = Config.Strings.You.Value;
                    color = Config.MarkerStyle.PlayerSelf.Color;

                    // draw self vec.x

                    // GUI.Label(new Rect(markInfo.Pos, 60, 0, 20), ((int)markInfo.WorldPosX).ToString(), guiStyle);
                }
            }

            _guiStyle.alignment = TextAnchor.UpperCenter;
            _guiStyle.normal.textColor = color;

            if (markInfo.Sign != "")
                GUI.Label(new Rect(markInfo.Pos, 8, 0, 20), markInfo.Sign, _guiStyle);

            float namePosY = markInfo.NameRow switch
            {
                MarkRow.Settled => 24,
                MarkRow.Movable => 56,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (markInfo.Name != "")
                GUI.Label(new Rect(markInfo.Pos, namePosY, 0, 20), markName, _guiStyle);

            if (markInfo.Count != 0)
                GUI.Label(new Rect(markInfo.Pos, namePosY + 16, 0, 20), markInfo.Count.ToString(), _guiStyle);

            // if (markInfo.Name != "")
            // GUI.Label(new Rect(markInfo.Pos, namePosY + 36, 0, 20), ((int)markInfo.WorldPosX).ToString(), guiStyle);
        }
    }

    private void UpdateStatsInfo()
    {
        var kingdom = Managers.Inst.kingdom;
        if (kingdom == null) return;

        var peasantList = GameObject.FindGameObjectsWithTag(Tags.Peasant);
        _statsInfo.PeasantCount = peasantList.Length;

        var workerList = kingdom._workers;
        _statsInfo.WorkerCount = workerList.Count;

        var archerList = kingdom._archers;
        _statsInfo.ArcherCount = archerList.Count;

        var farmerList = kingdom.Farmers;
        _statsInfo.FarmerCount = farmerList.Count;

        var farmhouseList = kingdom.GetFarmHouses();
        int maxFarmlands = 0;
        foreach (var obj in farmhouseList)
        {
            maxFarmlands += obj.CurrentMaxFarmlands();
        }
        _statsInfo.MaxFarmlands = maxFarmlands;
    }

    private void DrawStatsInfo(int playerId)
    {
        _guiStyle.normal.textColor = Config.MarkerStyle.StatsInfo.Color;
        _guiStyle.alignment = TextAnchor.UpperLeft;

        float boxTop = 160;
        if (Managers.COOP_ENABLED)
            boxTop = 160 - 56;

        var kingdom = Managers.Inst.kingdom;
        var boxRect = new Rect(5, boxTop, 120, 146);
        GUI.Box(boxRect, "", _guiBoxStyle);

        var infoLines = new List<string>();
        infoLines.Add(Config.Strings.Peasant + ": " + _statsInfo.PeasantCount);
        infoLines.Add(Config.Strings.Worker + ": " + _statsInfo.WorkerCount);
        infoLines.Add($"{Config.Strings.Archer.Value}: {_statsInfo.ArcherCount} ({GameExtensions.GetArcherCount(GameExtensions.ArcherType.Free)}|{GameExtensions.GetArcherCount(GameExtensions.ArcherType.GuardSlot)}|{GameExtensions.GetArcherCount(GameExtensions.ArcherType.KnightSoldier)})");
        infoLines.Add(Config.Strings.Pikeman + ": " + kingdom.Pikemen.Count);
        infoLines.Add($"{Config.Strings.Knight.Value}: {kingdom.Knights.Count} ({GameExtensions.GetKnightCount(true)})");
        infoLines.Add(Config.Strings.Farmer + ": " + _statsInfo.FarmerCount);
        infoLines.Add(Config.Strings.Farmlands + ": " + _statsInfo.MaxFarmlands);
        infoLines.Add("ZoomScale" + ": " + Config.SaveDataExtras.ZoomScale.Value);
        infoLines.Add("MapOffset" + ": " + Config.SaveDataExtras.MapOffset.Value);
        infoLines.Add("BorderSide" + ": " + kingdom.GetBorderSide(Side.Left) + ", " + kingdom.GetBorderSide(Side.Right));
        infoLines.Add("BorderSideIntact" + ": " + kingdom.GetBorderSideIntact(Side.Left) + ", " + kingdom.GetBorderSideIntact(Side.Right));
        // infoLines.Add("outerWall" + ": " + kingdom.outerWall?.left?.transform?.position.x + ", " + kingdom.outerWall?.right?.transform?.position.x);
        // infoLines.Add("intactWall" + ": " + kingdom.intactWall?.left?.transform?.position.x + ", " + kingdom.intactWall?.right?.transform?.position.x);

        for (int i = 0; i < infoLines.Count; i++)
        {
            GUI.Label(new Rect(14, boxTop + 6 + 20 * i, 120, 20), infoLines[i], _guiStyle);
        }
    }

    private void DrawExtraInfo(int playerId)
    {
        _guiStyle.normal.textColor = Config.MarkerStyle.ExtraInfo.Color;
        _guiStyle.alignment = TextAnchor.UpperLeft;

        var left = Screen.width / 2 - 20;
        var top = 136;
        if (Managers.COOP_ENABLED)
            top = 136 - 56;

        GUI.Label(new Rect(14, top, 60, 20),  Config.Strings.Land + ": " + (Managers.Inst.game.currentLand + 1), _guiStyle);
        GUI.Label(new Rect(14 + 60, top, 60, 20), Config.Strings.Days + ": " + (Managers.Inst.director.CurrentDayForSpawning), _guiStyle);

        float currentTime = Managers.Inst.director.currentTime;
        var currentHour = Math.Truncate(currentTime);
        var currentMints = Math.Truncate((currentTime - currentHour) * 60);
        GUI.Label(new Rect(left, top + 22, 40, 20), $"{currentHour:00.}:{currentMints:00.}", _guiStyle);

        var player = Managers.Inst.kingdom.GetPlayer(playerId);
        if (player != null)
        {
            GUI.Label(new Rect(Screen.width - 126, 136 + 22, 60, 20), Config.Strings.Gems + ": " + player.gems, _guiStyle);
            GUI.Label(new Rect(Screen.width - 66, 136 + 22, 60, 20), Config.Strings.Coins + ": " + player.coins, _guiStyle);
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

    public class MarkInfo
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

    public enum PlayerId
    {
        P1,
        P2
    }

    private static string GetBepInExDir()
    {
        var baseDir = Assembly.GetExecutingAssembly().Location;
        var bepInExDir = Directory.GetParent(baseDir)?.Parent?.Parent?.FullName;
        if (bepInExDir == null || !bepInExDir.EndsWith("BepInEx"))
            throw new DirectoryNotFoundException("BepInEx directory not found. The plugin was placed in the wrong directory.");

        return bepInExDir;
    }

    public static void LogDebug(string message,
        [System.Runtime.CompilerServices.CallerMemberName]
        string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath]
        string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int sourceLineNumber = 0)
    {
        _log.LogDebug($"[{Path.GetFileName(sourceFilePath)}][{sourceLineNumber.ToString("0000")}][{memberName}] {message}");
    }

    public static void LogTrace(string message,
        [System.Runtime.CompilerServices.CallerMemberName]
        string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath]
        string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int sourceLineNumber = 0)
    {
        _log.LogInfo($"[{Path.GetFileName(sourceFilePath)}][{sourceLineNumber.ToString("0000")}][{memberName}] {message}");
    }

    public static void LogMessage(string message,
        [System.Runtime.CompilerServices.CallerMemberName]
        string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath]
        string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int sourceLineNumber = 0)
    {
        _log.LogMessage($"[{Path.GetFileName(sourceFilePath)}][{sourceLineNumber.ToString("0000")}][{memberName}] {message}");
    }

    public static void LogWarning(string message,
        [System.Runtime.CompilerServices.CallerMemberName]
        string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath]
        string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int sourceLineNumber = 0)
    {
        _log.LogWarning($"[{Path.GetFileName(sourceFilePath)}][{sourceLineNumber.ToString("0000")}][{memberName}] {message}");
    }

    public static void LogError(string message,
        [System.Runtime.CompilerServices.CallerMemberName]
        string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath]
        string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber]
        int sourceLineNumber = 0)
    {
        _log.LogError($"[{Path.GetFileName(sourceFilePath)}][{sourceLineNumber.ToString("0000")}][{memberName}] {message}");
    }
}

public static class EnumUtil
{
    public static System.Collections.Generic.IEnumerable<T> GetValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }
}