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
using GoLib;

// Pour en savoir plus sur le modèle d'élément Page de base, consultez la page http://go.microsoft.com/fwlink/?LinkId=234237

namespace Kifu.Pages
{
    enum GameState
    {
        Ongoing, StoneSelection, Finished
    }

    /// <summary>
    /// Page de base qui inclut des caractéristiques communes à la plupart des applications.
    /// </summary>
    public sealed partial class Game : Kifu.Common.LayoutAwarePage
    {
        private double _size;
        private Goban _goban;
        private Image[,] _stones;
        private Rectangle[,] _territories;
        private GameState _game = GameState.Ongoing;
        private Colour _ia;
        private Color _black;
        private Color _white;
        private Color _shared;

        public Game()
        {
            int size = 19; // TODO: utiliser une taille paramétrable par l'utilisateur
            this.InitializeComponent();
            _goban = new Goban(size, Colour.Black);
            _stones = new Image[size, size];
            _territories = new Rectangle[size, size];
            _ia = Colour.White;

            _black = new Color();
            _white = new Color();
            _black.A = _white.A = 255;
            //_black.R = _black.G = _black.B = 0;
            _white.R = _white.G = _white.B = 255;
            _shared = new Color();
            _shared.A = 255;
            _shared.R = 255;
            _shared.G = _shared.B = 120;
        }

        /// <summary>
        /// Remplit la page à l'aide du contenu passé lors de la navigation. Tout état enregistré est également
        /// fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        /// <param name="navigationParameter">Valeur de paramètre passée à
        /// <see cref="Frame.Navigate(Type, Object)"/> lors de la requête initiale de cette page.
        /// </param>
        /// <param name="pageState">Dictionnaire d'état conservé par cette page durant une session
        /// antérieure. Null lors de la première visite de la page.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Conserve l'état associé à cette page en cas de suspension de l'application ou de la
        /// suppression de la page du cache de navigation. Les valeurs doivent être conformes aux
        /// exigences en matière de sérialisation de <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">Dictionnaire vide à remplir à l'aide de l'état sérialisable.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void GobanCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            StoneGroup.Changed += StoneGroup_Changed;
            Territory.Changed += Territory_Changed;
            _size = gobanCanvas.ActualWidth > gobanCanvas.ActualHeight ? gobanCanvas.ActualHeight : gobanCanvas.ActualWidth;
            gobanCanvas.Width = gobanCanvas.Height = _size;
            DrawGrid();
        }

