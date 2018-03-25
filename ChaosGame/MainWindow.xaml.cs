using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChaosGame
{
    public partial class MainWindow : Window
    {
        WriteableBitmap wbmp;
        Image wbmpImage;
        ChaosCode chaos;

        // color settings

        List<Color> cornerColors = new List<Color>() { Colors.Aqua, Colors.Gold, Colors.Lime };

        // generation settings

        int generateManyCount = 15000;

        // init

        public MainWindow()
        {
            InitializeComponent();

            chaos = new ChaosCode();
            wbmp = new WriteableBitmap(chaos.RenderSize, chaos.RenderSize, 128, 128, PixelFormats.Bgra32, null);
            wbmpImage = new Image();
            wbmpImage.Source = wbmp;
            wbmpImage.Width = wbmpImage.Height = 1000;
            theCanvas.Children.Add(wbmpImage);
        }

        // drawing chaos points

        private void DrawPoints(List<XYC> points)
        {
            foreach (XYC p in points)
            {
                wbmp.SetPixel(p.x, p.y,
                    cornerColors[p.c % cornerColors.Count]);
            }
        }

        // master controls

        private void GenerateMany_Click(object sender, RoutedEventArgs e)
        {
            List<XYC> points = chaos.GetChaosPoints(generateManyCount);

            DrawPoints(points);
        }
    }
}
