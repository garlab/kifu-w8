using GoLib;
using Kifu.Common;
using Kifu.Utils;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Kifu.Pages
{
    public static class GameForm
    {
        public static GameInfo Info()
        {
            var info = new GameInfo();
            info.Handicap = Settings.Handicap;
            info.Players[0].IsHuman = IsHuman(Settings.Black);
            info.Players[1].IsHuman = IsHuman(Settings.White);
            info.Rule = Rule(Settings.Rules);
            info.Size = Size(Settings.Size);
            return info;
        }

        public static bool IsHuman(string player)
        {
            return player == "Human";
        }

        public static int Size(string value)
        {
            if (value == "13x13") return 13;
            if (value == "9x9") return 9;
            return 19;
        }

        public static Rules Rule(string rules)
        {
            // TODO: faire une methode de parsing générique avec le parser sgf
            return rules == "Chinese" ? Rules.Chinese : Rules.Japanese;
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
                Settings.Black = BlackPlayerView.SelectedValue.ToString();
                Settings.White = WhitePlayerView.SelectedValue.ToString();
                Settings.Size = SizeView.SelectedValue.ToString();
                Settings.Handicap = (int)HandicapView.Value;
                Settings.Rules = RuleView.SelectedValue.ToString();
                this.Frame.Navigate(typeof(Game));
            }
        }

        private void Player_Changed(object sender, RoutedEventArgs e)
        {
            var box = sender as ComboBox;
            if (box != null && !GameForm.IsHuman(box.SelectedValue.ToString()))
            {
                var opponent = box == BlackPlayerView ? WhitePlayerView : BlackPlayerView;
                opponent.SelectedIndex = 0;
            }
        }

        private void Black_Loaded(object sender, RoutedEventArgs e)
        {
            BlackPlayerView.SelectedValue = Settings.Black;
        }

        private void White_Loaded(object sender, RoutedEventArgs e)
        {
            WhitePlayerView.SelectedValue = Settings.White;
        }

        private void Size_Loaded(object sender, RoutedEventArgs e)
        {
            SizeView.SelectedValue = Settings.Size;
        }

        private void Handicap_Loaded(object sender, RoutedEventArgs e)
        {
            HandicapView.Value = Settings.Handicap;
        }

        private void Rules_Loaded(object sender, RoutedEventArgs e)
        {
            RuleView.SelectedValue = Settings.Rules;
        }

        private async void pickSgfButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.Value != ApplicationViewState.Snapped || ApplicationView.TryUnsnap())
            {
                var openPicker = new FileOpenPicker();
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                //openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.FileTypeFilter.Clear();
                openPicker.FileTypeFilter.Add(".sgf");
                var file = await openPicker.PickSingleFileAsync();

                if (file != null)
                {
                    var buffer = await FileIO.ReadBufferAsync(file);
                    using (var reader = DataReader.FromBuffer(buffer))
                    {
                        string content = reader.ReadString(buffer.Length);
                        //sgfContent.Text = content;
                    }

                    //this.DataContext = file;
                    //var mruToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
        }
    }
}
