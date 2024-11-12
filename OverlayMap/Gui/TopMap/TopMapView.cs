using System;
using System.Collections.Generic;
using System.IO;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using KingdomMod.OverlayMap.Patchers;
using UnityEngine;
using UnityEngine.UI;
using static KingdomMod.OverlayMap.OverlayMapHolder;
using static KingdomMod.OverlayMap.Patchers.ObjectPatcher;

namespace KingdomMod.OverlayMap.Gui.TopMap;

public class TopMapView : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Image _backgroundImage;
    private Text _groupText;
    private readonly Dictionary<Component, MapMarker> _mapMarkers = new();
    public readonly List<MapMarker> PlayerMarkers = new();
    private float _timeSinceLastGuiUpdate = 0;

    public static float MappingScale;
    // public static float MapOffset;
    // public static float ZoomScale;
    public static readonly ExploredRegion ExploredRegion = new();
    public PlayerId PlayerId;
    public TopMapStyle Style = new();
    public Dictionary<Component, MapMarker> MapMarkers => _mapMarkers;
    private readonly Dictionary<Type, IComponentMapper> _componentMappers;

    public TopMapView()
    {
        LogTrace("TopMapView.Constructor");

        _componentMappers = new Dictionary<Type, IComponentMapper>
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
    }

    private void Awake()
    {
        LogTrace("TopMapView.Awake");

        _rectTransform = this.gameObject.AddComponent<RectTransform>();
        _backgroundImage = this.gameObject.AddComponent<Image>();

        Style.Init(this);
        UpdateLayout();
        UpdateBackgroundImage();
        // CreateText();
        // DrawLine(new Vector2(0, 0), new Vector2(300, 0), Color.red, 2);

        ObjectPatcher.OnComponentCreated += OnComponentCreated;
        ObjectPatcher.OnComponentDestroyed += OnComponentDestroyed;
        OverlayMapHolder.OnDirectorStateChanged += OnDirectorStateChanged;
        OverlayMapHolder.OnGameStateChanged += OnGameStateChanged;
        Game.OnGameStart += OnGameStart;
    }

    public void Init(PlayerId playerId)
    {
        PlayerId = playerId;
    }

    private void OnDestroy()
    {
        ObjectPatcher.OnComponentCreated -= OnComponentCreated;
        ObjectPatcher.OnComponentDestroyed -= OnComponentDestroyed;
        OverlayMapHolder.OnDirectorStateChanged -= OnDirectorStateChanged;
        OverlayMapHolder.OnGameStateChanged -= OnGameStateChanged;
        Game.OnGameStart -= OnGameStart;
        Style.Destroy();
    }

    private void Start()
    {
        LogTrace("TopMapView.Start");

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

    private void UpdatePlayerMarker()
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

    private void OnDirectorStateChanged(ProgramDirector.State state)
    {
        LogTrace($"ProgramDirector.state changed to {state}");

        if (state == ProgramDirector.State.LoadingMainScene)
        {
        }
    }

    public void OnGameStart()
    {
        LogTrace("TopMapView.OnGameStart");

        ExploredRegion.Init();

        var clientWidth = Screen.width - 40f;
        var minLevelWidth = Managers.Inst.game.currentLevelConfig.minLevelWidth;
        MappingScale = clientWidth / minLevelWidth;
        LogTrace($"MappingScale: {MappingScale}, minLevelWidth: {minLevelWidth}");

        UpdatePlayerMarker();
    }

    private void OnComponentCreated(Component component, HashSet<SourceFlag> sources)
    {
        if (_componentMappers.TryGetValue(component.GetType(), out var mapper))
        {
            LogTrace($"TopMapView.OnComponentCreated, component: {component}, sources: [{string.Join(", ", sources)}]");
            mapper.Map(component);
        }
    }

    private void OnComponentDestroyed(Component component, HashSet<SourceFlag> sources)
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
            ExploredRegion.ExploredLeft = Math.Min(ExploredRegion.ExploredLeft, l);
            ExploredRegion.ExploredRight = Math.Max(ExploredRegion.ExploredRight, r);
        }

        float wallLeft = Managers.Inst.kingdom.GetBorderSide(Side.Left);
        float wallRight = Managers.Inst.kingdom.GetBorderSide(Side.Right);

        ExploredRegion.ExploredLeft = Math.Min(ExploredRegion.ExploredLeft, wallLeft);
        ExploredRegion.ExploredRight = Math.Max(ExploredRegion.ExploredRight, wallRight);
    }

    private void Update()
    {
        _timeSinceLastGuiUpdate += Time.deltaTime;

        if (_timeSinceLastGuiUpdate > (1.0 / Global.GuiUpdatesPerSecond))
        {
            _timeSinceLastGuiUpdate = 0;

            if (!IsPlaying()) return;

            UpdateExploredRegion();

            foreach (var pair in _mapMarkers)
            {
                var markerData = pair.Value.Data;
                var worldPosX = markerData.Target.transform.position.x;
                markerData.IsInFogOfWar = !(ShowFullMap || (worldPosX >= ExploredRegion.ExploredLeft && worldPosX <= ExploredRegion.ExploredRight));

                if (markerData.IsInFogOfWar)
                    continue;

                if (markerData.VisibleUpdater != null)
                    markerData.Visible = markerData.VisibleUpdater(markerData.Target);
            }
        }
    }

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

            if (_mapMarkers.TryGetValue(target, out var marker))
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

            _mapMarkers.Add(target, mapMarker);

            return mapMarker;
        }
        catch (Exception e)
        {
            LogError(e.ToString());
            return null;
        }
    }

    public void TryRemoveMapMarker(Component target, HashSet<SourceFlag> sources)
    {
        if (_mapMarkers.TryGetValue(target, out var marker))
        {
            LogDebug($"TopMapView.TryRemoveMapMarker, target: {target}, sources: [{string.Join(", ", sources)}]");

            // 从列表中移除
            _mapMarkers.Remove(target);
            // 销毁 GameObject
            Destroy(marker.gameObject);
        }
    }

    public void ClearMapMarkers()
    {
        LogDebug($"TopMapView.ClearMapMarkers");
        foreach (var pair in _mapMarkers)
        {
            Destroy(pair.Value.gameObject);
        }

        _mapMarkers.Clear();
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

    // 在 Group 中添加文本控件
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

    // 在 Group 上绘制一条直线
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
}