using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using GoLib;

// Pour en savoir plus sur le modèle d'élément Page de base, consultez la page http://go.microsoft.com/fwlink/?LinkId=234237

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
        private Color _black;
        private Color _white;
        private Color _shared;

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
                this.passButton.IsEnabled = _state == GameState.Ongoing;
                this.undoButton.IsEnabled = _state != GameState.Finished;
                this.giveUpButton.IsEnabled = _state != GameState.Finished;
                this.submitButton.IsEnabled = _state == GameState.StoneSelection;
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

            _black = new Color();
            _white = new Color();
            _black.A = _white.A = 255;
            _white.R = _white.G = _white.B = 255;
            _shared = new Color();
            _shared.A = 255;
            _shared.R = 255;
            _shared.G = _shared.B = 120;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var info = GameForm.Info();
            _goban = new Goban(info);
            _stones = new Image[info.Size, info.Size];
            _territories = new Rectangle[info.Size, info.Size];
            _state = GameState.Ongoing;
        }

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
            Colour winner = _goban.CurrentColour.OpponentColor();
            winnerUi.Text = winner.ToString();
            resultUi.Text = winner.ToString()[0] + "+R";
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            State = GameState.Finished;
            Score score = new Score(_goban);
            var blackScore = score.Get(Colour.Black);
            var whiteScore = score.Get(Colour.White);
            blackScoreUi.Text = blackScore.ToString();
            whiteScoreUi.Text = whiteScore.ToString();
            winnerUi.Text = blackScore > whiteScore ? "Black" : "White";
            resultUi.Text = winnerUi.Text[0] + "+" + Math.Abs(blackScore - whiteScore);
        }

        private void replayButton_Click(object sender, RoutedEventArgs e)
        {
            _goban.Clear();
            State = GameState.Ongoing;
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
                if (lastMove != null && lastMove.Stone == Stone.FAKE)
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
            if (_goban.isMoveValid(stone))
            {
                Move move = _goban.Move(stone);
                Draw(stone);
                foreach (var captured in move.Captured)
                {
                    UnDraw(captured);
                }
                UpdateCaptured(stone.Color, move.Captured.Count);
                undoButton.IsEnabled = true;
                DrawMarker();
            }
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
            if (lastMove != null && lastMove.Stone == Stone.FAKE)
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
                UpdateCaptured(undo.Stone.Color, -undo.Captured.Count);
                undoButton.IsEnabled = _goban.Moves.Count != 0;
            }
            DrawMarker();
        }

        private void MarkGroup(GoLib.Point point)
        {
            _goban.MarkDead(point);
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
            var brush = new SolidColorBrush(_black);

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
            if (stone != null && stone != Stone.FAKE)
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
            hoshi.Fill = new SolidColorBrush(_black);
            Fit(hoshi, Convert(point));
            return hoshi;
        }

        private void Fit(FrameworkElement e, Windows.Foundation.Point point)
        {
            double gap = (SectionSize - e.Height) / 2;
            Canvas.SetLeft(e, point.X + gap);
            Canvas.SetTop(e, point.Y + gap);
        }

        #endregion

        #region Conversions

        private Color Convert(Colour colour)
        {
            return colour == Colour.Black ? _black : colour == Colour.White ? _white : _shared;
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
