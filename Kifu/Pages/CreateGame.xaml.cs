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

        public static bool IsHuman(String player)
        {
            return player == "Human";
        }

        public static int Size(String value)
        {
            if (value == "13x13") return 13;
            if (value == "9x9") return 9;
            return 19;
        }

        public static Rule Rules(String rule)
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

        private void Player_Changed(object sender, RoutedEventArgs e)
        {
            var box = sender as ComboBox;
            if (box != null && !GameForm.IsHuman(box.SelectedValue.ToString())) Box_Loaded(box == BlackPlayerView ? WhitePlayerView : BlackPlayerView, e);
        }

        private void Box_Loaded(object sender, RoutedEventArgs e)
        {
            var box = sender as ComboBox;
            if (box != null) box.SelectedIndex = 0;
        }
    }
}
