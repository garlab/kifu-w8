using GoLib;
using Kifu.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Kifu.Pages
{
    public static class GameForm
    {
        public static GameInfo Info()
        {
            var localsettings = ApplicationData.Current.LocalSettings;
            var info = new GameInfo();
            info.Handicap = int.Parse(localsettings.Values["handicap"].ToString());
            info.Players[0].IsHuman = IsHuman(localsettings.Values["black"].ToString());
            info.Players[1].IsHuman = IsHuman(localsettings.Values["white"].ToString());
            info.Rule = Rules(localsettings.Values["rules"].ToString());
            info.Size = Size(localsettings.Values["size"].ToString());
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
        private ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

        public CreateGame()
        {
            this.InitializeComponent();
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                var localsettings = ApplicationData.Current.LocalSettings;
                localsettings.Values["black"] = BlackPlayerView.SelectedValue;
                localsettings.Values["white"] = WhitePlayerView.SelectedValue;
                localsettings.Values["size"] = SizeView.SelectedValue;
                localsettings.Values["handicap"] = HandicapView.Value;
                localsettings.Values["rules"] = RuleView.SelectedValue;
                this.Frame.Navigate(typeof(Game));
            }
        }

        private void Player_Changed(object sender, RoutedEventArgs e)
        {
            var box = sender as ComboBox;
            if (box != null && !GameForm.IsHuman(box.SelectedValue.ToString()))
            {
                Init(box == BlackPlayerView ? WhitePlayerView : BlackPlayerView);
            }
        }

        private void Black_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settings.Values["black"] == null)
                Init(BlackPlayerView);
            else
                BlackPlayerView.SelectedValue = _settings.Values["black"];
        }

        private void White_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settings.Values["white"] == null)
                Init(WhitePlayerView);
            else
                WhitePlayerView.SelectedValue = _settings.Values["white"];
        }

        private void Size_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settings.Values["size"] == null)
                Init(SizeView);
            else
                SizeView.SelectedValue = _settings.Values["size"];
        }

        private void Handicap_Loaded(object sender, RoutedEventArgs e)
        {
            HandicapView.Value = _settings.Values["handicap"] == null ? 0 : int.Parse(_settings.Values["handicap"].ToString());
        }

        private void Rules_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settings.Values["rules"] == null)
                Init(RuleView);
            else
                RuleView.SelectedValue = _settings.Values["rules"];
        }

        private void Init(ComboBox box)
        {
            box.SelectedIndex = 0;
        }

        private async void pickSgfButton_Click(object sender, RoutedEventArgs e)
        {
            // File picker APIs don't work if the app is in a snapped state.
            // If the app is snapped, try to unsnap it first. Only show the picker if it unsnaps.
            if (ApplicationView.Value != ApplicationViewState.Snapped || ApplicationView.TryUnsnap())
            {
                var openPicker = new FileOpenPicker();
                //openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                //openPicker.ViewMode = PickerViewMode.Thumbnail;

                // Filter to include a sample subset of file types.
                openPicker.FileTypeFilter.Clear();
                openPicker.FileTypeFilter.Add(".sgf");

                // Open the file picker.
                var file = await openPicker.PickSingleFileAsync();

                // file is null if user cancels the file picker.
                if (file != null)
                {
                    var buffer = await FileIO.ReadBufferAsync(file);
                    using (var reader = DataReader.FromBuffer(buffer))
                    {
                        string content = reader.ReadString(buffer.Length);
                        sgfContent.Text = content;
                    }
                    this.DataContext = file;

                    //var mruToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
        }
    }
}
