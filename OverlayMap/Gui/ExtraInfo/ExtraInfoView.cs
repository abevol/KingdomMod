using System;
using KingdomMod.Shared.Attributes;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.ExtraInfo;

/// <summary>
/// 额外信息显示组件 (UGUI)
/// 显示岛屿编号、天数、当前时间、宝石和金币数量
/// </summary>
[RegisterTypeInIl2Cpp]
public class ExtraInfoView : MonoBehaviour
{
    private RectTransform _rectTransform;
    private float _timeSinceLastUpdate;
    private PlayerId _playerId;

    // 文本组件
    private TextMeshProUGUI _landAndDaysText;
    private TextMeshProUGUI _timeText;
    private TextMeshProUGUI _gemsText;
    private TextMeshProUGUI _coinsText;

#if IL2CPP
    public ExtraInfoView(IntPtr ptr) : base(ptr) { }
#endif

    private void Awake()
    {
        LogTrace("ExtraInfoView.Awake");

        _rectTransform = this.gameObject.AddComponent<RectTransform>();

        CreateTextComponents();
        UpdateLayout();
    }

    public void Init(PlayerId playerId)
    {
        _playerId = playerId;
    }

    private void Start()
    {
        LogTrace("ExtraInfoView.Start");
        ApplyStyleConfig();
    }

