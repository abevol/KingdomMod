using System;
using BepInEx.Logging;
using UnityEngine;
using System.IO;
using System.Reflection;
using KingdomMod.OverlayMap.Gui;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Coatsink.Common;
using KingdomMod.OverlayMap.Gui.Debugging;
using KingdomMod.OverlayMap.Gui.TopMap;
using KingdomMod.SharedLib;
using KingdomMod.Shared.Attributes;

namespace KingdomMod.OverlayMap;

[RegisterTypeInIl2Cpp]
public class OverlayMapHolder : MonoBehaviour
{
    public delegate void GameStateEventHandler(Game.State state);
    public delegate void ProgramDirectorStateEventHandler(ProgramDirector.State state);

    public static OverlayMapHolder Instance { get; private set; }
    private static ManualLogSource _log;
    private static BepInEx.Configuration.ConfigFile _config;
    public MapSwitch OverlayMapSwitch = new();
    public static bool ShowFullMap = false;
    private Canvas _canvas;
    public GuiStyle GlobalGuiStyle;
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
        LogDebug("OverlayMapHolder.Initialize");

        BepInExDir = GetBepInExDir();
        AssetsDir = Path.Combine(BepInExDir, "config", "KingdomMod.OverlayMap.Assets");

        GameObject obj = new(nameof(OverlayMapHolder));
        DontDestroyOnLoad(obj);
        obj.hideFlags = HideFlags.HideAndDontSave;
        Instance = obj.AddComponent<OverlayMapHolder>(); // Call "Awake" inside.
    }

    private void Awake()
    {
        LogDebug("OverlayMapHolder.Awake");

        Config.Global.ConfigBind(_config);
        Patchers.Patcher.PatchAll();

        CreateCanvas();
        GlobalGuiStyle = CreateGlobalGuiStyle();
        PlayerOverlays.P1 = CreatePlayerOverlay(PlayerId.P1);
        PlayerOverlays.P2 = CreatePlayerOverlay(PlayerId.P2);

        Level.OnLoaded += (System.Action<bool>)OnLevelLoaded;
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
        LogDebug("OverlayMapHolder.OnDestroy");
        Level.OnLoaded -= (System.Action<bool>)OnLevelLoaded;
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
        LogDebug("CreateFontDebugPanel");

        var debugPanelObj = new GameObject(nameof(SimpleFontDebugPanel));
        debugPanelObj.transform.SetParent(_canvas.transform, false);
        _fontDebugPanel = debugPanelObj.AddComponent<SimpleFontDebugPanel>();
        _fontDebugPanel.Initialize(_canvas);
    }

    private GuiStyle CreateGlobalGuiStyle()
    {
        LogDebug($"CreateGlobalGuiStyle");

        var guiObj = new GameObject(nameof(GuiStyle));
        guiObj.transform.SetParent(_canvas.transform, false);
        var guiComp = guiObj.AddComponent<GuiStyle>();

        return guiComp;
    }

    private PlayerOverlay CreatePlayerOverlay(PlayerId playerId)
    {
        LogDebug($"CreatePlayerOverlay, playerId: {playerId}");

        var guiObj = new GameObject(nameof(PlayerOverlay));
        guiObj.transform.SetParent(_canvas.transform, false);
        var guiComp = guiObj.AddComponent<PlayerOverlay>();
        guiComp.Init(playerId);

        return guiComp;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LogDebug($"Scene: {scene.name}, {scene.buildIndex}");

    }

    private void OnGamePlaying()
    {
    }

    private void OnGameQuitting()
    {
    }

    private void OnGameGoto(int pre, int next)
    {
        LogDebug($"OnGameGoto: pre:{pre}({(Game.State)pre}), {next}({(Game.State)next})");

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
        LogDebug($"OnProgramDirectorGoto: pre:{pre}({(ProgramDirector.State)pre}), {next}({(ProgramDirector.State)next})");

        OnProgramDirectorStateChanged?.Invoke((ProgramDirector.State)next);
    }

    private void Start()
    {
        LogDebug($"{this.GetType().Name}.Start");

        NetworkBigBoss.Instance.OnClientCaughtUp += (Action)this.OnClientCaughtUp;

    }

    private void DetectHotkeys()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            bool changed = false;
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                LogDebug("UpArrow");
                Config.SaveDataExtras.ZoomScale.Value += 0.01f;
                changed = true;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                LogDebug("DownArrow");
                Config.SaveDataExtras.ZoomScale.Value -= 0.01f;
                changed = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                LogDebug("LeftArrow");
                Config.SaveDataExtras.MapOffset.Value -= 1.0f;
                changed = true;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                LogDebug("RightArrow");
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
            OverlayMapSwitch.Toggle();

            if (OverlayMapSwitch.CurrentState == MapSwitchState.NewMap)
            {
                ForEachPlayerOverlay(playerOverlay => playerOverlay.Show());
            }
            else
            {
                ForEachPlayerOverlay(playerOverlay => playerOverlay.Hide());
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
        {
            LogDebug("F key pressed.");
            ShowFullMap = !ShowFullMap;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
        {
            LogDebug("T key pressed.");
            if (_fontDebugPanel == null)
            {
                CreateFontDebugPanel();
            }
            _showFontDebugPanel = !_showFontDebugPanel;
            _fontDebugPanel.SetVisible(_showFontDebugPanel);
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
        LogDebug("OverlayMapHolder.OnGameInit");

#if IL2CPP
        game.run.GetOverrideMethodDelegate<IHagletCallable, System.Action<Il2CppSystem.Action<int, int>>>("add_onGoto").Invoke(new System.Action<int, int>(OnGameGoto));
#else
        game.run.onGoto += OnGameGoto;
#endif
    }

    public void OnProgramDirectorRun()
    {
        LogDebug("OverlayMapHolder.OnProgramDirectorRun");

#if IL2CPP
        ProgramDirector.run.GetOverrideMethodDelegate<IHagletCallable, System.Action<Il2CppSystem.Action<int, int>>>("add_onGoto").Invoke(new System.Action<int, int>(OnProgramDirectorGoto));
#else
        ProgramDirector.run.onGoto += OnProgramDirectorGoto;
#endif
    }

    private void OnLevelLoaded(bool fromSave)
    {
        LogDebug($"OverlayMapHolder.OnLevelLoaded: fromSave: {fromSave}");

        Config.SaveDataExtras.Init();
    }

    public void OnGameStart()
    {
        LogDebug("OverlayMapHolder.OnGameStart");

    }

    public void OnGameEnd()
    {
        LogDebug("OverlayMapHolder.OnGameEnd");


    }

    public void OnGameSaved()
    {
        LogDebug("OverlayMapHolder.OnGameSaved");

        Config.SaveDataExtras.Save();
    }

    private void OnCurrentCampaignSwitch()
    {
        LogDebug($"OnCurrentCampaignSwitch: {GlobalSaveData.loaded.currentCampaign}");

    }

    public static Player GetLocalPlayer()
    {
        return Managers.Inst.kingdom.GetPlayer(NetworkBigBoss.HasWorldAuth ? 0 : 1);
    }

    public enum PlayerId
    {
        P1,
        P2
    }

    public enum MapSwitchState
    {
        Off,
        NewMap
    }

    public class MapSwitch
    {
        private MapSwitchState _currentState = MapSwitchState.NewMap;

        public MapSwitchState CurrentState => _currentState;

        public void Toggle()
        {
            _currentState = _currentState switch
            {
                MapSwitchState.NewMap => MapSwitchState.Off,
                MapSwitchState.Off => MapSwitchState.NewMap,
                _ => MapSwitchState.Off
            };
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

    public static void LogInfo(string message,
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

    public static void ForEachPlayerOverlay(System.Action<PlayerOverlay> action)
    {
        var p1 = Instance?.PlayerOverlays.P1;
        var p2 = Instance?.PlayerOverlays.P2;
        if (p1 != null && p1.enabled) action(p1);
        if (p2 != null && p2.enabled) action(p2);
    }

    public static void ForEachTopMapView(System.Action<TopMapView> action)
    {
        var p1 = Instance?.PlayerOverlays.P1;
        var p2 = Instance?.PlayerOverlays.P2;
        if (p1 != null && p1.enabled) action(p1.TopMapView);
        if (p2 != null && p2.enabled) action(p2.TopMapView);
    }
}
