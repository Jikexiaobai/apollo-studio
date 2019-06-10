﻿using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using Apollo.Components;
using Apollo.Core;
using Apollo.Devices;
using Apollo.Elements;

namespace Apollo.DeviceViewers {
    public class KeyFilterViewer: UserControl {
        public static readonly string DeviceIdentifier = "keyfilter";

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
        
        KeyFilter _filter;
        LaunchpadGrid Grid;

        private SolidColorBrush GetColor(bool value) => (SolidColorBrush)Application.Current.Styles.FindResource(value? "ThemeAccentBrush" : "ThemeForegroundLowBrush");

        public KeyFilterViewer(KeyFilter filter) {
            InitializeComponent();

            _filter = filter;

            Grid = this.Get<LaunchpadGrid>("Grid");

            for (int i = 0; i < 100; i++)
                Grid.SetColor(LaunchpadGrid.SignalToGrid(i), GetColor(_filter[i]));
        }

        bool drawingState;
        bool[] old;
        
        private void PadStarted(int index) {
            drawingState = !_filter[LaunchpadGrid.GridToSignal(index)];
            old = _filter.Filter.ToArray();
        }

        private void PadPressed(int index) => Grid.SetColor(index, GetColor(_filter[LaunchpadGrid.GridToSignal(index)] = drawingState));

        private void PadFinished(int index) {
            if (old == null) return;

            bool[] u = old.ToArray();
            bool[] r = _filter.Filter.ToArray();
            List<int> path = Track.GetPath(_filter);

            Program.Project.Undo.Add($"KeyFilter Changed", () => {
                ((KeyFilter)Track.TraversePath(path)).Filter = u.ToArray();
            }, () => {
                ((KeyFilter)Track.TraversePath(path)).Filter = r.ToArray();
            });

            old = null;
        }

        public void Set(bool[] filter) {
            for (int i = 0; i < 100; i++)
                Grid.SetColor(LaunchpadGrid.SignalToGrid(i), GetColor(filter[i]));
        }
    }
}
