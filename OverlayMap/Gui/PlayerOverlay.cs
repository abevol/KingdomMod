#if IL2CPP
using Il2CppInterop.Runtime.Injection;
#endif
using System;
using KingdomMod.OverlayMap.Gui.TopMap;
using KingdomMod.OverlayMap.Gui.StatsInfo;
using KingdomMod.OverlayMap.Gui.ExtraInfo;
using UnityEngine;
using static KingdomMod.OverlayMap.OverlayMapHolder;
using KingdomMod.Shared.Attributes;

namespace KingdomMod.OverlayMap.Gui
{
#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class PlayerOverlay : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private PlayerId _playerId;
        public TopMapView TopMapView;
        public StatsInfoView StatsInfoView;
        public ExtraInfoView ExtraInfoView;

#if IL2CPP
        public PlayerOverlay(IntPtr ptr) : base(ptr) { }
#endif

        private void Awake()
        {
            LogDebug("PlayerOverlay.Awake");

            _rectTransform = this.gameObject.AddComponent<RectTransform>();
            TopMapView = CreateTopMapView();
            StatsInfoView = CreateStatsInfoView();
            ExtraInfoView = CreateExtraInfoView();

            OverlayMapHolder.OnGameStateChanged += OnGameStateChanged;

            Hide();
        }

        public void Init(PlayerId playerId)
        {
            _playerId = playerId;
            TopMapView.Init(playerId);
            ExtraInfoView.Init(playerId);
        }

        private void OnDestroy()
        {
            OverlayMapHolder.OnGameStateChanged -= OnGameStateChanged;
        }

        private void Start()
        {
            LogDebug("PlayerOverlay.Start");

            UpdateLayout();
        }

        private bool GetPlayerOverlayEnabled(PlayerId playerId)
        {
            var player = Managers.Inst.kingdom.GetPlayer((int)playerId);
            if (player == null) return false;
            if (player.isActiveAndEnabled == false) return false;
            if (player.hasLocalAuthority == false && NetworkBigBoss.IsOnline) return false;
            return true;
        }

        private void UpdateLayout()
        {
            // 设置锚点
            _rectTransform.anchorMin = new Vector2(0, Managers.COOP_ENABLED && _playerId == PlayerId.P1 ? 0.5f : 0); // 左下角
            _rectTransform.anchorMax = new Vector2(1, Managers.COOP_ENABLED && _playerId == PlayerId.P2 ? 0.5f : 1); // 右上角
            _rectTransform.pivot = new Vector2(0.5f, 0.5f); // 以顶部中心为支点

            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            // 更新子组件布局
            StatsInfoView?.UpdateLayout();
            ExtraInfoView?.UpdateLayout();
        }

        public void Show()
        {
            if (!this.enabled)
                return;
            if (Instance.OverlayMapSwitch.CurrentState != OverlayMapHolder.MapSwitchState.NewMap)
                return;

            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /*
         * Called from:
         * Menu.HandleCoopToggle
         * NetworkBigBoss.Server_OnClientCaughtUp
         * NetworkBigBoss.Server_OnClientDisconnected
         */
        public void OnP2StateChanged(Game game, bool joined)
        {
            TopMapView.OnP2StateChanged(game, joined);
            this.enabled = GetPlayerOverlayEnabled(_playerId);
            UpdateLayout();

            if (IsPlaying())
                Show();
        }

        private void OnGameStateChanged(Game.State state)
        {
            LogDebug($"OnGameStateChanged.state changed to {state}");

            switch (state)
            {
                case Game.State.Playing:
                case Game.State.NetworkClientPlaying:
                    this.enabled = GetPlayerOverlayEnabled(_playerId);
                    Show();
                    break;
                case Game.State.Menu:
                case Game.State.SelectingP2Appearance:
                    Hide();
                    break;
                case Game.State.Quitting:
                    Hide();
                    break;
            }
        }

        private TopMapView CreateTopMapView()
        {
            LogDebug($"CreateTopMapView");

            var viewObj = new GameObject(nameof(TopMap.TopMapView));
            viewObj.transform.SetParent(this.transform, false);
            var view = viewObj.AddComponent<TopMapView>();
            return view;
        }

        private void DestroyTopMapView()
        {
            LogDebug($"DestroyTopMapView, _topMapView: {TopMapView}");
            if (TopMapView == null)
                return;

            GameObject.Destroy(TopMapView.gameObject);
            TopMapView = null;
        }

        private StatsInfoView CreateStatsInfoView()
        {
            LogDebug($"CreateStatsInfoView");

            var viewObj = new GameObject(nameof(StatsInfo.StatsInfoView));
            viewObj.transform.SetParent(this.transform, false);
            var view = viewObj.AddComponent<StatsInfoView>();
            return view;
        }

        private ExtraInfoView CreateExtraInfoView()
        {
            LogDebug($"CreateExtraInfoView");

            var viewObj = new GameObject(nameof(ExtraInfo.ExtraInfoView));
            viewObj.transform.SetParent(this.transform, false);
            var view = viewObj.AddComponent<ExtraInfoView>();
            return view;
        }
    }
}
