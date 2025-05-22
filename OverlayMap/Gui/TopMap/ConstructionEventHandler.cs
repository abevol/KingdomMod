using UnityEngine;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    public class ConstructionEventHandler : MonoBehaviour
    {
        private MapMarker _owner;
        private RectTransform _rectTransform;
        private Color _color;
        private ConstructionBuildingComponent _constructionBuildingComponent;

        public static void Create(MapMarker marker, Color color)
        {
            GameObject obj = new GameObject(nameof(ConstructionEventHandler));
            obj.transform.SetParent(marker.transform, false);
            var comp = obj.AddComponent<ConstructionEventHandler>();
            comp.Init(color);
        }

        private void Init(Color color)
        {
            _rectTransform = this.gameObject.AddComponent<RectTransform>();
            _owner = _rectTransform.parent.GetComponent<MapMarker>();
            _color = color;
        }

        private void Awake()
        {
            _constructionBuildingComponent = this.GetComponent<ConstructionBuildingComponent>();
            if (_constructionBuildingComponent != null)
                _constructionBuildingComponent.OnConstructionComplete += OnConstructionComplete;
        }

        public void OnConstructionComplete()
        {
            _constructionBuildingComponent.OnConstructionComplete -= OnConstructionComplete;

            _owner.UpdateColor(_color);
            var wallLine = this.GetComponent<WallLine>();
            if (wallLine!= null)
                wallLine.UpdateColor(_color);
        }
    }
}