        #region events

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var point = Convert(e.GetCurrentPoint(gobanCanvas).Position);
            switch (_game)
            {
                case GameState.Ongoing:
                    Move(new Stone(_goban.CurrentColour, point));
                    IAMove();
                    break;
                case GameState.StoneSelection:
                    MarkGroup(point);
                    break;
                default:
                    break;
            }
        }

        // TODO: disable passButton if game state != Ongoing
        private void passButton_Click(object sender, RoutedEventArgs e)
        {
            Pass();
            IAMove();
        }

        // TODO: disable if state == Finish
        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_game)
            {
                case GameState.Ongoing:
                    Undo();
                    break;
                case GameState.StoneSelection:
                    _game = GameState.Ongoing;
                    EraseTerritories();
                    break;
            }
        }

        private void Territory_Changed(object sender, EventArgs e)
        {
            var territory = sender as Territory;
            if (territory != null)
            {
                // TODO: mettre à jour l'affichage
            }
        }

        private void StoneGroup_Changed(object sender, EventArgs e)
        {
            var group = sender as StoneGroup;
            if (group != null)
            {
                // TODO: mettre à jour l'affichage
            }
        }

        #endregion

        #region Actions

        private void IAMove()
        {
            if (_goban.CurrentColour == _ia)
            {
                var lastMove = _goban.Moves.Last();
                if (lastMove != null && lastMove.Stone == Stone.FAKE)
                {
                    Pass(); // L'IA passe toujours lorsque le joueur passe
                    return;
                }
                AI.WeakAI ai = new AI.WeakAI(_goban, Colour.White);
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
                    Undraw(captured);
                }
            }
        }

        private void Pass()
        {
            var lastMove = _goban.Moves.Last();
            if (lastMove != null && lastMove.Stone == Stone.FAKE)
            {
                _game = GameState.StoneSelection;
                DrawTerritories();
            }
            _goban.Pass();
        }

        private void Undo()
        {
            Move undo = _goban.Undo();
            if (undo != null)
            {
                Undraw(undo.Stone);
                foreach (var captured in undo.Captured)
                {
                    Draw(captured);
                }
            }
        }

        private void MarkGroup(GoLib.Point point)
        {
            _goban.MarkDead(point);
        }

        private void EraseTerritories()
        {
            _goban.EraseTerritories();
            for (int i = 0; i < _goban.Size; ++i)
            {
                for (int j = 0; j < _goban.Size; ++j)
                {
                    if (_territories[i, j] != null)
                    {
                        gobanCanvas.Children.Remove(_territories[i, j]);
                        _territories[i, j] = null;
                    }
                }
            }
        }

        public void DrawTerritories()
        {
            _goban.ComputeTerritories();
            foreach (var territory in _goban.Territories)
            {
                DrawTerritory(territory);
            }
        }

        #endregion

        #region draw methods

        // TODO : remove
        private void Refresh()
        {
            foreach (var territory in _goban.Territories)
            {
                var color = Convert(territory.Color);
                var p = territory.Points.First();
                foreach (var point in territory.Points)
                {
                    // TODO: finir ui update
                }
            }
        }

        private void DrawGrid()
        {
            var brush = new SolidColorBrush(_black);

            for (var i = 0; i < _goban.Size; ++i)
            {
                var v = new Line();
                v.X1 = _size / (2 * _goban.Size);
                v.Y1 = _size / (2 * _goban.Size) + i * _size / _goban.Size;
                v.X2 = _size / (2 * _goban.Size) + (_goban.Size - 1) * _size / _goban.Size;
                v.Y2 = v.Y1;
                v.Stroke = brush;
                gobanCanvas.Children.Add(v);

                var h = new Line();
                h.X1 = _size / (2 * _goban.Size) + i * _size / _goban.Size;
                h.Y1 = _size / (2 * _goban.Size);
                h.X2 = h.X1;
                h.Y2 = _size / (2 * _goban.Size) + (_goban.Size - 1) * _size / _goban.Size;
                h.Stroke = brush;
                gobanCanvas.Children.Add(h);
            }
        }

        public void DrawTerritory(Territory territory)
        {
            double size = _size * 0.4 / _goban.Size;
            double gap = (_size / _goban.Size - size) / 2;
            var brush = new SolidColorBrush(Convert(territory.Color));

            foreach (var point in territory.Points)
            {
                var rect = new Rectangle();
                var coord = Convert(point);
                rect.Width = rect.Height = size;
                rect.Fill = brush;

                Canvas.SetLeft(rect, coord.X + gap);
                Canvas.SetTop(rect, coord.Y + gap);
                gobanCanvas.Children.Add(rect);
                _territories[point.X - 1, point.Y - 1] = rect;
            }
        }

        private void Draw(Stone stone)
        {
            var image = getImage(stone);
            var point = Convert(stone.Point);

            Canvas.SetTop(image, point.Y);
            Canvas.SetLeft(image, point.X);
            gobanCanvas.Children.Add(image);
            _stones[stone.Point.X - 1, stone.Point.Y - 1] = image;
        }

        private void Undraw(Stone stone)
        {
            var image = _stones[stone.Point.X - 1, stone.Point.Y - 1];
            gobanCanvas.Children.Remove(image);
        }

        #endregion

        #region Conversions

        // TODO: remplacer les conversions par un méchanisme plus propre

        private Color Convert(Colour colour)
        {
            return colour == Colour.Black ? _black : colour == Colour.White ? _white : _shared;
        }

        public GoLib.Point Convert(Windows.Foundation.Point p)
        {
            int x = (int)(p.X * _goban.Size / _size) + 1;
            int y = (int)(p.Y * _goban.Size / _size) + 1;
            return new GoLib.Point(x, y);
        }

        public Windows.Foundation.Point Convert(GoLib.Point p)
        {
            double x = (p.X - 1) * _size / _goban.Size;
            double y = (p.Y - 1) * _size / _goban.Size;
            return new Windows.Foundation.Point(x, y);
        }

        private Image getImage(Stone stone, bool alpha = false)
        {
            var image = new Image();
            image.Width = image.Height = _size / _goban.Size;
            image.Opacity = alpha ? 0.5 : 1;
            var uri = stone.Color == Colour.Black ? "ms-appx:///Assets/StoneBlack.png" : "ms-appx:///Assets/StoneWhite.png";
            image.Source = new BitmapImage(new Uri(uri));
            return image;
        }

        #endregion
    }
}
