using GoLib;
using GoLib.SGF;
using Kifu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace Kifu.Pages
{
    enum GameState
    {
        Ongoing, StoneSelection, Finished
    }

    public sealed partial class Game : Kifu.Common.LayoutAwarePage
    {
        private Goban _goban;
        private GameState _state;
        private Image[,] _stones;
        private Rectangle[,] _territories;
        private Ellipse _marker;

        #region properties

        public double GobanSize
        {
            get { return gobanCanvas.Height; }
            set
            {
                if (gobanCanvas.Height != value)
                {
                    gobanCanvas.Width = gobanCanvas.Height = value;
                    Clear();
                    Draw();
                }
            }
        }

        public double SectionSize
        {
            get { return GobanSize / _goban.Info.Size; }
        }

        GameState State
        {
            get { return _state; }
            set
            {
                _state = value;
                UpdateButtons();
                Clear();
                Draw();
            }
        }

        #endregion

        public Game()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += Current_SizeChanged;
            StoneGroup.Changed += StoneGroup_Changed;
            Territory.Changed += Territory_Changed;
            //WeakAI.Changed += AI_Changed;
            //WeakAI.CleanNow += AI_CleanNow;

            var info = GameForm.Info();
            _goban = new Goban(info);
            _stones = new Image[info.Size, info.Size];
            _territories = new Rectangle[info.Size, info.Size];
            _state = GameState.Ongoing;
        }

        // handles the Click event of the Button for showing the light dismiss with animations behavior
        private void ShowPopupAnimationClicked(object sender, RoutedEventArgs e)
        {
            //if (!LightDismissAnimatedPopup.IsOpen) { LightDismissAnimatedPopup.IsOpen = true; }
        }

        // Handles the Click event on the Button within the simple Popup control and simply closes it.
        private void CloseAnimatedPopupClicked(object sender, RoutedEventArgs e)
        {
            //if (LightDismissAnimatedPopup.IsOpen) { LightDismissAnimatedPopup.IsOpen = false; }
        }

        #region AI values display

        /*
        private List<TextBlock> tbs = new List<TextBlock>();

        private void AI_CleanNow(object sender, EventArgs e)
        {
            foreach (var tb in tbs)
            {
                gobanCanvas.Children.Remove(tb);
            }
            tbs.Clear();
        }

        private void AI_Changed(object sender, EventArgs e)
        {
            var vs = sender as WeakAI.VStone;
            if (vs != null)
            {
                string val = vs.Value.ToString();
                Draw(vs.Point, val.Substring(0, val.Length > 5 ? 5 : val.Length));
            }
        }

        private void Draw(GoLib.Point point, string text)
        {
            var label = Label(point, text);
            tbs.Add(label);
            Canvas.SetZIndex(label, 999);
            gobanCanvas.Children.Add(label);
        }

        private TextBlock Label(GoLib.Point point, string text)
        {
            var label = new TextBlock();
            label.Text = text;
            label.FontSize = SectionSize / 4;
            Fit(label, Convert(point));
            return label;
        }
        //*/
        #endregion

        #region events

        private void GobanCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            FitCanvas();
            AIMove();
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            FitCanvas();
        }

        private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var point = Convert(e.GetCurrentPoint(gobanCanvas).Position);
            switch (State)
            {
                case GameState.Ongoing:
                    Move(new Stone(_goban.CurrentColour, point));
                    AIMove();
                    break;
                case GameState.StoneSelection:
                    MarkGroup(point);
                    break;
                default:
                    break;
            }
        }

        private void FitCanvas()
        {
            switch (ApplicationView.Value)
            {
                case ApplicationViewState.Filled:
                case ApplicationViewState.FullScreenLandscape:
                case ApplicationViewState.FullScreenPortrait:
                    GobanSize = content.ActualHeight;
                    break;
                case ApplicationViewState.Snapped:
                    GobanSize = 320;
                    break;
            }
        }

        private void passButton_Click(object sender, RoutedEventArgs e)
        {
            Pass();
            AIMove();
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            switch (State)
            {
                case GameState.Ongoing:
                    Undo();
                    break;
                case GameState.StoneSelection:
                    Array.Clear(_territories, 0, _territories.Length);
                    _goban.EraseTerritories(); // perhaps useless
                    State = GameState.Ongoing;
                    break;
            }
        }

        private void giveUpButton_Click(object sender, RoutedEventArgs e)
        {
            State = GameState.Finished;
            Score score = _goban.Score;
            score.GiveUp(_goban.CurrentColour.OpponentColor());
            ShowMessageDialog(_goban.CurrentColour.ToString() + " resign", score.Winner.ToString(), score.Result);
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: tester si le goban n'est pas vide (bug #1)
            State = GameState.Finished;
            Score score = _goban.Score;
            score.ComputeScore(); // TODO: calculer le score au fur et à mesure
            blackScoreUi.Text = score.Get(Colour.Black).ToString();
            whiteScoreUi.Text = score.Get(Colour.White).ToString();
            ShowMessageDialog("Game over", score.Winner.ToString(), score.Result);
        }

        private void openGameButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void newGameButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            // File picker APIs don't work if the app is in a snapped state.
            // If the app is snapped, try to unsnap it first. Only show the picker if it unsnaps.
            if (ApplicationView.Value != ApplicationViewState.Snapped || ApplicationView.TryUnsnap())
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Smart Game Format", new List<string>() { ".sgf" });
                savePicker.DefaultFileExtension = ".sgf";
                savePicker.SuggestedFileName = "Game " + DateTime.Now.ToString("u");
                var file = await savePicker.PickSaveFileAsync();

                if (null != file) // file is null if user cancels the file picker.
                {
                    var content = SgfHelper.ToString(_goban);
                    var writeStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                    writeStream.Size = 0;
                    var oStream = writeStream.GetOutputStreamAt(0);
                    var dWriter = new DataWriter(oStream);
                    dWriter.WriteString(content);

                    await dWriter.StoreAsync();
                    await oStream.FlushAsync();

                    //this.DataContext = file;
                    //var mruToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
                }
            }
        }

        private void replayButton_Click(object sender, RoutedEventArgs e)
        {
            Replay();
        }

        private void Territory_Changed(object sender, EventArgs e)
        {
            var territory = sender as Territory;
            if (territory != null)
            {
                UnDraw(territory);
                Draw(territory);
            }
        }

        private void StoneGroup_Changed(object sender, EventArgs e)
        {
            var group = sender as StoneGroup;
            if (group != null)
            {
                UnDraw(group);
                Draw(group);
            }
        }

        #endregion

        #region Actions

        private void AIMove()
        {
            if (!_goban.CurrentPlayer.IsHuman)
            {
                var lastMove = _goban.Moves.Count == 0 ? null : _goban.Moves.Last();
                if (lastMove != null && lastMove.Stone.IsPass)
                {
                    Pass(); // L'IA passe toujours lorsque le joueur passe
                    return;
                }
                AI.WeakAI ai = new AI.WeakAI(_goban, _goban.CurrentColour);
                var next = ai.NextStone();
                if (next == null)
                    Pass();
                else
                {
                    Move(next);
                }
            }
        }

        private void Move(Stone stone)
        {
            if (_goban.IsMoveValid(stone))
            {
                Move move = _goban.Move(stone);
                Draw(stone);
                move.Captured.Each(UnDraw);
                UpdateCaptured(stone.Color, move.Captured.Count); // TODO: Move into goban.move
                undoButton.IsEnabled = true;
                DrawMarker();
            }
        }

        public void UpdateButtons()
        {
            this.passButton.IsEnabled = _state == GameState.Ongoing;
            this.undoButton.IsEnabled = _state != GameState.Finished && _goban.Moves.Count != 0;
            this.giveUpButton.IsEnabled = _state != GameState.Finished;
            this.submitButton.IsEnabled = _state == GameState.StoneSelection;
        }

        private void UpdateCaptured(Colour colour, int captured)
        {
            var ui = colour == Colour.Black ? blackCapturedUi : whiteCapturedUi;
            _goban.Captured[(int)colour - 1] += captured; // TODO: move this line in Goban class
            ui.Text = _goban.Captured[(int)colour - 1].ToString();
        }

        private void Pass()
        {
            var lastMove = _goban.Moves.Count == 0 ? null : _goban.Moves.Last();
            if (lastMove != null && lastMove.Stone.IsPass)
            {
                State = GameState.StoneSelection;
                _goban.ComputeTerritories();
                DrawTerritories();
            }
            _goban.Pass();
            undoButton.IsEnabled = true;
            DrawMarker();
        }

        private void Undo()
        {
            Move undo = _goban.Undo();
            if (undo != null)
            {
                UnDraw(undo.Stone);
                foreach (var captured in undo.Captured)
                {
                    Draw(captured);
                }
                UpdateCaptured(undo.Stone.Color, -undo.Captured.Count); // TODO: Move into goban.undo
            }
            UpdateButtons();
            DrawMarker();
        }

        private void MarkGroup(GoLib.Point point)
        {
            _goban.MarkDead(point);
        }

        private void Replay()
        {
            _goban.Clear();
            AIMove();
            State = GameState.Ongoing;
        }

        private async void ShowMessageDialog(string title, string winner, string result)
        {
            var messageDialog = new MessageDialog(string.Format("{0} win by {1}", winner, result), title) { DefaultCommandIndex = 1 };
            messageDialog.Commands.Add(new UICommand("Show board", null, 0));
            messageDialog.Commands.Add(new UICommand("Play again", null, 1));


            var commandChosen = await messageDialog.ShowAsync();
            if ((int)commandChosen.Id == 1)
            {
                Replay();
            }
            else
            {
                // TODO: add move review
            }
        }

        #endregion

        #region draw methods

        public void Clear()
        {
            gobanCanvas.Children.Clear();
            Array.Clear(_territories, 0, _territories.Length);
            Array.Clear(_stones, 0, _stones.Length);
        }

        public void Draw()
        {
            DrawGrids();
            DrawHoshis();
            DrawTerritories();
            DrawStones();
            DrawMarker();
        }

        private void DrawGrids()
        {
            var brush = new SolidColorBrush(Colors.Black);

            for (var i = 0; i < _goban.Info.Size; ++i)
            {
                var v = new Line();
                var h = new Line();
                v.X1 = h.Y1 = SectionSize / 2;
                v.Y1 = v.Y2 = h.X1 = h.X2 = SectionSize * (i + 0.5);
                v.X2 = h.Y2 = SectionSize * (_goban.Info.Size - 0.5);
                v.Stroke = h.Stroke = brush;
                gobanCanvas.Children.Add(v);
                gobanCanvas.Children.Add(h);
            }
        }

        private void DrawHoshis()
        {
            foreach (var point in _goban.Hoshis)
            {
                var hoshiView = HoshiView(point);
                gobanCanvas.Children.Add(hoshiView);
            }
        }

        public void DrawStones()
        {
            foreach (var group in _goban.Groups)
            {
                Draw(group);
            }
        }

        public void DrawTerritories()
        {
            foreach (var territory in _goban.Territories)
            {
                Draw(territory);
            }
        }

        public void Draw(StoneGroup group)
        {
            foreach (var stone in group.Stones)
            {
                Draw(stone, group.Alive);
            }
        }

        public void UnDraw(StoneGroup group)
        {
            foreach (var stone in group.Stones)
            {
                UnDraw(stone);
            }
        }

        private void Draw(Stone stone, bool alive = true)
        {
            var stoneView = StoneView(stone, alive);
            gobanCanvas.Children.Add(stoneView);
            _stones[stone.Point.X - 1, stone.Point.Y - 1] = stoneView;
        }

        private void UnDraw(Stone stone)
        {
            var stoneView = _stones[stone.Point.X - 1, stone.Point.Y - 1];
            gobanCanvas.Children.Remove(stoneView);
            _stones[stone.Point.X - 1, stone.Point.Y - 1] = null;
        }

        public void Draw(Territory territory)
        {
            foreach (var point in territory.Points)
            {
                var territoryView = TerritoryView(point, territory.Color);
                gobanCanvas.Children.Add(territoryView);
                _territories[point.X - 1, point.Y - 1] = territoryView;
            }
        }

        public void UnDraw(Territory territory)
        {
            foreach (var point in territory.Points)
            {
                var territoryView = _territories[point.X - 1, point.Y - 1];
                gobanCanvas.Children.Remove(territoryView);
                _territories[point.X - 1, point.Y - 1] = null;
            }
        }

        public void DrawMarker()
        {
            gobanCanvas.Children.Remove(_marker);
            var stone = _goban.Top;
            if (stone != null && !stone.IsPass)
            {
                _marker = MarkerView(stone);
                gobanCanvas.Children.Add(_marker);
            }
        }

        private Ellipse MarkerView(Stone stone)
        {
            var ellipse = new Ellipse();
            ellipse.Width = ellipse.Height = SectionSize * 0.6;
            ellipse.Stroke = new SolidColorBrush(Convert(stone.Color.OpponentColor()));
            ellipse.StrokeThickness = 2;
            Fit(ellipse, Convert(stone.Point));
            return ellipse;
        }

        private Image StoneView(Stone stone, bool alive = true)
        {
            var image = new Image();
            image.Width = image.Height = SectionSize;
            image.Opacity = alive ? 1 : 0.5;
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Stone" + stone.Color.ToString() + ".png"));
            Fit(image, Convert(stone.Point));
            return image;
        }

        private Rectangle TerritoryView(GoLib.Point point, Colour color)
        {
            var rect = new Rectangle();
            rect.Width = rect.Height = SectionSize * 0.4;
            rect.Fill = new SolidColorBrush(Convert(color));
            Fit(rect, Convert(point));
            return rect;
        }

        private Ellipse HoshiView(GoLib.Point point)
        {
            var hoshi = new Ellipse();
            hoshi.Width = hoshi.Height = SectionSize * 0.15;
            hoshi.Fill = new SolidColorBrush(Colors.Black);
            Fit(hoshi, Convert(point));
            return hoshi;
        }

        private void Fit(FrameworkElement e, Windows.Foundation.Point point)
        {
            double gap = (SectionSize - e.ActualHeight) / 2;
            Canvas.SetLeft(e, point.X + gap);
            Canvas.SetTop(e, point.Y + gap);
        }

        #endregion

        #region Conversions

        private Color Convert(Colour colour)
        {
            return colour == Colour.Black ? Colors.Black : colour == Colour.White ? Colors.White : Colors.Transparent;
        }

        public GoLib.Point Convert(Windows.Foundation.Point p)
        {
            int x = (int)(p.X / SectionSize) + 1;
            int y = (int)(p.Y / SectionSize) + 1;
            return new GoLib.Point(x, y);
        }

        public Windows.Foundation.Point Convert(GoLib.Point p)
        {
            double x = (p.X - 1) * SectionSize;
            double y = (p.Y - 1) * SectionSize;
            return new Windows.Foundation.Point(x, y);
        }

        #endregion
    }
}
