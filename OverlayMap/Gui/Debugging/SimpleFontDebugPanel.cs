using System;
using System.Collections.Generic;
using System.Linq;
using KingdomMod.Shared.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static KingdomMod.OverlayMap.OverlayMapHolder;
using FontData = KingdomMod.OverlayMap.Assets.FontData;
using FontManager = KingdomMod.OverlayMap.Assets.FontManager;
#if IL2CPP
using Il2CppInterop.Runtime.Attributes;
#endif

namespace KingdomMod.OverlayMap.Gui.Debugging
{
    /// <summary>
    /// 简化版字体调试面板，用于测试字体的UI显示效果
    /// </summary>
    [RegisterTypeInIl2Cpp]
    public class SimpleFontDebugPanel : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _panel;
        private bool _isVisible = false;
        private FontData _currentFontData;
        private List<FontData> _availableFonts = new List<FontData>();
        private int _currentFontIndex = 0;
        private float _fontSize = 24f;
        private Color _fontColor = Color.white;
        private TextAlignmentOptions _textAlignment = TextAlignmentOptions.Center;
        private FontStyles _fontStyle = FontStyles.Normal;
        private bool _enableRichText = true;
        private bool _enableWordWrapping = true;
        private float _lineSpacing = 0f;
        private float _characterSpacing = 0f;

        // UI组件引用
        private TextMeshProUGUI _testTextComponent;
        private TextMeshProUGUI _infoTextComponent;
        private int _currentColorIndex = 0;
        private int _currentAlignmentIndex = 0;
        private int _currentStyleIndex = 0;

        /// <summary>
        /// 字体列表配置
        /// </summary>
        private readonly string[] _fontList = new string[]
        {
            "PerfectDOSVGA437",
            "fonts/zpix",
            "fonts/kingdom",
            "fonts/kingdommenu",
            "fonts/notoserifhebrew-medium",
            "fonts/notosanssc-medium",
            "MSYH.TTC",
            "ARIAL.TTF",
            "ARIALUNI.TTF",
            "CALIBRI.TTF",
            "MSGOTHIC.TTC",
            "SEGOEUI.TTF",
            "SEGUISYM.TTF",
            "TIMES.TTF",
            "WINGDING.TTF"
        };

        private readonly string _testText = """
                                            字体测试文本 Font Test Text 1234567890 !@#$%^&*()
                                            城堡：♜
                                            城墙：۩
                                            浆果：♣
                                            河流：≈
                                            土堆：∧
                                            """;

        private readonly Color[] _colors = { Color.white, Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
        private readonly TextAlignmentOptions[] _alignments = { 
            TextAlignmentOptions.Left, TextAlignmentOptions.Center, TextAlignmentOptions.Right,
            TextAlignmentOptions.TopLeft, TextAlignmentOptions.Top, TextAlignmentOptions.TopRight,
            TextAlignmentOptions.Left, TextAlignmentOptions.Center, TextAlignmentOptions.Right,
            TextAlignmentOptions.BottomLeft, TextAlignmentOptions.Bottom, TextAlignmentOptions.BottomRight
        };
        private readonly FontStyles[] _styles = { FontStyles.Normal, FontStyles.Bold, FontStyles.Italic, FontStyles.Bold | FontStyles.Italic };

        /// <summary>
        /// 初始化字体调试面板
        /// </summary>
        public void Initialize(Canvas parentCanvas)
        {
            _canvas = parentCanvas;
            CreatePanel();
            LoadAvailableFonts();
            UpdateFontDisplay();
            UpdateInfoDisplay();
        }

        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            if (_panel)
            {
                _panel.SetActive(_isVisible);
            }
        }

