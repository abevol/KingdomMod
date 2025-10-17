#if IL2CPP
using Il2CppInterop.Runtime.Injection;
#endif
using KingdomMod.OverlayMap.Config;
using System;
using System.Collections.Generic;
using KingdomMod.SharedLib;
using UnityEngine;
using UnityEngine.UI;
using static KingdomMod.OverlayMap.OverlayMapHolder;
using KingdomMod.Shared.Attributes;
using Il2CppInterop.Runtime.Attributes;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    [RegisterTypeInIl2Cpp]
    public class WallLine : MonoBehaviour
    {
        public MapMarker Owner;
        private TopMapView _view;
        private Image _line;
        private RectTransform _rectTransform;
        private float _distance;
        private float _timeSinceLastGuiUpdate = 0;
        private LinkedListNode<MapMarker> _node;

#if IL2CPP
        public WallLine(IntPtr ptr) : base(ptr) { }
#endif

        public static WallLine Create(MapMarker wallMarker)
        {
            var obj = new GameObject(nameof(WallLine));
            obj.transform.SetParent(wallMarker.transform, false);
            var comp = obj.AddComponent<WallLine>();

            return comp;
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        public void Init(LinkedListNode<MapMarker> node)
        {
            _rectTransform = this.gameObject.AddComponent<RectTransform>();
            Owner = _rectTransform.parent.GetComponent<MapMarker>();
            _view = _rectTransform.parent.transform.parent.GetComponent<TopMapView>();
            _node = node;

            // 设置锚点
            // _rectTransform.anchorMin = new Vector2(0.0f, 0.5f); // 以父元素中心为锚点
            // _rectTransform.anchorMax = new Vector2(1.0f, 0.5f); // 以父元素中心为锚点
            _rectTransform.pivot = new Vector2(0.0f, 0.5f); // 以自身中心为支点
            _rectTransform.sizeDelta = Vector2.zero;

            _line = CreateLineObject(Color.green.WithAlpha(0.5f));
        }

        private void Awake()
        {

        }

        private void OnDestroy()
        {
        }

        private void Start()
        {
        }

        private void Update()
        {
            _timeSinceLastGuiUpdate += Time.deltaTime;

            if (_timeSinceLastGuiUpdate > (1.0 / Global.GuiUpdatesPerSecond))
            {
                _timeSinceLastGuiUpdate = 0;

                if (!IsPlaying()) return;

                UpdatePosition();

            }
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
            lineRect.pivot = new Vector2(0, 0.5f); // 支点在左侧中间
            lineRect.sizeDelta = new Vector2(distance, thickness);
            lineRect.anchoredPosition = lineStart;
            lineRect.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(lineDelta.y, lineDelta.x) * Mathf.Rad2Deg);
        }

        private Image CreateLineObject(Color color)
        {
            var obj = new GameObject("Line");
            var comp = obj.AddComponent<Image>();

            comp.color = color;
            return comp;
        }

        public void UpdateVisible(bool visible)
        {
            this.gameObject.SetActive(visible);
        }

        public void UpdateColor(Color color)
        {
            _line.color = color;
        }

        public void UpdatePosition(bool force = false)
        {
            // 从中心到两边分布

            if (TopMapView.MappingScale == 0)
            {
                // LogError("TopMapView.MappingScale is 0");
                return;
            }

            // 获取上一个城墙或城堡
            MapMarker previousWall;
            if (_node.Previous != null && _node.Previous.Value != null)
                previousWall = _node.Previous.Value;
            else
                previousWall = _view.CastleMarker; // 如果是第一个城墙，则从城堡开始

            // 计算起点和终点
            var startPosition = previousWall.transform.localPosition;
            var endPosition = Owner.transform.localPosition;

            // 设置 WallLine 的位置和旋转（线条应该始终对齐于 x 轴）
            _rectTransform.localPosition = startPosition;

            // 计算宽度，根据起点和终点的 X 坐标差来设置
            var distance = Mathf.Abs(endPosition.x - startPosition.x);
            if (distance == 0)
                return;

            if (force || Math.Abs(_distance - distance) > 0f)
            {
                _distance = distance;
                _rectTransform.sizeDelta = new Vector2(distance, 2.0f);
                // _rectTransform.anchoredPosition = new Vector2(
                //     (_distance + SaveDataExtras.MapOffset) * TopMapView.MappingScale * SaveDataExtras.ZoomScale,
                //     _rectTransform.anchoredPosition.y
                // );
            }
        }
    }
}