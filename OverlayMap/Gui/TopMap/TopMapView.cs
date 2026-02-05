using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using KingdomMod.OverlayMap.Patchers;
using static KingdomMod.OverlayMap.OverlayMapHolder;
using KingdomMod.Shared.Attributes;
using Il2CppInterop.Runtime;
using static UnityEngine.GraphicsBuffer;


#if IL2CPP
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
#endif

namespace KingdomMod.OverlayMap.Gui.TopMap;

[RegisterTypeInIl2Cpp]
public class TopMapView : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Image _backgroundImage;
    private Text _groupText;
    private float _timeSinceLastGuiUpdate;
    private bool _isStarted;

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private Dictionary<Type, IComponentMapper> _componentMappers { get; set; }

    private Dictionary<IntPtr, IComponentMapper> _fastLookup;
    
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
    public LinkedList<MapMarker> LeftWalls { get; set; }
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public LinkedList<MapMarker> RightWalls { get; set; }
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public List<WallLine> WallLines { get; set; }
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public Dictionary<Component, MapMarker> MapMarkers { get; set; }

#if IL2CPP
    public TopMapView(IntPtr ptr) : base(ptr) { }
#endif

    public TopMapView()
    {
        LogTrace("TopMapView.Constructor");
    }

    private void Awake()
    {
        LogTrace("TopMapView.Awake");

        _componentMappers = new Dictionary<Type, IComponentMapper>()
        {
            { typeof(Beach),                new Mappers.BeachMapper(this) },
            { typeof(BeggarCamp),           new Mappers.BeggarCampMapper(this) },
            { typeof(Beggar),               new Mappers.BeggarMapper(this) },
            { typeof(BoarSpawnGroup),       new Mappers.BoarSpawnGroupMapper(this) },
            { typeof(Boat),                 new Mappers.BoatMapper(this) },
            { typeof(BoatSummoningBell),    new Mappers.BoatSummoningBellMapper(this) },
            { typeof(Bomb),                 new Mappers.BombMapper(this) },
            { typeof(Cabin),                new Mappers.CabinMapper(this) },
            { typeof(Campfire),             new Mappers.CampfireMapper(this) },
            { typeof(Castle),               new Mappers.CastleMapper(this) },
            { typeof(Chest),                new Mappers.ChestMapper(this) },
            { typeof(CitizenHousePayable),  new Mappers.CitizenHousePayableMapper(this) },
            { typeof(Deer),                 new Mappers.DeerMapper(this) },
            { typeof(DogSpawn),             new Mappers.DogSpawnMapper(this) },
            { typeof(Farmhouse),            new Mappers.FarmhouseMapper(this) },
            { typeof(HelPuzzleController),  new Mappers.HelPuzzleControllerMapper(this) },
            { typeof(HephaestusForge),      new Mappers.HephaestusForgeMapper(this) },
            { typeof(MerchantSpawner),      new Mappers.MerchantSpawnerMapper(this) },
            { typeof(PayableBlocker),       new Mappers.PayableBlockerMapper(this) },
            { typeof(PayableBush),          new Mappers.PayableBushMapper(this) },
            { typeof(PayableGemChest),      new Mappers.PayableGemChestMapper(this) },
            { typeof(PayableShop),          new Mappers.PayableShopMapper(this) },
            { typeof(PayableUpgrade),       new Mappers.PayableUpgradeMapper(this) },
            { typeof(PersephoneCage),       new Mappers.PersephoneCageMapper(this) },
            { typeof(Player),               new Mappers.PlayerMapper(this) },
            { typeof(Portal),               new Mappers.PortalMapper(this) },
            { typeof(River),                new Mappers.RiverMapper(this) },
            { typeof(Scaffolding),          new Mappers.ScaffoldingMapper(this) },
            { typeof(Statue),               new Mappers.StatueMapper(this) },
            { typeof(Steed),                new Mappers.SteedMapper(this) },
            { typeof(SteedSpawn),           new Mappers.SteedSpawnMapper(this) },
            { typeof(TeleporterExit),       new Mappers.TeleporterExitMapper(this) },
            { typeof(ThorPuzzleController), new Mappers.ThorPuzzleControllerMapper(this) },
            { typeof(TimeStatue),           new Mappers.TimeStatueMapper(this) },
            { typeof(UnlockNewRulerStatue), new Mappers.UnlockNewRulerStatueMapper(this) },
        };

        BuildFastLookup();
        
        // 创建 EnemyMapper（不注册到 _componentMappers，因为它不通过 ObjectPatcher 触发）
        _enemyMapper = new Mappers.EnemyMapper(this);

        PlayerMarkers = new List<MapMarker>();
        LeftWalls = new LinkedList<MapMarker>();
        RightWalls = new LinkedList<MapMarker>();
        WallLines = new List<WallLine>();
        MapMarkers = new Dictionary<Component, MapMarker>();

        _rectTransform = this.gameObject.AddComponent<RectTransform>();
        _backgroundImage = this.gameObject.AddComponent<Image>();

        UpdateLayout();
        UpdateBackgroundImage();

        ObjectPatcher.OnComponentCreated += OnComponentCreated;
        ObjectPatcher.OnComponentDestroyed += OnComponentDestroyed;
        OverlayMapHolder.OnGameStateChanged += OnGameStateChanged;
        Game.OnGameStart += (System.Action)OnGameStart;
        Level.OnLoaded += (System.Action<bool>)OnLevelLoaded;
    }

    private void BuildFastLookup()
    {
        _fastLookup = new Dictionary<IntPtr, IComponentMapper>();
        foreach (var kvp in _componentMappers)
        {
            // 获取 System.Type 对应的 IL2CPP 类型指针
            var il2cppType = Il2CppType.From(kvp.Key);
            _fastLookup[il2cppType.Pointer] = kvp.Value;
        }
    }

    public void Init(PlayerId playerId)
    {
        PlayerId = playerId;
    }

    private void OnDestroy()
    {
        ObjectPatcher.OnComponentCreated -= OnComponentCreated;
        ObjectPatcher.OnComponentDestroyed -= OnComponentDestroyed;
        OverlayMapHolder.OnGameStateChanged -= OnGameStateChanged;
        Game.OnGameStart -= (System.Action)OnGameStart;
        Level.OnLoaded -= (System.Action<bool>)OnLevelLoaded;
    }

    private void Start()
    {
        LogTrace("TopMapView.Start");

        // foreach (var kv in _componentMappers)
        // {
        //     var mapper = kv.Value;
        //     var components = mapper.GetComponents();
        //     foreach (var component in components)
        //     {
        //         if (component != null)
        //             mapper.Map(component);
        //     }
        // }

        _isStarted = true;
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
        LogTrace($"OnGameStateChanged.state changed to {state}");

        switch (state)
        {
            case Game.State.Playing:
            case Game.State.NetworkClientPlaying:
                break;
            case Game.State.Menu:
                break;
            case Game.State.Quitting:
                PlayerMarkers.Clear();
                ClearWallNodes();
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
        LogTrace("TopMapView.OnGameStart");

        var clientWidth = Screen.width - 40f;
        var minLevelWidth = Managers.Inst.game.currentLevelConfig.minLevelWidth;
        MappingScale = clientWidth / minLevelWidth;
        LogTrace($"MappingScale: {MappingScale}, minLevelWidth: {minLevelWidth}");
        
        UpdatePlayerMarker();

        // 游戏开始时自动调整地图偏移以居中显示
        // 在这里调用是因为此时 SaveDataExtras 已经初始化，且所有 MapMarkers 已创建
        AutoCenterMap();
    }
    
    private void OnLevelLoaded(bool fromSave)
    {
        LogWarning($"OnLevelLoaded: fromSave: {fromSave}, MapMarkers: {MapMarkers.Count}");
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
            LogTrace("AutoCenterMap: No markers to center");
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
        
        LogInfo($"AutoCenterMap: minWorldX={minWorldX}, maxWorldX={maxWorldX}, offset={offset}");
        
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
    public void OnComponentCreated(Component comp)
    {
        // 获取组件底层的真实类型指针
        var typePtr = comp.GetIl2CppType().Pointer;

        // 指针碰撞，极速查找
        if (_fastLookup.TryGetValue(typePtr, out var mapper))
        {
            LogTrace($"OnComponentCreated Found {comp.GetIl2CppType().Name} on {comp.name}, Pointer: {comp.Pointer:X}");
            mapper.Map(comp);
        }
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
            bool isWall = LeftWalls.Contains(marker) || RightWalls.Contains(marker);
            if (isWall)
            {
                RemoveWallFromList(marker);
            }
            
            // 从 WallLines 列表中查找并删除该 marker 自己的 WallLine
            // 不删除 TargetMarker == marker 的 WallLine，因为那是下一个墙的线条
            for (int i = WallLines.Count - 1; i >= 0; i--)
            {
                var wallLine = WallLines[i];
                if (wallLine != null && wallLine.OwnerMarker == marker)
                {
                    WallLines.RemoveAt(i);
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

    //
    // Wall lines controller
    //

    /// <summary>
    /// 将墙体 marker 添加到 LeftWalls 或 RightWalls 列表，并创建连接线
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void AddWallToList(MapMarker wallMarker)
    {
        if (wallMarker == null || wallMarker.Data == null || wallMarker.Data.Target == null)
        {
            LogError("AddWallToList: wallMarker or its data is null");
            return;
        }

        // 判断墙在城堡的左侧还是右侧
        var kingdom = Managers.Inst.kingdom;
        var worldPosX = wallMarker.Data.Target.transform.position.x;
        var isLeftSide = kingdom.GetBorderSideForPosition(worldPosX) == Side.Left;
        var wallList = isLeftSide ? LeftWalls : RightWalls;

        // 根据 worldPosX 的绝对值插入到有序列表中
        var absX = Math.Abs(worldPosX);
        LinkedListNode<MapMarker> insertNode = null;
        var current = wallList.First;
        
        while (current != null)
        {
            var currentAbsX = Math.Abs(current.Value.Data.Target.transform.position.x);
            if (absX < currentAbsX)
            {
                insertNode = current;
                break;
            }
            current = current.Next;
        }

        // 插入到列表
        LinkedListNode<MapMarker> wallNode;
        if (insertNode != null)
            wallNode = wallList.AddBefore(insertNode, wallMarker);
        else
            wallNode = wallList.AddLast(wallMarker);

        // 创建从前一个墙（或城堡）到当前墙的连接线
        CreateWallLineForNode(wallNode);
        
        // 如果当前墙后面还有墙，需要更新下一个墙的连接线（指向当前墙而不是前一个）
        if (wallNode.Next != null)
        {
            UpdateWallLineForNode(wallNode.Next);
        }
    }

    /// <summary>
    /// 从列表中移除墙体 marker，并清理相关连接线
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void RemoveWallFromList(MapMarker wallMarker)
    {
        if (wallMarker == null)
            return;

        // 先确定墙体在哪个列表中
        bool isInLeftWalls = LeftWalls.Contains(wallMarker);
        bool isInRightWalls = RightWalls.Contains(wallMarker);
        
        if (!isInLeftWalls && !isInRightWalls)
        {
            LogWarning($"RemoveWallFromList: wallMarker not found in LeftWalls or RightWalls");
            return;
        }

        var wallList = isInLeftWalls ? LeftWalls : RightWalls;
        
        // 重要：在移除之前找到下一个节点
        LinkedListNode<MapMarker> nextNode = null;
        var currentNode = wallList.First;
        while (currentNode != null)
        {
            if (currentNode.Value == wallMarker)
            {
                nextNode = currentNode.Next;
                break;
            }
            currentNode = currentNode.Next;
        }
        
        // 从列表中移除
        wallList.Remove(wallMarker);
        
        // 更新下一个墙的连接线（如果存在）
        if (nextNode != null)
        {
            UpdateWallLineForNode(nextNode);
        }
    }

    /// <summary>
    /// 为指定节点创建连接线
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private void CreateWallLineForNode(LinkedListNode<MapMarker> wallNode)
    {
        if (wallNode == null || wallNode.Value == null)
            return;

        var currentWall = wallNode.Value;
        
        // 获取连接目标（前一个墙或城堡）
        MapMarker targetMarker;
        if (wallNode.Previous != null && wallNode.Previous.Value != null)
            targetMarker = wallNode.Previous.Value;
        else
            targetMarker = CastleMarker; // 第一个墙连接到城堡

        if (targetMarker == null)
        {
            LogError("CreateWallLineForNode: target marker (castle or previous wall) is null");
            return;
        }

        // 获取线条颜色（与当前墙的颜色一致）
        // ConfigEntryWrapper<string> 有隐式转换到 Color
        Color lineColor = currentWall.Data.Color != null 
            ? (Color)currentWall.Data.Color 
            : Color.green;

        // 创建 WallLine 组件
        // 注意：WallLine 是 TopMapView 的子对象，而不是 MapMarker 的子对象
        var wallLine = WallLine.Create(this, currentWall, targetMarker, lineColor);
        
        // 添加到 WallLines 列表
        WallLines.Add(wallLine);
        
        LogDebug($"Created WallLine for wall at {currentWall.Data.Target.transform.position.x}, connecting to {targetMarker.Data.Target.transform.position.x}, color: {lineColor}");
    }

    /// <summary>
    /// 更新指定节点的连接线（删除旧线条并创建新线条）
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private void UpdateWallLineForNode(LinkedListNode<MapMarker> wallNode)
    {
        if (wallNode == null || wallNode.Value == null)
            return;

        var wallMarker = wallNode.Value;
        
        // 从 WallLines 列表中查找并销毁旧的 WallLine
        for (int i = WallLines.Count - 1; i >= 0; i--)
        {
            var wallLine = WallLines[i];
            if (wallLine != null && wallLine.OwnerMarker == wallMarker)
            {
                WallLines.RemoveAt(i);
                Destroy(wallLine.gameObject);
                break;  // 每个墙体只有一条自己的 WallLine
            }
        }

        // 创建新的 WallLine
        CreateWallLineForNode(wallNode);
    }

    /// <summary>
    /// 在列表中查找指定墙体的下一个节点
    /// </summary>
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private LinkedListNode<MapMarker> FindNextWallNode(LinkedList<MapMarker> wallList, MapMarker wallMarker)
    {
        var current = wallList.First;
        LinkedListNode<MapMarker> previousNode = null;
        
        while (current != null)
        {
            if (previousNode != null && previousNode.Value == wallMarker)
            {
                return current;
            }
            previousNode = current;
            current = current.Next;
        }
        
        return null;
    }

    /// <summary>
    /// 清空墙体列表和所有 WallLine（游戏退出时调用）
    /// </summary>
    public void ClearWallNodes()
    {
        // 销毁所有 WallLine
        foreach (var wallLine in WallLines)
        {
            if (wallLine != null)
            {
                Destroy(wallLine.gameObject);
            }
        }
        WallLines.Clear();
        
        LeftWalls.Clear();
        RightWalls.Clear();
    }
}
