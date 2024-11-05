using System;
using UnityEngine;
using KingdomMod.OverlayMap.Config.Extensions;
using static KingdomMod.OverlayMap.OverlayMapHolder;

namespace KingdomMod.OverlayMap.Gui.TopMap
{
    public delegate ConfigEntryWrapper<string> ColorUpdaterFn(Component component);

    public delegate int CountUpdaterFn(Component component);

    public delegate bool VisibleUpdaterFn(Component component);

    public enum MarkerRow
    {
        Settled = 0,
        Movable = 1
    }

    public class MapMarkerData : IDisposable
    {
        private bool _disposed = false;
        private ConfigEntryWrapper<string> _color;
        private ConfigEntryWrapper<string> _sign;
        private ConfigEntryWrapper<string> _title;
        private ConfigEntryWrapper<string> _icon;

        private int _count;
        private bool _visible;

        public TopMapView Owner;
        public MapMarker Self;
        public Component Target;
        public MarkerRow Row;

        public ConfigEntryWrapper<string> Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    if (_color != null)
                    {
                        LogMessage($"old color: {_color.Value}, PlayerId: {Owner.PlayerId}, Target: {Target.name}");
                        _color.Entry.SettingChanged -= Self.OnColorConfigChanged;
                    }

                    _color = value;
                    if (_color != null)
                    {
                        _color.Entry.SettingChanged += Self.OnColorConfigChanged;
                        Self.UpdateColor(_color);
                        LogMessage($"update color: {_color.Value}, PlayerId: {Owner.PlayerId}, Target: {Target.name}");
                    }
                }
            }
        }

        public ConfigEntryWrapper<string> Sign
        {
            get => _sign;
            set
            {
                if (_sign != value)
                {
                    if (_sign != null)
                        _sign.Entry.SettingChanged -= Self.OnSignConfigChanged;

                    _sign = value;
                    if (_sign != null)
                    {
                        _sign.Entry.SettingChanged += Self.OnSignConfigChanged;
                        Self.UpdateSign(_sign);
                    }
                }
            }
        }

        public ConfigEntryWrapper<string> Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    if (_title != null)
                        _title.Entry.SettingChanged -= Self.OnTitleConfigChanged;

                    _title = value;
                    if (_title != null)
                    {
                        _title.Entry.SettingChanged += Self.OnTitleConfigChanged;
                        Self.UpdateTitle(_title);
                    }
                }
            }
        }

        public ConfigEntryWrapper<string> Icon;

        public int Count
        {
            get => _count;
            set
            {
                if (_count != value)
                {
                    _count = value;
                    Self.UpdateCount(_count);
                }
            }
        }

        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    Self.UpdateVisible(_visible);
                }
            }
        }

        public ColorUpdaterFn ColorUpdater;
        public CountUpdaterFn CountUpdater;
        public VisibleUpdaterFn VisibleUpdater;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            LogMessage($"Disposing MapMarkerData for {Title.Value}, disposing: {disposing}");
            if (!_disposed)
            {
                if (disposing)
                {
                    // 清理托管资源
                    if (_color != null)
                        _color.Entry.SettingChanged -= Self.OnColorConfigChanged;
                    if (_sign != null)
                        _sign.Entry.SettingChanged -= Self.OnSignConfigChanged;
                    if (_title != null)
                        _title.Entry.SettingChanged -= Self.OnTitleConfigChanged;
                }

                // 清理非托管资源

                _disposed = true;
            }
        }

        ~MapMarkerData()
        {
            Dispose(false);
        }
    }
}