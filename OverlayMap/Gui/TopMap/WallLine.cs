#if IL2CPP
using Il2CppInterop.Runtime.Injection;
#endif
using KingdomMod.Shared.Attributes;
using Il2CppInterop.Runtime.Attributes;
using System;
using UnityEngine;
using UnityEngine.UI;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    /// <summary>
    /// UGUI 墙体连接线组件
    /// 使用 Image + RectTransform 绘制连接相邻墙体（或城堡）的线条
    /// </summary>
    [RegisterTypeInIl2Cpp]
    public class WallLine : MonoBehaviour
    {
        private MapMarker _ownerMarker;      // 当前墙的 marker
        private MapMarker _targetMarker;     // 连接的目标 marker（前一个墙或城堡）
        private Image _lineImage;            // 线条的 Image 组件
        private RectTransform _rectTransform;
        private Color _lineColor;
        private float _lastDistance;         // 上次计算的距离，用于优化更新
        private const float LINE_THICKNESS = 2.0f;  // 线条粗细（像素）

        // 暴露属性供外部访问
        public MapMarker OwnerMarker => _ownerMarker;
        public MapMarker TargetMarker => _targetMarker;

#if IL2CPP
        public WallLine(IntPtr ptr) : base(ptr) { }
#endif

        /// <summary>
        /// 创建墙体连接线
        /// </summary>
        /// <param name="topMapView">TopMapView（作为父对象）</param>
        /// <param name="ownerMarker">当前墙的 MapMarker</param>
        /// <param name="targetMarker">连接目标（前一个墙或城堡）</param>
        /// <param name="initialColor">初始颜色</param>
        public static WallLine Create(TopMapView topMapView, MapMarker ownerMarker, MapMarker targetMarker, Color initialColor)
        {
            var obj = new GameObject("WallLine");
            // 重要：WallLine 是 TopMapView 的子对象，而不是 MapMarker 的子对象
            // 这样 anchoredPosition 才能正确使用绝对坐标
            obj.transform.SetParent(topMapView.transform, false);
            var comp = obj.AddComponent<WallLine>();
            comp.Init(ownerMarker, targetMarker, initialColor);
            return comp;
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void Init(MapMarker ownerMarker, MapMarker targetMarker, Color initialColor)
        {
            _ownerMarker = ownerMarker;
            _targetMarker = targetMarker;
            _lineColor = initialColor;

            // 创建 RectTransform
            _rectTransform = this.gameObject.AddComponent<RectTransform>();
            
            // 设置锚点和支点
            // 锚点设置为父元素中心（与 MapMarker 一致）
            _rectTransform.anchorMin = new Vector2(0.5f, 1);
            _rectTransform.anchorMax = new Vector2(0.5f, 1);
            // 支点在线条左侧中间，使其从起点向右延伸
            _rectTransform.pivot = new Vector2(0.0f, 0.5f);
            
            // 创建 Image 组件并设置为纯色填充
            _lineImage = this.gameObject.AddComponent<Image>();
            _lineImage.color = _lineColor;
            
            // 创建一个简单的白色纹理作为 sprite（用于纯色填充）
            // Unity UGUI Image 需要 sprite 才能显示
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            _lineImage.sprite = sprite;
            _lineImage.type = Image.Type.Sliced;  // 使用切片模式以支持拉伸

            // 监听 owner 和 target 的位置变化
            _ownerMarker.OnPositionChanged += OnMarkerPositionChanged;
            _targetMarker.OnPositionChanged += OnMarkerPositionChanged;

            // 初始化位置
            UpdateLinePosition();
        }

        private void OnDestroy()
        {
            // 取消事件监听
            if (_ownerMarker != null)
                _ownerMarker.OnPositionChanged -= OnMarkerPositionChanged;
            if (_targetMarker != null)
                _targetMarker.OnPositionChanged -= OnMarkerPositionChanged;
        }

#if IL2CPP
        [HideFromIl2Cpp]
#endif
        private void OnMarkerPositionChanged(MapMarker marker, Vector2 position)
        {
            // 检查 WallLine 自己是否已被销毁（Unity 延迟销毁机制）
            if (this == null || !this || this.gameObject == null)
            {
                return;
            }
            
            UpdateLinePosition();
        }

        /// <summary>
        /// 更新线条的位置、长度和旋转
        /// </summary>
        public void UpdateLinePosition()
        {
            // 检查 WallLine 自己是否已被销毁
            if (this == null || !this)
            {
                return;
            }
            
            // 检查 GameObject 是否仍然有效
            if (this.gameObject == null || !this.gameObject)
            {
                return;
            }
            
            if (_ownerMarker == null || _targetMarker == null)
            {
                LogError("WallLine: ownerMarker or targetMarker is null");
                return;
            }

            if (_ownerMarker.Data == null || _targetMarker.Data == null)
            {
                LogError("WallLine: marker data is null");
                return;
            }

            // 重要：WallLine 现在是 TopMapView 的子对象，与 MapMarker 同级
            // 直接获取 Marker 的 anchoredPosition（相对于 TopMapView）
            var ownerRect = _ownerMarker.GetComponent<RectTransform>();
            var targetRect = _targetMarker.GetComponent<RectTransform>();
            
            Vector2 startPos = targetRect.anchoredPosition;  // 起点：前一个墙/城堡
            Vector2 endPos = ownerRect.anchoredPosition;     // 终点：当前墙
            Vector2 delta = endPos - startPos;
            float distance = delta.magnitude;

            // 优化：仅在距离变化时更新
            if (Mathf.Abs(distance - _lastDistance) < 0.01f)
                return;

            _lastDistance = distance;

            if (distance < 0.1f)
            {
                // 距离太近，隐藏线条
                LogWarning($"WallLine distance too small: {distance}, hiding line");
                this.gameObject.SetActive(false);
                return;
            }

            this.gameObject.SetActive(true);

            // 设置线条位置：由于 Pivot 是 (0, 0.5)，直接设为起点
            // 向下偏移 2 像素，使线条与墙体标记对齐更好
            _rectTransform.anchoredPosition = new Vector2(startPos.x, startPos.y - 2);

            // 设置线条的长度和宽度
            _rectTransform.sizeDelta = new Vector2(distance, LINE_THICKNESS);

            // 计算旋转角度：从起点指向终点
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            _rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            
            LogDebug($"WallLine updated: startPos={startPos}, endPos={endPos}, distance={distance}, angle={angle}, color={_lineColor}");
        }

        /// <summary>
        /// 更新线条颜色
        /// </summary>
        public void UpdateColor(Color color)
        {
            _lineColor = color;
            if (_lineImage != null)
                _lineImage.color = color;
        }

        /// <summary>
        /// 更新线条可见性
        /// </summary>
        public void UpdateVisible(bool visible)
        {
            this.gameObject.SetActive(visible);
        }
    }
}
