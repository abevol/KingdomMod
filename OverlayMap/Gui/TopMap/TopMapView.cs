using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using static KingdomMod.OverlayMap.OverlayMapHolder;
using KingdomMod.Shared.Attributes;

#if IL2CPP
using Il2CppInterop.Runtime.Attributes;
#endif

namespace KingdomMod.OverlayMap.Gui.TopMap;

#if IL2CPP
[RegisterTypeInIl2Cpp]
#endif
public class TopMapView : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Image _backgroundImage;
    private float _timeSinceLastGuiUpdate;

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private Dictionary<Type, List<IMarkerResolver>> _resolvers { get; set; }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private Dictionary<IntPtr, List<IMarkerResolver>> _resolverLookup { get; set; }  // IL2CPP 指针查找

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private Dictionary<MapMarkerType, IComponentMapper> _mappers { get; set; }
    
    private Mappers.EnemyMapper _enemyMapper;

    public static float MappingScale;
    public PlayerId PlayerId;

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public MapMarker CastleMarker { get; set; }
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public List<MapMarker> PlayerMarkers { get; set; }
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public WallLineController WallController { get; private set; }
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public Dictionary<Component, MapMarker> MapMarkers { get; set; }

#if IL2CPP
    public TopMapView(IntPtr ptr) : base(ptr) { }
