using GoLib;
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
    public sealed partial class CreateGame : Kifu.Common.LayoutAwarePage
    {
        private GameInfo _info;

        public CreateGame()
        {
            this.InitializeComponent();
            _info = new GameInfo();
        }

        /*
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            base.LoadState(navigationParameter, pageState);
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }//*/

        #region events

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(Game), _info);
            }
        }

        private void BlackPlayerView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _info.Players[0].IsHuman = IsHuman(BlackPlayerView);
        }

        private void WhitePlayerView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _info.Players[1].IsHuman = IsHuman(WhitePlayerView);
        }

        private void SizeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _info.Size = Size(SizeView.SelectedValue.ToString());
        }

        private void HandicapView_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _info.Handicap = (int)HandicapView.Value;
        }

        private void RuleView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool chinese = RuleView.SelectedValue.ToString() == "Chinese";
            _info.Rule = chinese ? Rule.Chinese : Rule.Japanese;
        }

        #endregion

        #region convert

        static bool IsHuman(ComboBox box)
        {
            return box.SelectedValue.ToString() == "Human";
        }

        static int Size(String value)
        {
            if (value == "13x13") return 13;
            if (value == "9x9") return 9;
            return 19;
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
