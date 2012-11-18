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
    /// <summary>
    /// Page de base qui inclut des caractéristiques communes à la plupart des applications.
    /// </summary>
    public sealed partial class Game : Kifu.Common.LayoutAwarePage
    {
        private double _size;
        private Goban _goban;
        private Image[,] _stones;

        public Game()
        {
            this.InitializeComponent();
            _goban = new Goban(19, Colour.Black);
            _stones = new Image[19, 19];
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
            _size = GobanCanvas.ActualWidth > GobanCanvas.ActualHeight ? GobanCanvas.ActualHeight : GobanCanvas.ActualWidth;
            GobanCanvas.Width = GobanCanvas.Height = _size;
            DrawGrid();
        }

        private void DrawGrid()
        {
            var black = new Color();
            black.A = 255;
            black.R = 0;
            black.G = 0;
            black.B = 0;

            var brush = new SolidColorBrush(black);

            for (var i = 0; i < _goban.Size; ++i)
            {
                var v = new Line();
                v.X1 = _size / (2 * _goban.Size);
                v.Y1 = _size / (2 * _goban.Size) + i * _size / _goban.Size;
                v.X2 = _size / (2 * _goban.Size) + (_goban.Size - 1) * _size / _goban.Size;
                v.Y2 = v.Y1;
                v.Stroke = brush;
                GobanCanvas.Children.Add(v);

                var h = new Line();
                h.X1 = _size / (2 * _goban.Size) + i * _size / _goban.Size;
                h.Y1 = _size / (2 * _goban.Size);
                h.X2 = h.X1;
                h.Y2 = _size / (2 * _goban.Size) + (_goban.Size - 1) * _size / _goban.Size;
                h.Stroke = brush;
                GobanCanvas.Children.Add(h);
            }
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            
        }

        private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var p = e.GetCurrentPoint(GobanCanvas).Position;
            int x = (int) (p.X * _goban.Size / _size);
            int y = (int) (p.Y * _goban.Size / _size);

            Stone stone = new Stone(_goban.CurrentColour, new GoLib.Point(x, y));

            if (_goban.isMoveValid(stone))
            {
                Move move = _goban.Move(stone);
                draw(stone);
                undraw(move.Captured);
            }
        }

        public void draw(Stone stone)
        {
            var image = getImage(stone);

            double left = stone.Point.X * _size / _goban.Size;
            double top = stone.Point.Y * _size / _goban.Size;

            Canvas.SetTop(image, top);
            Canvas.SetLeft(image, left);
            GobanCanvas.Children.Add(image);
            _stones[stone.Point.X, stone.Point.Y] = image;
        }

        private void undraw(List<Stone> stones)
        {
            foreach (var stone in stones)
            {
                var image = _stones[stone.Point.X, stone.Point.Y];
                GobanCanvas.Children.Remove(image);
            }
        }

        private Image getImage(Stone stone, bool alpha = false)
        {
            var image = new Image();
            image.Width = image.Height = _size / _goban.Size;
            //image.Opacity = 150;
            var uri = stone.Color == Colour.Black ? "ms-appx:///Assets/StoneBlack.png" : "ms-appx:///Assets/StoneWhite.png";
            image.Source = new BitmapImage(new Uri(uri));
            return image;
        }
    }
}
