using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingdomMod.OverlayMap.Config;
using KingdomMod.OverlayMap.Config.Extensions;
using UnityEngine;
using UnityEngine.UI;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.View;

public struct MapMarkerConfig
{
    public MonoBehaviour Source;
    public ConfigEntryWrapper<string> Color;
    public ConfigEntryWrapper<string> Sign;
    public ConfigEntryWrapper<string> Title;
    public ConfigEntryWrapper<string> Icon;
}

public class MapMarker : MonoBehaviour
{
    private RectTransform _rectTransform;
    private float _worldPosX;
    // private Image _icon;
    private Text _sign;
    private Text _title;
    private Text _count;
    private MapMarkerConfig _config;

    private void Awake()
    {
        LogMessage("MapMarker.Awake");
        _rectTransform = this.gameObject.AddComponent<RectTransform>();
        // _icon = this.gameObject.AddComponent<Image>();
        CreateTextObject("Sign", -10, out _sign);
        CreateTextObject("Title", -26, out _title);
        CreateTextObject("Count", -26 - 16, out _count);

        // 设置Group的尺寸和外边距

        // 设置锚点
        _rectTransform.anchorMin = new Vector2(0.5f, 1); // 以顶部中心为锚点
        _rectTransform.anchorMax = new Vector2(0.5f, 1); // 以顶部中心为锚点
        _rectTransform.pivot = new Vector2(0.5f, 0.5f); // 以顶部中心为支点

    }

    private void CreateTextObject(string objName, float yPos, out Text textComponent)
    {
        GameObject textObject = new GameObject(objName);
        textObject.transform.SetParent(this.transform);
        textComponent = textObject.AddComponent<Text>();

        // 设置文本的其他属性（如字体、颜色等）
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 12;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;

        var rectTrans = textObject.GetComponent<RectTransform>();
        rectTrans.anchoredPosition = new Vector2(0, yPos);
    }

    public void SetConfig(MapMarkerConfig config)
    {
        LogMessage($"MapMarker.SetConfig, _sign:{_sign}, _name:{_title}, _config.Sign:{_config.Sign}, _config.Title:{_config.Title}");
        _config = config;

        _sign.text = _config.Sign.Value;
        _title.text = _config.Title.Value;
    }

    public void UpdatePosition()
    {
        // 从中心到两边分布

        if (Math.Abs(_worldPosX - _config.Source.transform.position.x) > 0.1f)
        {
            _worldPosX = _config.Source.transform.position.x;
            _rectTransform.anchoredPosition = new Vector2(
                _worldPosX * TopMapView.MappingScale,
                _rectTransform.anchoredPosition.y
            );
        }
    }
}

public class TopMapView : MonoBehaviour
{
    public static float MappingScale;
    private RectTransform _rectTransform;
    private Image _backgroundImage;
    private Text _groupText;
    private Dictionary<MonoBehaviour, MapMarker> _mapMarkers = new ();
    private float _timeSinceLastGuiUpdate = 0;

    private void Awake()
    {
        LogMessage("TopMapView.Awake");

        _rectTransform = this.gameObject.AddComponent<RectTransform>();
        _backgroundImage = this.gameObject.AddComponent<Image>();

        // 设置Group的尺寸和外边距

        // 设置锚点
        _rectTransform.anchorMin = new Vector2(0, 1); // 左上角
        _rectTransform.anchorMax = new Vector2(1, 1); // 右上角
        _rectTransform.pivot = new Vector2(0.5f, 1); // 以顶部中心为支点

        // 设置外边距
        _rectTransform.offsetMin = new Vector2(5, -155); // 左边距5，顶部边距5
        _rectTransform.offsetMax = new Vector2(-5, -5); // 右边距5，底部边距0

        ReloadBackgroundImage();
        CreateText();
        DrawLine(new Vector2(0, 0), new Vector2(300, 0), Color.red, 2);

        Config.GuiStyle.OnChangedEvent += GuiStyleOnChangedEvent;
        Game.OnGameStart += (Action)OnGameStart;
    }


    public static void OnGameStart()
    {
        LogMessage("TopMapView.OnGameStart");

        var clientWidth = Screen.width - 40f;
        var minLevelWidth = Managers.Inst.game.currentLevelConfig.minLevelWidth;
        MappingScale = clientWidth / minLevelWidth;
        LogMessage($"MappingScale: {MappingScale}, minLevelWidth: {minLevelWidth}");
    }

    private void GuiStyleOnChangedEvent(object sender, FileSystemEventArgs e)
    {
        ReloadBackgroundImage();
    }

    private void Update()
    {
        _timeSinceLastGuiUpdate += Time.deltaTime;

        if (_timeSinceLastGuiUpdate > (1.0 / Global.GuiUpdatesPerSecond))
        {
            _timeSinceLastGuiUpdate = 0;

            if (!IsPlaying()) return;

            UpdateMapMarkers();
        }

    }

    private void OnGUI()
    {
        if (!IsPlaying()) return;

        // if (OverlayMapHolder.Instance.EnabledOverlayMap)
        // {
        //     DrawGuiForPlayer(0);
        //     DrawGuiForPlayer(1);
        // }
    }