#endif

    public TopMapView()
    {
        LogDebug("TopMapView.Constructor");
    }

    private void Awake()
    {
        LogDebug("TopMapView.Awake");

        // 初始化 Mapper 系统
        MapperInitializer.Initialize(this);
        
        // 创建 EnemyMapper（不通过 ObjectPatcher 触发，直接管理）
        _enemyMapper = new Mappers.EnemyMapper(this);

        PlayerMarkers = new List<MapMarker>();
        WallController = new WallLineController(this);
        MapMarkers = new Dictionary<Component, MapMarker>();

        _rectTransform = this.gameObject.AddComponent<RectTransform>();
        _backgroundImage = this.gameObject.AddComponent<Image>();

        UpdateLayout();
        UpdateBackgroundImage();

        OverlayMapHolder.OnGameStateChanged += OnGameStateChanged;
        Game.OnGameStart += (System.Action)OnGameStart;
        Level.OnLoaded += (System.Action<bool>)OnLevelLoaded;
    }

    /// <summary>
    /// 设置 Resolver 字典（由 MapperInitializer 调用）
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    internal void SetResolvers(
        Dictionary<Type, List<IMarkerResolver>> resolvers,
        Dictionary<IntPtr, List<IMarkerResolver>> resolverLookup)
    {
        _resolvers = resolvers;
        _resolverLookup = resolverLookup;
    }

    /// <summary>
    /// 设置 Mapper 字典（由 MapperInitializer 调用）
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    internal void SetMappers(Dictionary<MapMarkerType, IComponentMapper> mappers)
    {
        _mappers = mappers;
    }

    /// <summary>
    /// 获取指定类型的映射器
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public IComponentMapper GetMapper(MapMarkerType type)
    {
        if (_mappers != null && _mappers.TryGetValue(type, out var mapper))
            return mapper;
        return null;
    }


    public void Init(PlayerId playerId)
    {
        PlayerId = playerId;
    }

    private void OnDestroy()
    {
        OverlayMapHolder.OnGameStateChanged -= OnGameStateChanged;
        Game.OnGameStart -= (System.Action)OnGameStart;
        Level.OnLoaded -= (System.Action<bool>)OnLevelLoaded;
    }

    private void Start()
    {
        LogDebug("TopMapView.Start");

    }

    public void UpdateLayout()
    {
        // 设置Group的尺寸和外边距

        // 设置锚点
        _rectTransform.anchorMin = new Vector2(0, 1); // 左上角
        _rectTransform.anchorMax = new Vector2(1, 1); // 右上角
        _rectTransform.pivot = new Vector2(0.5f, 1); // 以顶部中心为支点

        // 设置外边距
        float height = 150;
        if (Managers.COOP_ENABLED)
            height = 150 - 56;
        _rectTransform.offsetMin = new Vector2(5, -(5 + height)); // 左边距5，顶部边距5
        _rectTransform.offsetMax = new Vector2(-5, -5); // 右边距5，底部边距0

    }

    private void OnGameStateChanged(Game.State state)
    {
        LogDebug($"OnGameStateChanged.state changed to {state}");

        switch (state)
        {
            case Game.State.Playing:
            case Game.State.NetworkClientPlaying:
                break;
            case Game.State.Menu:
                break;
            case Game.State.Quitting:
                PlayerMarkers.Clear();
                WallController?.ClearWallNodes();
                ClearMapMarkers();
                break;
        }
    }

    public void OnP2StateChanged(Game game, bool joined)
    {
        LogDebug("TopMapView.OnP2StateChanged");

        UpdateLayout();
        UpdatePlayerMarker();
    }

    public void OnSetupPlayerId(Player player, int id)
    {
        LogDebug($"TopMapView.OnSetupPlayerId: {player.playerId}, id: {id}");

    }

    public void UpdatePlayerMarker()
    {
        foreach (var playerMarker in PlayerMarkers)
        {
            var player = playerMarker.Data.Target.Cast<Player>();
            if (IsYourSelf(player))
            {
                LogDebug($"IsYourSelf, PlayerId: {PlayerId}, player.playerId: {player.playerId}");
                playerMarker.Data.Title = Strings.You;
                playerMarker.Data.Color = MarkerStyle.PlayerSelf.Color;
            }
            else if (player.playerId == (int)PlayerId.P1)
            {
                LogDebug($"IsP1, PlayerId: {PlayerId}, player.playerId: {player.playerId}");
                playerMarker.Data.Title = Strings.P1;
                playerMarker.Data.Color = MarkerStyle.Player.Color;
            }
            else if (player.playerId == (int)PlayerId.P2)
            {
                LogDebug($"IsP2, PlayerId: {PlayerId}, player.playerId: {player.playerId}");
                playerMarker.Data.Title = Strings.P2;
                playerMarker.Data.Color = MarkerStyle.Player.Color;
            }
        }
    }

    private bool IsYourSelf(Player player)
    {
        if (player.playerId == (int)PlayerId.P1)
        {
            if (PlayerId == PlayerId.P1 && NetworkBigBoss.HasWorldAuth)
            {
                return true;
            }
        }
        else if (player.playerId == (int)PlayerId.P2)
        {
            if (PlayerId == PlayerId.P2 && (Managers.COOP_ENABLED || ProgramDirector.IsClient))
            {
                return true;
            }
        }

        return false;
    }

    public void OnGameStart()
    {
        LogDebug("TopMapView.OnGameStart");

        var clientWidth = Screen.width - 40f;
        var minLevelWidth = Managers.Inst.game.currentLevelConfig.minLevelWidth;
        MappingScale = clientWidth / minLevelWidth;
        LogDebug($"MappingScale: {MappingScale}, minLevelWidth: {minLevelWidth}");
        
        UpdatePlayerMarker();

        // 游戏开始时自动调整地图偏移以居中显示
        // 在这里调用是因为此时 SaveDataExtras 已经初始化，且所有 MapMarkers 已创建
        AutoCenterMap();
    }
    
    private void OnLevelLoaded(bool fromSave)
    {
        LogDebug($"OnLevelLoaded: fromSave: {fromSave}, MapMarkers: {MapMarkers.Count}");
    }
    
    /// <summary>
    /// 自动调整地图偏移，使 MapMarkers 在屏幕上居中显示
    /// </summary>
    public void AutoCenterMap()
    {
        // 检查 SaveDataExtras 是否已初始化
        if (SaveDataExtras.MapOffset == null)
        {
            LogWarning("AutoCenterMap: SaveDataExtras not initialized yet");
            return;
        }
        
        if (MapMarkers.Count == 0)
        {
            LogDebug("AutoCenterMap: No markers to center");
            return;
        }
        
        // 查找所有 MapMarker 的世界坐标 X 的最小值和最大值
        float minWorldX = float.MaxValue;
        float maxWorldX = float.MinValue;
        
        foreach (var pair in MapMarkers)
        {
            if (pair.Key == null || !pair.Key) continue;
            if (pair.Value == null || !pair.Value) continue;
            if (pair.Value.Data == null) continue;
            if (pair.Value.Data.Target == null || !pair.Value.Data.Target) continue;
            
            float worldX = pair.Value.Data.Target.transform.position.x;
            if (worldX < minWorldX) minWorldX = worldX;
            if (worldX > maxWorldX) maxWorldX = worldX;
        }
        
        if (minWorldX == float.MaxValue || maxWorldX == float.MinValue)
        {
            LogWarning("AutoCenterMap: No valid markers found");
            return;
        }
        
        // 计算偏移量，使最小值和最大值到屏幕中心的距离相等
        // UI坐标 = (世界坐标 + MapOffset) * MappingScale * ZoomScale
        // 要让地图居中：offset = -(minWorldX + maxWorldX) / 2
        float offset = -(minWorldX + maxWorldX) / 2f;
        
        SaveDataExtras.MapOffset.Value = offset;
        
        LogDebug($"AutoCenterMap: minWorldX={minWorldX}, maxWorldX={maxWorldX}, offset={offset}");
        
        // 强制更新所有 MapMarker 的位置
        foreach (var pair in MapMarkers)
        {
            if (pair.Value != null && pair.Value)
            {
                pair.Value.UpdatePosition(true);
            }
        }
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void OnComponentCreated(Component comp, NotifierType notifierType = NotifierType.Standard)
    {
        TryResolveAndMap(comp, notifierType);
    }

    /// <summary>
    /// 尝试使用 Resolver 识别组件类型并映射。
    /// </summary>
    /// <returns>如果成功识别并映射，返回 true；否则返回 false</returns>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private bool TryResolveAndMap(Component comp, NotifierType notifierType)
    {
        // 1. 获取组件的类型指针
        var il2cppType = comp.GetIl2CppType();
        var typePtr = il2cppType.Pointer;

        // 2. 通过指针查找对应的 Resolver 列表
        if (!_resolverLookup.TryGetValue(typePtr, out var resolvers))
            return false;

        // 3. 遍历 Resolver，尝试识别
        foreach (var resolver in resolvers)
        {
            var markerType = resolver.Resolve(comp);

            // 4. 如果识别成功，查找对应的 Mapper
            if (markerType.HasValue && _mappers.TryGetValue(markerType.Value, out var mapper))
            {
                LogDebug($"Resolved {il2cppType.Name} -> {markerType.Value}, mapping...");
                mapper.Map(comp, notifierType, resolver.ResolverType);
                return true;  // 成功识别并映射
            }
        }

        return false;  // 未能识别
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void OnComponentDestroyed(Component comp)
    {
        TryRemoveMapMarker(comp);
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void OnComponentVisibleChanged(Component comp, bool visible)
    {
        if (MapMarkers.TryGetValue(comp, out var marker))
        {
            marker.OnTargetVisibleChanged(visible);
        }
    }

    private void UpdateExploredRegion()
    {
        foreach (var playerMarker in PlayerMarkers)
        {
            var player = playerMarker.Data.Target as Player;
            if (player == null) continue;
            if (player.isActiveAndEnabled == false) continue;
            var mover = player.mover;
            if (mover == null) continue;

            float l = mover.transform.position.x - 12;
            float r = mover.transform.position.x + 12;
            SaveDataExtras.ExploredLeft.Value = Math.Min(SaveDataExtras.ExploredLeft, l);
            SaveDataExtras.ExploredRight.Value = Math.Max(SaveDataExtras.ExploredRight, r);
        }

        float wallLeft = Managers.Inst.kingdom.GetBorderSide(Side.Left);
        float wallRight = Managers.Inst.kingdom.GetBorderSide(Side.Right);

        SaveDataExtras.ExploredLeft.Value = Math.Min(SaveDataExtras.ExploredLeft, wallLeft);
        SaveDataExtras.ExploredRight.Value = Math.Max(SaveDataExtras.ExploredRight, wallRight);
    }

    private void Update()
    {
        // 每帧更新敌人组位置(轻量级操作,实现平滑移动)
        if (_enemyMapper != null)
        {
            _enemyMapper.UpdateGroupPositions();
        }

        _timeSinceLastGuiUpdate += Time.deltaTime;

        if (_timeSinceLastGuiUpdate > (1.0 / Global.GuiUpdatesPerSecond))
        {
            _timeSinceLastGuiUpdate = 0;

            if (!IsPlaying()) return;

            UpdateExploredRegion();
            
            // 更新敌人分组(重量级操作,有 0.5 秒限流)
            if (_enemyMapper != null)
            {
                _enemyMapper.UpdateEnemyGroups();
            }

            // 清理已销毁的 MapMarkers（Key 或 Target 已销毁）
            var destroyedKeys = new List<Component>();
            foreach (var pair in MapMarkers)
            {
                // 检查字典的 Key（Component）是否已销毁
                if (!pair.Key)
                {
                    destroyedKeys.Add(pair.Key);
                    continue;
                }
                
                var markerData = pair.Value.Data;
                if (markerData == null)
                {
                    destroyedKeys.Add(pair.Key);
                    continue;
                }
                
                // 检查 Target 是否已销毁
                if (!markerData.Target)
                {
                    destroyedKeys.Add(pair.Key);
                    continue;
                }
                
                var worldPosX = markerData.Target.transform.position.x;
                markerData.IsInFogOfWar = !(ShowFullMap || (worldPosX >= SaveDataExtras.ExploredLeft && worldPosX <= SaveDataExtras.ExploredRight));

                if (markerData.IsInFogOfWar)
                    continue;

                if (markerData.VisibleUpdater != null)
                    markerData.Visible = markerData.VisibleUpdater(markerData.Target);
            }
            
            // 清理已销毁的 MapMarkers
            if (destroyedKeys.Count > 0)
            {
                LogDebug($"Cleaning up {destroyedKeys.Count} destroyed MapMarkers in Update");
                foreach (var key in destroyedKeys)
                {
                    TryRemoveMapMarker(key);
                }
            }
        }
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public MapMarker TryAddMapMarker(
        Component target,
        ConfigEntryWrapper<string> color,
        ConfigEntryWrapper<string> sign,
        ConfigEntryWrapper<string> title,
        CountUpdaterFn countUpdater = null,
        ColorUpdaterFn colorUpdater = null,
        VisibleUpdaterFn visibleUpdater = null,
        MarkerRow row = MarkerRow.Settled)
    {
        try
        {
            // LogTrace($"TopMapView.TryAddMapMarker, title: {title?.Value}, target: {target}");

            if (!target)
                return null;

            if (!target.gameObject)
                return null;

            if (MapMarkers.TryGetValue(target, out var marker))
                return marker;

            // 创建一个新的 GameObject 并添加 MapMarker 组件
            var mapMarkerObject = new GameObject(nameof(MapMarker));
            mapMarkerObject.transform.SetParent(this.gameObject.transform, false);
            var mapMarker = mapMarkerObject.AddComponent<MapMarker>();
            mapMarker.Init();

            // 初始化 MapMarkerData 并配置各项参数
            var data = new MapMarkerData
            {
                Owner = this,
                Self = mapMarker,
                Target = target,
                Row = row,
                Color = color,
                Sign = sign,
                Title = title,
                Icon = null,
                Count = 0,
                Visible = true,
                ColorUpdater = colorUpdater,
                CountUpdater = countUpdater,
                VisibleUpdater = visibleUpdater
            };

            // 设置 MapMarker 的配置
            mapMarker.SetData(data);

            MapMarkers.Add(target, mapMarker);

            return mapMarker;
        }
        catch (Exception e)
        {
            LogError(e.ToString());
            return null;
        }
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void TryRemoveMapMarker(Component target)
    {
        if (MapMarkers.TryGetValue(target, out var marker))
        {
            LogDebug($"TopMapView.TryRemoveMapMarker, target: {target}, Pointer: {target.Pointer:X}");

            // 检查是否是墙体（通过判断是否在 LeftWalls 或 RightWalls 中）
            bool isWall = WallController.LeftWalls.Contains(marker) || WallController.RightWalls.Contains(marker);
            if (isWall)
            {
                WallController.RemoveWallFromList(marker);
            }
            
            // 从 WallLines 列表中查找并删除该 marker 自己的 WallLine
            // 不删除 TargetMarker == marker 的 WallLine，因为那是下一个墙的线条
            for (int i = WallController.WallLines.Count - 1; i >= 0; i--)
            {
                var wallLine = WallController.WallLines[i];
                if (wallLine != null && wallLine.OwnerMarker == marker)
                {
                    WallController.WallLines.RemoveAt(i);
                    Destroy(wallLine.gameObject);
                    break;  // 每个 marker 只有一条自己的 WallLine
                }
            }
            
            MapMarkers.Remove(target);
            Destroy(marker.gameObject);
        }
    }

    public void ClearMapMarkers()
    {
        LogDebug($"TopMapView.ClearMapMarkers");
        foreach (var pair in MapMarkers)
        {
            Destroy(pair.Value.gameObject);
        }

        MapMarkers.Clear();
    }

    public void UpdateBackgroundImage()
    {
        // 读取 PNG 文件
        var bgImageFile = Path.Combine(AssetsDir, Config.GuiStyle.TopMap.BackgroundImageFile);
        byte[] fileData = File.ReadAllBytes(bgImageFile);

        // 创建 Texture2D 并加载数据
        Texture2D texture = new Texture2D(2, 2); // 创建一个临时的纹理
        texture.LoadImage(fileData); // 加载数据

        var imageArea = (RectInt)Config.GuiStyle.TopMap.BackgroundImageArea;
        imageArea.y = texture.height - imageArea.y - imageArea.height;
        Texture2D subTexture = new Texture2D(imageArea.width, imageArea.height);
        Color[] pixels = texture.GetPixels(imageArea.x, imageArea.y, imageArea.width, imageArea.height);
        subTexture.SetPixels(pixels);
        subTexture.Apply();

        // 创建 Sprite
        Rect rect = new Rect(0, 0, subTexture.width, subTexture.height);
        Vector4 border = Config.GuiStyle.TopMap.BackgroundImageBorder; // 根据需要设置边框宽度
        Sprite sprite = Sprite.Create(MakeColoredTexture(subTexture, Config.GuiStyle.TopMap.BackgroundColor), rect, new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect, border);

        // 添加背景图，并设置九宫格
        _backgroundImage.sprite = sprite; // 将png图片放在Resources文件夹下
        _backgroundImage.type = Image.Type.Sliced;
    }
}
