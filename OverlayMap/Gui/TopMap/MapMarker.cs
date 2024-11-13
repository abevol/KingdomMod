using System;
using UnityEngine;
using KingdomMod.OverlayMap.Config;
using static KingdomMod.OverlayMap.OverlayMapHolder;
using TMPro;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
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

        public MapMarkerData Data => _data;

        public void Init()
        {
            _rectTransform = this.gameObject.AddComponent<RectTransform>();
            _owner = _rectTransform.parent.GetComponent<TopMapView>();

            // 设置锚点
            _rectTransform.anchorMin = new Vector2(0.5f, 1); // 以顶部中心为锚点
            _rectTransform.anchorMax = new Vector2(0.5f, 1); // 以顶部中心为锚点
            _rectTransform.pivot = new Vector2(0.5f, 0.5f); // 以顶部中心为支点

            // _icon = this.gameObject.AddComponent<Image>();
            _sign = CreateTextObject("Sign", -10, _owner.Style.SignFont.Font, _owner.Style.SignFontSize);
            _title = CreateTextObject("Title", -26, _owner.Style.TitleFont.Font, _owner.Style.TitleFontSize);
            _count = CreateTextObject("Count", -26 - 16, _owner.Style.CountFont.Font, _owner.Style.CountFontSize);

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
            UpdatePosition();
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
            textObject.transform.SetParent(this.transform);
            var textComponent = textObject.AddComponent<TextMeshProUGUI>();

            // 设置文本的其他属性（如字体、颜色等）
            textComponent.font = font;
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;

            var rectTrans = textObject.GetComponent<RectTransform>();
            rectTrans.anchoredPosition = new Vector2(0, yPos);

            return textComponent;
        }

        // private Text CreateTextObject(string objName, float yPos)
        // {
        //     GameObject textObject = new GameObject(objName);
        //     textObject.transform.SetParent(this.transform);
        //     var textComponent = textObject.AddComponent<Text>();
        //
        //     // 设置文本的其他属性（如字体、颜色等）
        //     textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        //     textComponent.fontSize = 12;
        //     textComponent.color = Color.white;
        //     textComponent.alignment = TextAnchor.MiddleCenter;
        //
        //     var rectTrans = textObject.GetComponent<RectTransform>();
        //     rectTrans.anchoredPosition = new Vector2(0, yPos);
        //
        //     return textComponent;
        // }

        public void SetData(MapMarkerData data)
        {
            _data = data;
            SetRow(_data.Row);
        }

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

        public void OnSignConfigChanged(object sender, EventArgs e)
        {
            UpdateSign(_data.Sign);
        }

        public void OnTitleConfigChanged(object sender, EventArgs e)
        {
            UpdateTitle(_data.Title);
        }

        public void OnColorConfigChanged(object sender, EventArgs e)
        {
            UpdateColor(_data.Color);
        }

        public void UpdateSign(string text)
        {
            _owner.Style.SignFont.TryAddCharacters(text);
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
            _owner.Style.TitleFont.TryAddCharacters(text);
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
            _owner.Style.CountFont.TryAddCharacters(count.ToString());
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
                    (_worldPosX + +SaveDataExtras.MapOffset) * TopMapView.MappingScale * SaveDataExtras.ZoomScale,
                    _rectTransform.anchoredPosition.y
                );
            }
        }
    }
}