        /// <summary>
        /// 设置面板可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            if (_panel)
            {
                _panel.SetActive(_isVisible);
            }
        }

        /// <summary>
        /// 创建面板UI
        /// </summary>
        private void CreatePanel()
        {
            // 创建主面板
            _panel = new GameObject("SimpleFontDebugPanel");
            _panel.transform.SetParent(_canvas.transform, false);
            _panel.SetActive(false);

            // 添加背景
            var background = _panel.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.8f);

            // 设置面板位置和大小
            var rectTransform = _panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
            rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // 创建测试文本显示区域
            CreateTestTextDisplay();
            
            // 创建信息显示区域
            CreateInfoDisplay();
        }

        /// <summary>
        /// 创建测试文本显示区域
        /// </summary>
        private void CreateTestTextDisplay()
        {
            var textObj = new GameObject("TestTextDisplay");
            textObj.transform.SetParent(_panel.transform, false);

            // 添加背景
            var backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(textObj.transform, false);
            backgroundObj.transform.SetAsFirstSibling();

            var backgroundImage = backgroundObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            var backgroundRect = backgroundImage.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            // 添加文本组件
            _testTextComponent = textObj.AddComponent<TextMeshProUGUI>();
            _testTextComponent.text = _testText;
            _testTextComponent.fontSize = _fontSize;
            _testTextComponent.color = _fontColor;
            _testTextComponent.alignment = _textAlignment;
            _testTextComponent.fontStyle = _fontStyle;
            _testTextComponent.richText = _enableRichText;
            _testTextComponent.enableWordWrapping = _enableWordWrapping;
            _testTextComponent.lineSpacing = _lineSpacing;
            _testTextComponent.characterSpacing = _characterSpacing;

            var textRect = _testTextComponent.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.3f);
            textRect.anchorMax = new Vector2(0.95f, 0.95f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 创建信息显示区域
        /// </summary>
        private void CreateInfoDisplay()
        {
            var infoObj = new GameObject("InfoDisplay");
            infoObj.transform.SetParent(_panel.transform, false);

            _infoTextComponent = infoObj.AddComponent<TextMeshProUGUI>();
            _infoTextComponent.fontSize = 16;
            _infoTextComponent.color = Color.white;
            _infoTextComponent.alignment = TextAlignmentOptions.TopLeft;
            _infoTextComponent.enableWordWrapping = true;

            // 设置字体（如果有当前字体的话）
            if (_currentFontData?.Font)
            {
                _infoTextComponent.font = _currentFontData.Font;
            }

            var infoRect = _infoTextComponent.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.05f, 0.05f);
            infoRect.anchorMax = new Vector2(0.95f, 0.25f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;

            UpdateInfoDisplay();
        }

        /// <summary>
        /// 更新信息显示
        /// </summary>
        private void UpdateInfoDisplay()
        {
            if (!_infoTextComponent) return;

            // 确保信息文本组件使用正确的字体
            if (_currentFontData?.Font)
            {
                _infoTextComponent.font = _currentFontData.Font;
            }

            var fontName = _currentFontData?.FontName ?? "无字体";
            var colorName = GetColorName(_fontColor);
            var alignmentName = GetAlignmentName(_textAlignment);
            var styleName = GetStyleName(_fontStyle);

            var availableFontsText = string.Join("\n", _availableFonts
                .Select((fontData, index) =>
                {
                    // 安全地访问字体名称，防止因字体资源被销毁而引发异常
                    var familyName = (fontData != null && fontData.Font) ? fontData.Font.faceInfo.familyName : " (Destroyed)";
                    return $"{index + 1}. {familyName}";
                }));

            var info = $"字体调试面板 - Font Debug Panel\n" +
                       $"热键: Ctrl+T 切换显示 | 数字键1/2切换字体 | 方向键调整参数\n" +
                       $"当前字体: {fontName} | 大小: {_fontSize:F1} | 颜色: {colorName}\n" +
                       $"对齐: {alignmentName} | 样式: {styleName} | 富文本: {(_enableRichText ? "开" : "关")}\n" +
                       $"换行: {(_enableWordWrapping ? "开" : "关")} | 行间距: {_lineSpacing:F1} | 字符间距: {_characterSpacing:F1}\n" +
                       $"可用字体列表:\n{availableFontsText}";

            _infoTextComponent.text = info;
        }

        /// <summary>
        /// 获取颜色名称
        /// </summary>
        private string GetColorName(Color color)
        {
            for (int i = 0; i < _colors.Length; i++)
            {
                if (Mathf.Approximately(color.r, _colors[i].r) && 
                    Mathf.Approximately(color.g, _colors[i].g) && 
                    Mathf.Approximately(color.b, _colors[i].b))
                {
                    return new string[] { "白色", "红色", "绿色", "蓝色", "黄色", "青色", "洋红" }[i];
                }
            }
            return "自定义";
        }

        /// <summary>
        /// 获取对齐方式名称
        /// </summary>
        private string GetAlignmentName(TextAlignmentOptions alignment)
        {
            var names = new string[] { "左", "中", "右", "左上", "上", "右上", "左中", "中", "右中", "左下", "下", "右下" };
            var index = Array.IndexOf(_alignments, alignment);
            return index >= 0 ? names[index] : "未知";
        }

        /// <summary>
        /// 获取样式名称
        /// </summary>
        private string GetStyleName(FontStyles style)
        {
            var names = new string[] { "普通", "粗体", "斜体", "粗斜体" };
            var index = Array.IndexOf(_styles, style);
            return index >= 0 ? names[index] : "未知";
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        private void Update()
        {
            if (!_isVisible || !_panel || !_panel.activeInHierarchy) return;

            // 数字键1切换上一个字体，数字键2切换下一个字体
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (_availableFonts.Count > 0)
                {
                    _currentFontIndex = (_currentFontIndex - 1 + _availableFonts.Count) % _availableFonts.Count;
                    _currentFontData = _availableFonts[_currentFontIndex];
                    LogTrace($"_currentFontIndex: {_currentFontIndex}, _availableFonts.Count: {_availableFonts.Count}");
                    LogTrace($"_availableFonts[{_currentFontIndex}]: {_availableFonts[_currentFontIndex]?.ToString() ?? "null"}, Font: {_availableFonts[_currentFontIndex].Font.faceInfo.familyName}");

                    // 确保当前字体包含中文字符
                    AddChineseCharactersToFont(_currentFontData);
                    
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (_availableFonts.Count > 0)
                {
                    _currentFontIndex = (_currentFontIndex + 1) % _availableFonts.Count;
                    _currentFontData = _availableFonts[_currentFontIndex];
                    LogTrace($"_currentFontIndex: {_currentFontIndex}, _availableFonts.Count: {_availableFonts.Count}");
                    LogTrace($"_availableFonts[{_currentFontIndex}]: {_availableFonts[_currentFontIndex]?.ToString() ?? "null"}, Font: {_availableFonts[_currentFontIndex].Font.faceInfo.familyName}");

                    // 确保当前字体包含中文字符
                    AddChineseCharactersToFont(_currentFontData);
                    
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
            }


            // 方向键调整参数
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    _fontSize = Mathf.Clamp(_fontSize + 2f, 8f, 72f);
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    _fontSize = Mathf.Clamp(_fontSize - 2f, 8f, 72f);
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    _characterSpacing = Mathf.Clamp(_characterSpacing - 5f, -50f, 100f);
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    _characterSpacing = Mathf.Clamp(_characterSpacing + 5f, -50f, 100f);
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
            }
            else
            {
                // 普通方向键调整其他参数
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    _lineSpacing = Mathf.Clamp(_lineSpacing + 5f, -50f, 100f);
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    _lineSpacing = Mathf.Clamp(_lineSpacing - 5f, -50f, 100f);
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    _currentColorIndex = (_currentColorIndex - 1 + _colors.Length) % _colors.Length;
                    _fontColor = _colors[_currentColorIndex];
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    _currentColorIndex = (_currentColorIndex + 1) % _colors.Length;
                    _fontColor = _colors[_currentColorIndex];
                    UpdateFontDisplay();
                    UpdateInfoDisplay();
                }
            }

            // 其他快捷键
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _currentAlignmentIndex = (_currentAlignmentIndex + 1) % _alignments.Length;
                _textAlignment = _alignments[_currentAlignmentIndex];
                UpdateFontDisplay();
                UpdateInfoDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                _currentStyleIndex = (_currentStyleIndex + 1) % _styles.Length;
                _fontStyle = _styles[_currentStyleIndex];
                UpdateFontDisplay();
                UpdateInfoDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                _enableRichText = !_enableRichText;
                UpdateFontDisplay();
                UpdateInfoDisplay();
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                _enableWordWrapping = !_enableWordWrapping;
                UpdateFontDisplay();
                UpdateInfoDisplay();
            }
        }

        /// <summary>
        /// 加载可用字体
        /// </summary>
        private void LoadAvailableFonts()
        {
            _availableFonts.Clear();

            // 使用字体列表配置
            foreach (var fontName in _fontList)
            {
                try
                {
                    var fontData = FontManager.CreateMainFont(fontName, null);
                    if (fontData != null)
                    {
                        // 添加中文字符到字体
                        AddChineseCharactersToFont(fontData);
                        _availableFonts.Add(fontData);
                        LogTrace($"Loaded font: {fontName}, {fontData.Font?.faceInfo?.familyName ?? "null"}");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to load font {fontName}: {ex.Message}");
                }
            }

            if (_availableFonts.Count > 0)
            {
                _currentFontData = _availableFonts[0];
                _currentFontIndex = 0;
            }
        }

        /// <summary>
        /// 向字体添加中文字符
        /// </summary>
#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void AddChineseCharactersToFont(FontData fontData)
        {
            if (fontData == null) return;

            // 添加测试文本中的所有字符
            fontData.TryAddCharacters(_testText);
            
            // 添加信息显示中可能用到的中文字符
            var infoChars = "列表字体调试面板热键切换显示数字键切换字体方向键调整参数当前字体大小颜色对齐样式富文本换行行间距字符间距白色红色绿色蓝色黄色青色洋红左中右左上上右上左中中右中左下下右下普通粗体斜体粗斜体开关未知自定义";
            fontData.TryAddCharacters(infoChars);
            
            // 添加一些常用的中文字符
            var commonChineseChars = "的一是了我不人在他有这个上们来到时大地为子中你说生国年着就那和要她出也得里后自以会家可下而过天去能对小多然于心学么之都好看起发当没成只如事把还用第样道想作种开美总从无情面最女但现前些所同日手又行意动方期它头经长儿回位分爱老因很给名法间斯知世什两次使身者被高已亲其进此话常与活正感";
            fontData.TryAddCharacters(commonChineseChars);
            
            LogTrace($"Added Chinese characters to font: {fontData.FontName}");
        }

        /// <summary>
        /// 更新字体显示
        /// </summary>
        private void UpdateFontDisplay()
        {
            if (!_testTextComponent)
            {
                LogTrace("UpdateFontDisplay, _testTextComponent is null or destroyed.");
                return;
            }

            if (_currentFontData == null)
            {
                LogTrace("UpdateFontDisplay, _currentFontData is null.");
                // Even if font data is null, we should update the text properties
            }
            else
            {
                // --- 诊断代码 ---
                var font = _currentFontData.Font;
                bool isCSharpNull = font == null;
                // isUnityNull 使用 UnityEngine.Object 重载的布尔运算符来检查对象是否已在引擎端被销毁
                bool isUnityNull = !font; 
                LogTrace($"UpdateFontDisplay Font Check. Font Name: '{_currentFontData.FontName}', C# null: {isCSharpNull}, Unity destroyed: {isUnityNull}");
                // --- 诊断结束 ---

                if (!isUnityNull)
                {
                    LogTrace($"UpdateFontDisplay, Before setting font, _testTextComponent.font: {_testTextComponent.font?.faceInfo.familyName ?? "null"}");
                    _testTextComponent.font = font;
                    LogTrace($"UpdateFontDisplay, After setting font, _testTextComponent.font: {_testTextComponent.font.faceInfo.familyName}");

                    // 同时更新信息文本组件的字体
                    if (_infoTextComponent)
                    {
                        LogTrace($"UpdateFontDisplay, Before setting font, _infoTextComponent.font: {_infoTextComponent.font?.faceInfo.familyName ?? "null"}");
                        _infoTextComponent.font = font;
                        LogTrace($"UpdateFontDisplay, After setting font, _infoTextComponent.font: {_infoTextComponent.font.faceInfo.familyName}");
                    }
                }
                else
                {
                    LogWarning($"UpdateFontDisplay, SKIPPING font assignment because font asset '{_currentFontData.FontName}' is destroyed.");
                }
            }

            // LogTrace($"UpdateFontDisplay, StackTrace: {System.Environment.StackTrace}");
            _testTextComponent.text = _testText;
            _testTextComponent.fontSize = _fontSize;
            _testTextComponent.color = _fontColor;
            _testTextComponent.alignment = _textAlignment;
            _testTextComponent.fontStyle = _fontStyle;
            _testTextComponent.richText = _enableRichText;
            _testTextComponent.enableWordWrapping = _enableWordWrapping;
            _testTextComponent.lineSpacing = _lineSpacing;
            _testTextComponent.characterSpacing = _characterSpacing;
        }
    }
}