    /// <summary>
    /// 应用风格配置（字体、字体大小）
    /// 在 Start() 中调用，此时 GuiStyle 已初始化完成
    /// </summary>
    private void ApplyStyleConfig()
    {
        try
        {
            if (Instance == null || Instance.GlobalGuiStyle == null)
            {
                LogWarning("Instance or GlobalGuiStyle is null, using default font");
                return;
            }

            var styleComp = Instance.GlobalGuiStyle.ExtraInfoStyle;
            if (styleComp != null && styleComp.TextFont != null && styleComp.TextFont.Font != null)
            {
                UpdateTextFont(styleComp.TextFont.Font);
                UpdateTextFontSize(styleComp.TextFontSize);
                LogDebug($"Applied ExtraInfo font: {styleComp.TextFont.Font.name}, size: {styleComp.TextFontSize}");
            }
            else
            {
                LogWarning("ExtraInfoStyle or TextFont is null, using default font");
            }
        }
        catch (Exception ex)
        {
            LogError($"Failed to apply style config: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新布局和定位
    /// </summary>
    public void UpdateLayout()
    {
        // 设置锚点：铺满整个父容器
        _rectTransform.anchorMin = Vector2.zero;
        _rectTransform.anchorMax = Vector2.one;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 创建文本组件
    /// </summary>
    private void CreateTextComponents()
    {
        float topOffset = 136;
        if (Managers.COOP_ENABLED)
            topOffset = 136 - 56;

        // 左侧：岛屿和天数
        CreateLandAndDaysText(topOffset);

        // 中间：当前时间
        CreateTimeText(topOffset);

        // 右侧：宝石和金币
        CreateGemsAndCoinsText(topOffset);
    }

    /// <summary>
    /// 创建岛屿和天数文本（左侧）
    /// </summary>
    private void CreateLandAndDaysText(float topOffset)
    {
        var textObj = new GameObject("LandAndDaysText");
        textObj.transform.SetParent(this.transform, false);
        _landAndDaysText = textObj.AddComponent<TextMeshProUGUI>();

        // 配置 RectTransform
        var textRect = _landAndDaysText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(0, 1);
        textRect.pivot = new Vector2(0, 1);
        textRect.anchoredPosition = new Vector2(14, -topOffset);
        textRect.sizeDelta = new Vector2(120, 20);

        // 配置文本样式
        _landAndDaysText.color = Config.MarkerStyle.ExtraInfo.Color;
        _landAndDaysText.alignment = TextAlignmentOptions.TopLeft;
        _landAndDaysText.enableWordWrapping = false;
        _landAndDaysText.overflowMode = TextOverflowModes.Overflow;

        // 使用配置文件中的默认字体大小（字体在 Start() 中加载）
        _landAndDaysText.fontSize = Config.GuiStyle.ExtraInfo.Text.FontSize;
    }

    /// <summary>
    /// 创建时间文本（中间）
    /// </summary>
    private void CreateTimeText(float topOffset)
    {
        var textObj = new GameObject("TimeText");
        textObj.transform.SetParent(this.transform, false);
        _timeText = textObj.AddComponent<TextMeshProUGUI>();

        // 配置 RectTransform
        var textRect = _timeText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1);
        textRect.anchorMax = new Vector2(0.5f, 1);
        textRect.pivot = new Vector2(0, 1);
        textRect.anchoredPosition = new Vector2(-20, -(topOffset + 22));
        textRect.sizeDelta = new Vector2(40, 20);

        // 配置文本样式
        _timeText.color = Config.MarkerStyle.ExtraInfo.Color;
        _timeText.alignment = TextAlignmentOptions.TopLeft;
        _timeText.enableWordWrapping = false;
        _timeText.overflowMode = TextOverflowModes.Overflow;

        // 使用配置文件中的默认字体大小（字体在 Start() 中加载）
        _timeText.fontSize = Config.GuiStyle.ExtraInfo.Text.FontSize;
    }

    /// <summary>
    /// 创建宝石和金币文本（右侧）
    /// </summary>
    private void CreateGemsAndCoinsText(float topOffset)
    {
        // 宝石文本
        var gemsObj = new GameObject("GemsText");
        gemsObj.transform.SetParent(this.transform, false);
        _gemsText = gemsObj.AddComponent<TextMeshProUGUI>();

        var gemsRect = _gemsText.GetComponent<RectTransform>();
        gemsRect.anchorMin = new Vector2(1, 1);
        gemsRect.anchorMax = new Vector2(1, 1);
        gemsRect.pivot = new Vector2(1, 1);
        gemsRect.anchoredPosition = new Vector2(-66, -(136 + 22));
        gemsRect.sizeDelta = new Vector2(60, 20);

        _gemsText.color = Config.MarkerStyle.ExtraInfo.Color;
        _gemsText.alignment = TextAlignmentOptions.TopRight;
        _gemsText.enableWordWrapping = false;
        _gemsText.overflowMode = TextOverflowModes.Overflow;

        // 使用配置文件中的默认字体大小（字体在 Start() 中加载）
        _gemsText.fontSize = Config.GuiStyle.ExtraInfo.Text.FontSize;

        // 金币文本
        var coinsObj = new GameObject("CoinsText");
        coinsObj.transform.SetParent(this.transform, false);
        _coinsText = coinsObj.AddComponent<TextMeshProUGUI>();

        var coinsRect = _coinsText.GetComponent<RectTransform>();
        coinsRect.anchorMin = new Vector2(1, 1);
        coinsRect.anchorMax = new Vector2(1, 1);
        coinsRect.pivot = new Vector2(1, 1);
        coinsRect.anchoredPosition = new Vector2(-6, -(136 + 22));
        coinsRect.sizeDelta = new Vector2(60, 20);

        _coinsText.color = Config.MarkerStyle.ExtraInfo.Color;
        _coinsText.alignment = TextAlignmentOptions.TopRight;
        _coinsText.enableWordWrapping = false;
        _coinsText.overflowMode = TextOverflowModes.Overflow;

        // 使用配置文件中的默认字体大小（字体在 Start() 中加载）
        _coinsText.fontSize = Config.GuiStyle.ExtraInfo.Text.FontSize;
    }

    private void Update()
    {
        _timeSinceLastUpdate += Time.deltaTime;

        if (_timeSinceLastUpdate > (1.0f / Config.Global.GuiUpdatesPerSecond))
        {
            _timeSinceLastUpdate = 0;

            if (!IsPlaying()) return;

            UpdateExtraInfo();
        }
    }

    /// <summary>
    /// 更新额外信息显示
    /// </summary>
    private void UpdateExtraInfo()
    {
        try
        {
            var game = Managers.Inst?.game;
            var director = Managers.Inst?.director;
            var kingdom = Managers.Inst?.kingdom;

            if (game == null || director == null || kingdom == null)
                return;

            // 更新岛屿和天数
            if (_landAndDaysText != null)
            {
                var land = game.currentLand + 1;
                var days = director.CurrentDayForSpawning;
                _landAndDaysText.text = $"{Config.Strings.Land.Value}: {land}  {Config.Strings.Days.Value}: {days}";
            }

            // 更新当前时间
            if (_timeText != null)
            {
                float currentTime = director.currentTime;
                var currentHour = Math.Truncate(currentTime);
                var currentMints = Math.Truncate((currentTime - currentHour) * 60);
                _timeText.text = $"{currentHour:00.}:{currentMints:00.}";
            }

            // 更新宝石和金币（根据玩家 ID 获取）
            var player = kingdom.GetPlayer((int)_playerId);
            if (player != null)
            {
                if (_gemsText != null)
                    _gemsText.text = $"{Config.Strings.Gems.Value}: {player.gems}";

                if (_coinsText != null)
                    _coinsText.text = $"{Config.Strings.Coins.Value}: {player.coins}";
            }
        }
        catch (Exception ex)
        {
            LogError($"Failed to update extra info: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新文本字体
    /// </summary>
    public void UpdateTextFont(TMP_FontAsset font)
    {
        if (font == null) return;

        if (_landAndDaysText != null)
            _landAndDaysText.font = font;

        if (_timeText != null)
            _timeText.font = font;

        if (_gemsText != null)
            _gemsText.font = font;

        if (_coinsText != null)
            _coinsText.font = font;
    }

    /// <summary>
    /// 更新文本字体大小
    /// </summary>
    public void UpdateTextFontSize(float fontSize)
    {
        if (_landAndDaysText != null)
            _landAndDaysText.fontSize = fontSize;

        if (_timeText != null)
            _timeText.fontSize = fontSize;

        if (_gemsText != null)
            _gemsText.fontSize = fontSize;

        if (_coinsText != null)
            _coinsText.fontSize = fontSize;
    }

    /// <summary>
    /// 强制更新文本网格（用于备用字体更改后）
    /// </summary>
    public void ForceTextMeshUpdate()
    {
        if (_landAndDaysText != null)
            _landAndDaysText.ForceMeshUpdate();

        if (_timeText != null)
            _timeText.ForceMeshUpdate();

        if (_gemsText != null)
            _gemsText.ForceMeshUpdate();

        if (_coinsText != null)
            _coinsText.ForceMeshUpdate();
    }
}

