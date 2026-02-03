using Il2CppInterop.Runtime.Attributes;
using KingdomMod.OverlayMap.Config;
using KingdomMod.Shared.Attributes;
using System;
using TMPro;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    [RegisterTypeInIl2Cpp]
    public class MapMarker : MonoBehaviour
    {
        private TopMapView _owner;
        private RectTransform _rectTransform;
        private float _worldPosX;
        // private Image _icon;
        private TextMeshProUGUI _sign;
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _count;
        private MapMarkerData _data;
        private float _timeSinceLastGuiUpdate = 0;

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        public MapMarkerData Data => _data;

        public delegate void PositionEventHandler(MapMarker mapMarker, Vector2 position);

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        public event PositionEventHandler OnPositionChanged;

#if IL2CPP
        public MapMarker(IntPtr ptr) : base(ptr) { }
#endif

        public void Init()
        {
            _rectTransform = this.gameObject.AddComponent<RectTransform>();
            _owner = _rectTransform.parent.GetComponent<TopMapView>();
            var style = Instance.guiStyle.topMapStyle;

            // 设置锚点
            _rectTransform.anchorMin = new Vector2(0.5f, 1); // 以顶部中心为锚点
            _rectTransform.anchorMax = new Vector2(0.5f, 1); // 以顶部中心为锚点
            _rectTransform.pivot = new Vector2(0.5f, 0.5f); // 以顶部中心为支点

            // _icon = this.gameObject.AddComponent<Image>();
            _sign = CreateTextObject("Sign", -10, style.SignFont.Font, style.SignFontSize);
            _title = CreateTextObject("Title", -26, style.TitleFont.Font, style.TitleFontSize);
            _count = CreateTextObject("Count", -26 - 16, style.CountFont.Font, style.CountFontSize);
        }

        private void Awake()
        {

        }

        private void OnDestroy()
        {
            _data?.Dispose();
        }

        private void Start()
        {
            // UpdatePosition();
        }

        private void Update()
        {
            _timeSinceLastGuiUpdate += Time.deltaTime;

            if (_timeSinceLastGuiUpdate > (1.0 / Global.GuiUpdatesPerSecond))
            {
                _timeSinceLastGuiUpdate = 0;

                if (!IsPlaying()) return;

                UpdatePosition();

                if (_data.ColorUpdater != null)
                    _data.Color = _data.ColorUpdater(_data.Target);
                if (_data.CountUpdater != null)
                    _data.Count = _data.CountUpdater(_data.Target);
            }
        }

        private TextMeshProUGUI CreateTextObject(string objName, float yPos, TMP_FontAsset font, float fontSize)
        {
            GameObject textObject = new GameObject(objName);
            var textComponent = textObject.AddComponent<TextMeshProUGUI>();
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(_rectTransform, false);

            // 设置文本的其他属性（如字体、颜色等）
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;

            var rectTrans = textObject.GetComponent<RectTransform>();
            rectTrans.anchoredPosition = new Vector2(0, yPos);

            return textComponent;
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        public void SetData(MapMarkerData data)
        {
            _data = data;
            SetRow(_data.Row);
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void SetRow(MarkerRow row)
        {
            float titlePosY = _data.Row switch
            {
                MarkerRow.Settled => -26,
                MarkerRow.Movable => -58,
                _ => throw new ArgumentOutOfRangeException()
            };

            _title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, titlePosY);
            _count.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, titlePosY - 16);
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        public void OnSignConfigChanged(object sender, EventArgs e)
        {
            UpdateSign(_data.Sign);
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        public void OnTitleConfigChanged(object sender, EventArgs e)
        {
            UpdateTitle(_data.Title);
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        public void OnColorConfigChanged(object sender, EventArgs e)
        {
            UpdateColor(_data.Color);
        }

        public void UpdateSign(string text)
        {
            Instance.guiStyle.topMapStyle.SignFont.TryAddCharacters(text);
            _sign.text = text;
        }

        public void UpdateSignFont(TMP_FontAsset font)
        {
            _sign.font = font;
        }

        public void UpdateSignFontSize(float fontSize)
        {
            _sign.fontSize = fontSize;
        }

        public void ForceSignMeshUpdate()
        {
            _sign.ForceMeshUpdate();
        }

        public void UpdateTitle(string text)
        {
            Instance.guiStyle.topMapStyle.TitleFont.TryAddCharacters(text);
            _title.text = text;
        }

        public void UpdateTitleFont(TMP_FontAsset font)
        {
            _title.font = font;
        }

        public void UpdateTitleFontSize(float fontSize)
        {
            _title.fontSize = fontSize;
        }

        public void ForceTitleMeshUpdate()
        {
            _title.ForceMeshUpdate();
        }

        public void UpdateCount(int count)
        {
            Instance.guiStyle.topMapStyle.CountFont.TryAddCharacters(count.ToString());
            _count.text = count == 0 ? "" : count.ToString();
        }

        public void UpdateCountFont(TMP_FontAsset font)
        {
            _count.font = font;
        }

        public void UpdateCountFontSize(float fontSize)
        {
            _count.fontSize = fontSize;
        }

        public void ForceCountMeshUpdate()
        {
            _count.ForceMeshUpdate();
        }

        public void UpdateVisible(bool visible)
        {
            this.gameObject.SetActive(visible);
        }

        public void UpdateColor(Color color)
        {
            _sign.color = color;
            _title.color = color;
            _count.color = color;
        }

        public void UpdatePosition(bool force = false)
        {
            // 从中心到两边分布

            if (_data == null)
            {
                LogError("_data is null");
                return;
            }

            if (_data.Target == null)
            {
                LogError("_data.Target is null");
                return;
            }

            if (TopMapView.MappingScale == 0)
            {
                // LogError("TopMapView.MappingScale is 0");
                return;
            }

            if (force || Math.Abs(_worldPosX - _data.Target.transform.position.x) > 0f)
            {
                _worldPosX = _data.Target.transform.position.x;
                _rectTransform.anchoredPosition = new Vector2(
                    (_worldPosX + SaveDataExtras.MapOffset) * TopMapView.MappingScale * SaveDataExtras.ZoomScale,
                    _rectTransform.anchoredPosition.y
                );

                OnPositionChanged?.Invoke(this, _rectTransform.anchoredPosition);
            }
        }
    }
}