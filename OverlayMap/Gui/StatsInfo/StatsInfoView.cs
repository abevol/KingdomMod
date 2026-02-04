using System;
using System.Collections.Generic;
using System.IO;
using KingdomMod.Shared.Attributes;
using KingdomMod.SharedLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.StatsInfo;

/// <summary>
/// 统计信息面板视图组件 (UGUI)
/// 显示游戏统计信息：农民、工人、弓箭手、骑士、农夫、农田等数量
/// </summary>
[RegisterTypeInIl2Cpp]
public class StatsInfoView : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Image _backgroundImage;
    private TextMeshProUGUI _statsText;
    private float _timeSinceLastUpdate;

    // 统计数据缓存
    private StatsData _statsData = new StatsData();

#if IL2CPP
    public StatsInfoView(IntPtr ptr) : base(ptr) { }
#endif

    private void Awake()
    {
        LogTrace("StatsInfoView.Awake");

        // 创建 RectTransform
        _rectTransform = this.gameObject.AddComponent<RectTransform>();

        // 创建背景 Image
        _backgroundImage = this.gameObject.AddComponent<Image>();

        // 创建文本组件
        CreateTextComponent();

        UpdateLayout();
        UpdateBackgroundImage();
    }

    private void Start()
    {
        LogTrace("StatsInfoView.Start");
    }

    /// <summary>
    /// 更新布局和定位
    /// </summary>
    public void UpdateLayout()
    {
        // 设置锚点：左上角
        _rectTransform.anchorMin = new Vector2(0, 1);
        _rectTransform.anchorMax = new Vector2(0, 1);
        _rectTransform.pivot = new Vector2(0, 1);

        // 计算顶部偏移（TopMapView 下方）
        float topOffset = 160;
        if (Managers.COOP_ENABLED)
            topOffset = 160 - 56;

        // 设置尺寸和位置
        float width = 120;
        float height = 146;
        _rectTransform.anchoredPosition = new Vector2(5, -topOffset);
        _rectTransform.sizeDelta = new Vector2(width, height);
    }

    /// <summary>
    /// 创建文本组件
    /// </summary>
    private void CreateTextComponent()
    {
        var textObj = new GameObject("StatsText");
        textObj.transform.SetParent(this.transform, false);
        _statsText = textObj.AddComponent<TextMeshProUGUI>();

        // 配置 RectTransform
        var textRect = _statsText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(9, 5);
        textRect.offsetMax = new Vector2(-5, -5);

        // 配置文本样式
        _statsText.fontSize = 14;
        _statsText.color = Config.MarkerStyle.StatsInfo.Color;
        _statsText.alignment = TextAlignmentOptions.TopLeft;
        _statsText.enableWordWrapping = false;
        _statsText.overflowMode = TextOverflowModes.Overflow;
    }

    /// <summary>
    /// 更新背景图像（参考 TopMapView.UpdateBackgroundImage）
    /// </summary>
    private void UpdateBackgroundImage()
    {
        try
        {
            // 读取 PNG 文件
            var bgImageFile = Path.Combine(AssetsDir, Config.GuiStyle.TopMap.BackgroundImageFile);
            if (!File.Exists(bgImageFile))
            {
                LogError($"Background image file not found: {bgImageFile}");
                return;
            }

            byte[] fileData = File.ReadAllBytes(bgImageFile);

            // 创建 Texture2D 并加载数据
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            // 提取九宫格区域
            var imageArea = (RectInt)Config.GuiStyle.TopMap.BackgroundImageArea;
            imageArea.y = texture.height - imageArea.y - imageArea.height;
            Texture2D subTexture = new Texture2D(imageArea.width, imageArea.height);
            Color[] pixels = texture.GetPixels(imageArea.x, imageArea.y, imageArea.width, imageArea.height);
            subTexture.SetPixels(pixels);
            subTexture.Apply();

            // 创建 Sprite
            Rect rect = new Rect(0, 0, subTexture.width, subTexture.height);
            Vector4 border = Config.GuiStyle.TopMap.BackgroundImageBorder;
            Sprite sprite = Sprite.Create(
                MakeColoredTexture(subTexture, Config.GuiStyle.TopMap.BackgroundColor),
                rect,
                new Vector2(0.5f, 0.5f),
                100,
                1,
                SpriteMeshType.FullRect,
                border
            );

            // 设置背景图
            _backgroundImage.sprite = sprite;
            _backgroundImage.type = Image.Type.Sliced;
        }
        catch (Exception ex)
        {
            LogError($"Failed to update background image: {ex.Message}");
        }
    }

    private void Update()
    {
        _timeSinceLastUpdate += Time.deltaTime;

        if (_timeSinceLastUpdate > (1.0f / Config.Global.GuiUpdatesPerSecond))
        {
            _timeSinceLastUpdate = 0;

            if (!IsPlaying()) return;

            UpdateStatsData();
            UpdateStatsText();
        }
    }

    /// <summary>
    /// 更新统计数据（从游戏对象中收集）
    /// </summary>
    private void UpdateStatsData()
    {
        var kingdom = Managers.Inst?.kingdom;
        if (kingdom == null) return;

        try
        {
            // 农民数量
            var peasantList = GameObject.FindGameObjectsWithTag(Tags.Peasant);
            _statsData.PeasantCount = peasantList?.Length ?? 0;

            // 工人数量
            _statsData.WorkerCount = kingdom._workers?.Count ?? 0;

            // 弓箭手数量
            _statsData.ArcherCount = kingdom._archers?.Count ?? 0;
            _statsData.ArcherFreeCount = GameExtensions.GetArcherCount(GameExtensions.ArcherType.Free);
            _statsData.ArcherGuardCount = GameExtensions.GetArcherCount(GameExtensions.ArcherType.GuardSlot);
            _statsData.ArcherKnightCount = GameExtensions.GetArcherCount(GameExtensions.ArcherType.KnightSoldier);

            // 长矛兵数量
            _statsData.PikemanCount = kingdom.Pikemen?.Count ?? 0;

            // 骑士数量
            _statsData.KnightCount = kingdom.Knights?.Count ?? 0;
            _statsData.KnightMountedCount = GameExtensions.GetKnightCount(true);

            // 农夫数量
            _statsData.FarmerCount = kingdom.Farmers?.Count ?? 0;

            // 农田数量
            int maxFarmlands = 0;
            var farmhouseList = kingdom.GetFarmHouses();
            if (farmhouseList != null)
            {
                foreach (var farmhouse in farmhouseList)
                {
                    if (farmhouse != null)
                        maxFarmlands += farmhouse.CurrentMaxFarmlands();
                }
            }
            _statsData.MaxFarmlands = maxFarmlands;

            // 调试信息
            _statsData.ZoomScale = Config.SaveDataExtras.ZoomScale.Value;
            _statsData.MapOffset = Config.SaveDataExtras.MapOffset.Value;
            _statsData.BorderLeft = kingdom.GetBorderSide(Side.Left);
            _statsData.BorderRight = kingdom.GetBorderSide(Side.Right);
            _statsData.BorderLeftIntact = kingdom.GetBorderSideIntact(Side.Left);
            _statsData.BorderRightIntact = kingdom.GetBorderSideIntact(Side.Right);
        }
        catch (Exception ex)
        {
            LogError($"Failed to update stats data: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新统计文本显示
    /// </summary>
    private void UpdateStatsText()
    {
        if (_statsText == null) return;

        try
        {
            var lines = new List<string>
            {
                $"{Config.Strings.Peasant.Value}: {_statsData.PeasantCount}",
                $"{Config.Strings.Worker.Value}: {_statsData.WorkerCount}",
                $"{Config.Strings.Archer.Value}: {_statsData.ArcherCount} ({_statsData.ArcherFreeCount}|{_statsData.ArcherGuardCount}|{_statsData.ArcherKnightCount})",
                $"{Config.Strings.Pikeman.Value}: {_statsData.PikemanCount}",
                $"{Config.Strings.Knight.Value}: {_statsData.KnightCount} ({_statsData.KnightMountedCount})",
                $"{Config.Strings.Farmer.Value}: {_statsData.FarmerCount}",
                $"{Config.Strings.Farmlands.Value}: {_statsData.MaxFarmlands}",
                $"ZoomScale: {_statsData.ZoomScale:F2}",
                $"MapOffset: {_statsData.MapOffset:F0}",
                $"BorderSide: {_statsData.BorderLeft:F0}, {_statsData.BorderRight:F0}",
                $"BorderSideIntact: {_statsData.BorderLeftIntact:F0}, {_statsData.BorderRightIntact:F0}"
            };

            _statsText.text = string.Join("\n", lines);
        }
        catch (Exception ex)
        {
            LogError($"Failed to update stats text: {ex.Message}");
        }
    }

    /// <summary>
    /// 统计数据结构
    /// </summary>
    private class StatsData
    {
        public int PeasantCount;
        public int WorkerCount;
        public int ArcherCount;
        public int ArcherFreeCount;
        public int ArcherGuardCount;
        public int ArcherKnightCount;
        public int PikemanCount;
        public int KnightCount;
        public int KnightMountedCount;
        public int FarmerCount;
        public int MaxFarmlands;
        public float ZoomScale;
        public float MapOffset;
        public float BorderLeft;
        public float BorderRight;
        public float BorderLeftIntact;
        public float BorderRightIntact;
    }
}