    public void TryAddMapMarker(
        MonoBehaviour source,
        ConfigEntryWrapper<string> color,
        ConfigEntryWrapper<string> sign,
        ConfigEntryWrapper<string> title)
    {
        if (_mapMarkers.TryGetValue(source, out var marker))
        {
            marker.UpdatePosition();
            return;
        }

        // 创建一个新的 GameObject 并添加 MapMarker 组件
        var mapMarkerObject = new GameObject(nameof(MapMarker));
        var mapMarker = mapMarkerObject.AddComponent<MapMarker>();

        // 初始化 MapMarkerConfig 并配置各项参数
        var config = new MapMarkerConfig
        {
            Source = source,
            Color = color,
            Sign = sign,
            Title = title,
            Icon = null
        };

        // 设置 MapMarker 的配置
        mapMarker.SetConfig(config);

        // 将 MapMarker 对象添加到 _topMapView
        mapMarkerObject.transform.SetParent(this.gameObject.transform, false);

        _mapMarkers.Add(source, mapMarker);
    }

    public void RemoveMapMarker(MonoBehaviour source)
    {
        if (_mapMarkers.ContainsKey(source))
        {
            // 从列表中移除
            _mapMarkers.Remove(source);
            // 销毁 GameObject
            Destroy(source.gameObject);
        }
        else
        {
            LogWarning("MapMarker not found in the list.");
        }
    }

    private void UpdateMapMarkers()
    {
        var world = Managers.Inst.world;
        if (world == null) return;
        var level = Managers.Inst.level;
        if (level == null) return;
        var kingdom = Managers.Inst.kingdom;
        if (kingdom == null) return;
        var payables = Managers.Inst.payables;
        if (payables == null) return;

        var castle = kingdom.castle;
        if (castle != null)
        {
            var payable = castle._payableUpgrade;
            bool canPay = !payable.IsLocked(GetLocalPlayer(), out var reason);
            bool isLocked = reason != LockIndicator.LockReason.NotLocked && reason != LockIndicator.LockReason.NoUpgrade;
            bool isLockedForInvalidTime = reason == LockIndicator.LockReason.InvalidTime;
            var price = isLockedForInvalidTime ? (int)(payable.timeAvailableFrom - Time.time) : canPay ? payable.Price : 0;
            var color = isLocked ? MarkerStyle.Castle.Locked.Color : MarkerStyle.Castle.Color;
            TryAddMapMarker(castle, color, MarkerStyle.Castle.Sign, Strings.Castle);
        }

        foreach (var obj in kingdom.AllPortals)
        {
            if (obj.type == Portal.Type.Regular)
                TryAddMapMarker(obj, MarkerStyle.Portal.Color, MarkerStyle.Portal.Sign, Strings.Portal);
            else if (obj.type == Portal.Type.Cliff)
                TryAddMapMarker(obj, obj.state switch
                {
                    Portal.State.Destroyed => MarkerStyle.PortalCliff.Destroyed.Color,
                    Portal.State.Rebuilding => MarkerStyle.PortalCliff.Rebuilding.Color,
                    _ => MarkerStyle.PortalCliff.Color
                }, MarkerStyle.PortalCliff.Sign, Strings.PortalCliff);
            else if (obj.type == Portal.Type.Dock)
                TryAddMapMarker(obj, MarkerStyle.PortalDock.Color, MarkerStyle.PortalDock.Sign, Strings.PortalDock);
        }

        // if (_mapMarkers.Count == 0)
        //     return;
        //
        // var startPos = 0f;
        // var endPos = 0f;
        //
        // foreach (var poi in _mapMarkers)
        // {
        //     startPos = System.Math.Min(startPos, poi.Key.transform.position.x);
        //     endPos = System.Math.Max(endPos, poi.Key.transform.position.x);
        // }
        //
        // var mapWidth = endPos - startPos;
        // var clientWidth = Screen.width - 40;
        // var scale = clientWidth / mapWidth;

        // foreach (var poi in poiList)
        // {
        //     poi.Pos = (poi.WorldPosX - startPos) * scale + 16;
        // }
    }

    public void ReloadBackgroundImage()
    {
        // 读取 PNG 文件
        var guiStylePath = Path.Combine(GetBepInExDir(), "config", "GuiStyle");
        var bgImageFile = Path.Combine(guiStylePath, GuiStyle.BackgroundImageFile);
        byte[] fileData = File.ReadAllBytes(bgImageFile);

        // 创建 Texture2D 并加载数据
        Texture2D texture = new Texture2D(2, 2); // 创建一个临时的纹理
        texture.LoadImage(fileData); // 加载数据

        var imageArea = (RectInt)GuiStyle.BackgroundImageArea;
        imageArea.y = texture.height - imageArea.y - imageArea.height;
        Texture2D subTexture = new Texture2D(imageArea.width, imageArea.height);
        Color[] pixels = texture.GetPixels(imageArea.x, imageArea.y, imageArea.width, imageArea.height);
        subTexture.SetPixels(pixels);
        subTexture.Apply();

        // 创建 Sprite
        Rect rect = new Rect(0, 0, subTexture.width, subTexture.height);
        Vector4 border = GuiStyle.BackgroundImageBorder; // 根据需要设置边框宽度
        Sprite sprite = Sprite.Create(MakeColoredTexture(subTexture, GuiStyle.BackgroundColor), rect, new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect, border);

        // 添加背景图，并设置九宫格
        _backgroundImage.sprite = sprite; // 将png图片放在Resources文件夹下
        _backgroundImage.type = Image.Type.Sliced;
    }

    // 在 Group 中添加文本控件
    private void CreateText()
    {
        GameObject textObj = new GameObject("GroupText");
        _groupText = textObj.AddComponent<Text>();
        textObj.transform.SetParent(_rectTransform, false);

        _groupText.text = "这是一个文本控件";
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