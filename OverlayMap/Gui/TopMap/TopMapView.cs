using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using KingdomMod.OverlayMap.Patchers;
using KingdomMod.Shared;
using static KingdomMod.OverlayMap.OverlayMapHolder;
using static KingdomMod.OverlayMap.Patchers.ObjectPatcher;
using KingdomMod.Shared.Attributes;


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

    public static float MappingScale;
    public PlayerId PlayerId;
#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public TopMapStyle Style { get; set; }
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
    public Dictionary<Component, MapMarker> MapMarkers { get; set; }

    public static void ForEachTopMapView(System.Action<TopMapView> action)
    {
        var p1View = Instance?.PlayerOverlays.P1?.TopMapView;
        var p2View = Instance?.PlayerOverlays.P2?.TopMapView;
        if (p1View != null) action(p1View);
        if (p2View != null) action(p2View);
    }

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
            { typeof(PayableBush),          new Mappers.PayableBushMapper(this) },
            { typeof(PayableGemChest),      new Mappers.PayableGemChestMapper(this) },
            { typeof(PayableShop),          new Mappers.PayableShopMapper(this) },
            { typeof(PayableUpgrade),       new Mappers.PayableUpgradeMapper(this) },
            { typeof(PersephoneCage),       new Mappers.PersephoneCageMapper(this) },
            { typeof(Player),               new Mappers.PlayerMapper(this) },
            { typeof(Portal),               new Mappers.PortalMapper(this) },
            { typeof(River),                new Mappers.RiverMapper(this) },
            { typeof(Statue),               new Mappers.StatueMapper(this) },
            { typeof(Steed),                new Mappers.SteedMapper(this) },
            { typeof(SteedSpawn),           new Mappers.SteedSpawnMapper(this) },
            { typeof(TeleporterExit),       new Mappers.TeleporterExitMapper(this) },
            { typeof(ThorPuzzleController), new Mappers.ThorPuzzleControllerMapper(this) },
            { typeof(TimeStatue),           new Mappers.TimeStatueMapper(this) },
            { typeof(UnlockNewRulerStatue), new Mappers.UnlockNewRulerStatueMapper(this) },
            { typeof(WreckPlaceholder),     new Mappers.WreckPlaceholderMapper(this) },
        };

        Style = new TopMapStyle();
        PlayerMarkers = new List<MapMarker>();
        LeftWalls = new LinkedList<MapMarker>();
        RightWalls = new LinkedList<MapMarker>();
        MapMarkers = new Dictionary<Component, MapMarker>();

        _rectTransform = this.gameObject.AddComponent<RectTransform>();
        _backgroundImage = this.gameObject.AddComponent<Image>();

        Style.Init(this);
        UpdateLayout();
        UpdateBackgroundImage();
        // CreateText();
        // DrawLine(new Vector2(0, 0), new Vector2(300, 0), Color.red, 2);

        ObjectPatcher.OnComponentCreated += OnComponentCreated;
        ObjectPatcher.OnComponentDestroyed += OnComponentDestroyed;
        OverlayMapHolder.OnGameStateChanged += OnGameStateChanged;
        Game.OnGameStart += (System.Action)OnGameStart;
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
        Style.Destroy();
    }

    private void Start()
    {
        LogTrace("TopMapView.Start");

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
            var player = (Player)playerMarker.Data.Target;
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
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void OnComponentCreated(Component component, HashSet<SourceFlag> sources)
    {
        // if (!_isStarted)
        // {
        //     return;
        // }

        if (_componentMappers.TryGetValue(component.GetType(), out var mapper))
        {
            LogTrace($"TopMapView.OnComponentCreated, component: {component}, sources: [{string.Join(", ", sources)}]");
            // LogTrace($"TopMapView.OnComponentCreated, StackTrace: {System.Environment.StackTrace}");
            mapper.Map(component);
        }
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void OnComponentDestroyed(Component component, HashSet<SourceFlag> sources)
    {
        TryRemoveMapMarker(component, sources);
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
        _timeSinceLastGuiUpdate += Time.deltaTime;

        if (_timeSinceLastGuiUpdate > (1.0 / Global.GuiUpdatesPerSecond))
        {
            _timeSinceLastGuiUpdate = 0;

            if (!IsPlaying()) return;

            UpdateExploredRegion();

            foreach (var pair in MapMarkers)
            {
                var markerData = pair.Value.Data;
                var worldPosX = markerData.Target.transform.position.x;
                markerData.IsInFogOfWar = !(ShowFullMap || (worldPosX >= SaveDataExtras.ExploredLeft && worldPosX <= SaveDataExtras.ExploredRight));

                if (markerData.IsInFogOfWar)
                    continue;

                if (markerData.VisibleUpdater != null)
                    markerData.Visible = markerData.VisibleUpdater(markerData.Target);
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
            LogDebug($"TopMapView.TryAddMapMarker, title: {title?.Value}, target: {target}");

            if (target.gameObject == null)
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
    public void TryRemoveMapMarker(Component target, HashSet<SourceFlag> sources)
    {
        if (MapMarkers.TryGetValue(target, out var marker))
        {
            LogDebug($"TopMapView.TryRemoveMapMarker, target: {target}, sources: [{string.Join(", ", sources)}]");

            RemoveWallNode(marker);
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
        var bgImageFile = Path.Combine(AssetsDir, GuiStyle.TopMap.BackgroundImageFile);
        byte[] fileData = File.ReadAllBytes(bgImageFile);

        // 创建 Texture2D 并加载数据
        Texture2D texture = new Texture2D(2, 2); // 创建一个临时的纹理
        texture.LoadImage(fileData); // 加载数据

        var imageArea = (RectInt)GuiStyle.TopMap.BackgroundImageArea;
        imageArea.y = texture.height - imageArea.y - imageArea.height;
        Texture2D subTexture = new Texture2D(imageArea.width, imageArea.height);
        Color[] pixels = texture.GetPixels(imageArea.x, imageArea.y, imageArea.width, imageArea.height);
        subTexture.SetPixels(pixels);
        subTexture.Apply();

        // 创建 Sprite
        Rect rect = new Rect(0, 0, subTexture.width, subTexture.height);
        Vector4 border = GuiStyle.TopMap.BackgroundImageBorder; // 根据需要设置边框宽度
        Sprite sprite = Sprite.Create(MakeColoredTexture(subTexture, GuiStyle.TopMap.BackgroundColor), rect, new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect, border);

        // 添加背景图，并设置九宫格
        _backgroundImage.sprite = sprite; // 将png图片放在Resources文件夹下
        _backgroundImage.type = Image.Type.Sliced;
    }
    private void CreateText()
    {
        GameObject textObj = new GameObject("GroupText");
        textObj.transform.SetParent(_rectTransform, false);
        _groupText = textObj.AddComponent<Text>();

        _groupText.text = "这是一个文本控件\u2663\u265c\u06e9";
        _groupText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _groupText.fontSize = 12;
        _groupText.color = Color.white;
        _groupText.alignment = TextAnchor.MiddleCenter;

        // // 添加描边效果
        // Outline outline = textObj.AddComponent<Outline>();
        // outline.effectColor = new Color(0, 0, 0, 0.5f); // 描边颜色 (黑色)
        // outline.effectDistance = new Vector2(1, -1); // 描边偏移
        //
        // // 添加阴影效果
        // Shadow shadow = textObj.AddComponent<Shadow>();
        // shadow.effectColor = new Color(0, 0, 0, 0.5f); // 阴影颜色 (黑色，半透明)
        // shadow.effectDistance = new Vector2(2, -2); // 阴影偏移

        // 设置文本控件的尺寸和位置
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.sizeDelta = new Vector2(0, 0); // 填充整个 Group
    }

    private void DrawLine(Vector2 lineStart, Vector2 lineEnd, Color color, uint thickness)
    {
        GameObject lineObj = new GameObject("Line");
        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = color; // 直线颜色

        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        lineRect.SetParent(_rectTransform, false);

        Vector2 lineDelta = lineEnd - lineStart;
        float distance = lineDelta.magnitude;
        lineRect.pivot = new Vector2(0, 0.5f); // 中心点在左侧中间
        lineRect.sizeDelta = new Vector2(distance, thickness);
        lineRect.anchoredPosition = lineStart;
        lineRect.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(lineDelta.y, lineDelta.x) * Mathf.Rad2Deg);
    }

    //
    // Wall lines controller
    //

    // public void AddWallLine(MapMarker wallMarker, bool isCastle = false)
    // {
    //     LinkedListNode<WallLine> node;
    //     var wallLine = WallLine.Create(wallMarker);
    //     var kingdom = Managers.Inst.kingdom;
    //     var worldPosX = wallMarker.Data.Target.transform.position.x;
    //     var wallLines = kingdom.GetBorderSideForPosition(worldPosX) == Side.Left ? SidedWalls.left : SidedWalls.right;
    //     if (wallLines.Count == 0 || worldPosX <= wallLines.First.Value.Owner.Data.Target.transform.position.x)
    //     {
    //         node = wallLines.AddFirst(wallLine);
    //     }
    //     else if (worldPosX >= wallLines.Last.Value.Owner.Data.Target.transform.position.x)
    //     {
    //         node = wallLines.AddLast(wallLine);
    //     }
    //     else
    //     {
    //         var current = wallLines.First;
    //         while (current != null && current.Value.Owner.Data.Target.transform.position.x < worldPosX)
    //         {
    //             current = current.Next;
    //         }
    //
    //         if (current != null)
    //             node = wallLines.AddBefore(current, wallLine);
    //         else
    //             node = wallLines.AddLast(wallLine);
    //     }
    //
    //     wallLine.Init(node);
    // }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private LinkedList<MapMarker> GetLinkedWalls(MapMarker wallMarker)
    {
        var kingdom = Managers.Inst.kingdom;
        var worldPosX = wallMarker.Data.Target.transform.position.x;
        var linkedWalls = kingdom.GetBorderSideForPosition(worldPosX) == Side.Left ? LeftWalls : RightWalls;
        return linkedWalls;
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void AddWallNode(MapMarker wallMarker)
    {
        LinkedListNode<MapMarker> wallNode;
        var linkedWalls = GetLinkedWalls(wallMarker);
        var worldPosX = Math.Abs(wallMarker.Data.Target.transform.position.x);
        var current = linkedWalls.First;
        while (current != null && Math.Abs(current.Value.Data.Target.transform.position.x) < worldPosX)
        {
            current = current.Next;
        }

        if (current != null)
            wallNode = linkedWalls.AddBefore(current, wallMarker);
        else
            wallNode = linkedWalls.AddLast(wallMarker);

        AddWallLine(wallNode);
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void AddWallLine(LinkedListNode<MapMarker> wallNode)
    {
        var currentWall = wallNode.Value;

        // 获取上一个城墙或城堡
        MapMarker previousWall;
        if (wallNode.Previous != null && wallNode.Previous.Value != null)
            previousWall = wallNode.Previous.Value;
        else
            previousWall = CastleMarker; // 如果是第一个城墙，则从城堡开始

        // 创建新的 WallLine 组件
        var wallLine = WallLine.Create(currentWall);
        wallLine.Init(wallNode);
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void UpdateWallLinePosition(MapMarker wallMarker)
    {
        var wallLineRect = wallMarker.transform.Find("WallLine");
        var wallLineObject = wallLineRect.gameObject;


    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    public void RemoveWallNode(MapMarker wallMarker)
    {
        var linkedWalls = GetLinkedWalls(wallMarker);
        linkedWalls.Remove(wallMarker);
    }

    public void ClearWallNodes()
    {
        LeftWalls.Clear();
        RightWalls.Clear();
    }

    private WallLine CreateWallLine()
    {
        var wallLineObject = new GameObject(nameof(WallLine));
        wallLineObject.transform.SetParent(this.gameObject.transform, false);
        var wallLine = wallLineObject.AddComponent<WallLine>();
        return wallLine;
    }
}