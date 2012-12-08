using GoLib;
using Kifu.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Kifu.Pages
{
    public static class GameForm
    {
        public static GameInfo Info(string d)
        {
            var t = d.Split(':');
            var info = new GameInfo();
            info.Handicap = int.Parse(t[3]);
            info.Players[0].IsHuman = IsHuman(t[0]);
            info.Players[1].IsHuman = IsHuman(t[1]);
            info.Rule = Rules(t[4]);
            info.Size = Size(t[2]);
            return info;
        }

        static bool IsHuman(String player)
        {
            return player == "Human";
        }

        static int Size(String value)
        {
            if (value == "13x13") return 13;
            if (value == "9x9") return 9;
            return 19;
        }

        static Rule Rules(String rule)
        {
            return rule == "Chinese" ? Rule.Chinese : Rule.Japanese;
        }
    }

    public sealed partial class CreateGame : LayoutAwarePage
    {
        public CreateGame()
        {
            this.InitializeComponent();
        }

        #region events

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                string d = BlackPlayerView.SelectedValue.ToString() // 0 -> black
                    + ':' + WhitePlayerView.SelectedValue.ToString() // 1 -> white
                    + ':' + SizeView.SelectedValue.ToString() // 2 -> size
                    + ':' + HandicapView.Value.ToString() // 3 -> handicap
                    + ':' + RuleView.SelectedValue.ToString(); // 4 -> rules
                this.Frame.Navigate(typeof(Game), d);
            }
        }

        private void BlackPlayerView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (BlackPlayerView.SelectedValue.ToString() != "Human")
            {
                WhitePlayerView.SelectedIndex = 0;
            }
        }

        private void WhitePlayerView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (WhitePlayerView.SelectedValue.ToString() != "Human")
            {
                BlackPlayerView.SelectedIndex = 0;
            }
        }

        #endregion

        private void BlackPlayerView_Loaded(object sender, RoutedEventArgs e)
        {
            BlackPlayerView.SelectedIndex = 0;
        }

        private void WhitePlayerView_Loaded(object sender, RoutedEventArgs e)
        {
            WhitePlayerView.SelectedIndex = 0;
        }

        private void SizeView_Loaded(object sender, RoutedEventArgs e)
        {
            SizeView.SelectedIndex = 0;
        }

        private void RuleView_Loaded(object sender, RoutedEventArgs e)
        {
            RuleView.SelectedIndex = 0;
        }
    }
}
