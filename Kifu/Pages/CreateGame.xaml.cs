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
    struct GameInfo
    {
        public int size;
        public int handicap;
        public double komi;
    }

    public sealed partial class CreateGame : Kifu.Common.LayoutAwarePage
    {
        private GameInfo _info;

        public CreateGame()
        {
            this.InitializeComponent();
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        #region events

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(Game), _info);
            }
        }

        private void SizeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var value = this.SizeView.SelectedValue as string;
            //int index = SizeView.SelectedIndex;
            if (value == "19x19") _info.size = 19;
            if (value == "13x13") _info.size = 13;
            if (value == "9x9") _info.size = 9;
        }

        #endregion
    }
}
