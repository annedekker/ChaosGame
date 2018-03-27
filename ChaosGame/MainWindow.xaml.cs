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
        ChaosCode chaos;

        // color settings

        List<Color> cornerColors = new List<Color>() { Colors.Aqua, Colors.Gold, Colors.Lime };

        // generation settings

        int generateManyCount = 15000;

        // init

        public MainWindow()
        {
            chaos = new ChaosCode();
            InitializeComponent();
            
            wbmp = new WriteableBitmap(chaos.RenderSize, chaos.RenderSize, 333, 333, PixelFormats.Bgra32, null);
            wbmpImage.Width = wbmpImage.Height = theCanvas.Width = theCanvas.Height = chaos.RenderSize;
            wbmpImage.Source = wbmp;
        }

        // drawing chaos points

        private void DrawPoints(List<XYC> points)
        {
            foreach (XYC p in points)
            {
                if (p.x < 0 || p.x >= chaos.RenderSize ||
                    p.y < 0 || p.y >= chaos.RenderSize) continue;

                Color color = cornerColors[p.c % cornerColors.Count];
                byte[] colorData = { color.B, color.G, color.R, color.A }; // BGRA

                Int32Rect area = new Int32Rect(p.x, p.y, 1, 1);

                wbmp.WritePixels(area, colorData, 4, 0);
            }
        }

        // master controls

        private void ClearChaos_Click(object sender, EventArgs e)
        {
            /*byte[] colorData = { 0, 0, 0, 0 };
            
            for (int x = 0; x < chaos.RenderSize; x++)
            {
                for (int y = 0; y < chaos.RenderSize; y++)
                {
                    Int32Rect rect = new Int32Rect(x, y, 1, 1);
                    wbmp.WritePixels(rect, colorData, 4, 0);
                }
            }*/

            // find a better way to do this
            wbmp = new WriteableBitmap(chaos.RenderSize, chaos.RenderSize, 333, 333, PixelFormats.Bgra32, null);
            wbmpImage.Source = wbmp;
        }

        private void GenerateMany_Click(object sender, RoutedEventArgs e)
        {
            List<XYC> points = chaos.GetChaosPoints(generateManyCount);

            DrawPoints(points);
        }

        private void SideViewControl_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Name.Equals("settingsButton"))
            {
                if (settingsPanel.Visibility == Visibility.Visible) settingsPanel.Visibility = Visibility.Collapsed;
                else settingsPanel.Visibility = Visibility.Visible;
            }
        }

        // side view - settings

        private void RenderSizeTextbox_Changed(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length < 1) return;

            int value;

            if (!Int32.TryParse((sender as TextBox).Text, out value)) value = 512;

            if (value > 32768) value = 32768;
            else if (value < 1) value = 1;

            (sender as TextBox).Text = value.ToString();
        }

        private void RenderSizeOk_Click(object sender, EventArgs e)
        {
            int value = Int32.Parse(renderSizeTextbox.Text);

            if (value < 2) value = 2;

            chaos.RenderSize = value;
            wbmp = new WriteableBitmap(chaos.RenderSize, chaos.RenderSize, 333, 333, PixelFormats.Bgra32, null);
            wbmpImage.Width = wbmpImage.Height = theCanvas.Width = theCanvas.Height = chaos.RenderSize;
            wbmpImage.Source = wbmp;
        }

        private void CornerCountButton_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Content.Equals("+"))
            {
                if (chaos.CornerCount == 63) (sender as Button).IsEnabled = false;
                cornerCountDownButton.IsEnabled = true;

                chaos.CornerCount++;
                cornerCountTextbox.Text = chaos.CornerCount.ToString();
            }
            else
            {
                if (chaos.CornerCount == 4) (sender as Button).IsEnabled = false;
                cornerCountUpButton.IsEnabled = true;

                chaos.CornerCount--;
                cornerCountTextbox.Text = chaos.CornerCount.ToString();
            }

            if (!chaos.AutoCorners) BuildManualCornerPanel();
            if (chaos.ExcludeFromLast > 0) BuildExcludeIndexesPanel();
        }

        private void CornerCountTextbox_Changed(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text.Length < 1) return;

            int value;
            if (!Int32.TryParse((sender as TextBox).Text, out value)) value = 3;

            if (value < 1) value = 1;
            else if (value > 64) value = 64;

            (sender as TextBox).Text = value.ToString();

            try
            {
                if (value >= 3)
                {
                    chaos.CornerCount = value;
                    if (!chaos.AutoCorners) BuildManualCornerPanel();
                    if (chaos.ExcludeFromLast > 0) BuildExcludeIndexesPanel();
                }
            }
            catch (Exception) { }
        }

        private void AutoCorner_Checked(object sender, EventArgs e)
        {
            try
            {
                if ((sender as CheckBox).IsChecked == true)
                {
                    manualCornerView.Visibility = Visibility.Collapsed;
                    shapeSizeView.Visibility = Visibility.Visible;

                    chaos.AutoCorners = true;
                }
                else
                {
                    BuildManualCornerPanel();
                    manualCornerView.Visibility = Visibility.Visible;
                    shapeSizeView.Visibility = Visibility.Collapsed;

                    chaos.AutoCorners = false;
                }
            }
            catch (NullReferenceException) { }
        }

        private void ShapeSizeTextbox_Changed(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length < 1) return;

            int value;

            if (!Int32.TryParse((sender as TextBox).Text, out value)) value = 512;

            if (value > 65536) value = 65536;
            else if (value < 1) value = 1;

            (sender as TextBox).Text = value.ToString();
        }

        private void ShapeSizeOk_Click(object sender, EventArgs e)
        {
            int value = Int32.Parse(shapeSizeTextbox.Text);

            if (value < 2) value = 2;

            chaos.ShapeSize = value;
        }

        private void BuildManualCornerPanel()
        {
            manualCornerPanel.Children.Clear();

            StackPanel header = new StackPanel();
            header.Margin = new Thickness(1, 0, 1, 0);

            Label hlbl = new Label();
            hlbl.Content = "#";
            hlbl.HorizontalContentAlignment = HorizontalAlignment.Center;
            hlbl.Height = 26;
            hlbl.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            header.Children.Add(hlbl);

            hlbl = new Label();
            hlbl.Content = "x";
            hlbl.HorizontalContentAlignment = HorizontalAlignment.Center;
            hlbl.Height = 26;
            hlbl.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            header.Children.Add(hlbl);

            hlbl = new Label();
            hlbl.Content = "y";
            hlbl.HorizontalContentAlignment = HorizontalAlignment.Center;
            hlbl.Height = 26;
            hlbl.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            header.Children.Add(hlbl);
            manualCornerPanel.Children.Add(header);

            foreach (XYC c in chaos.Corners)
            {
                BuildManualCornerItem(c.c, c.x, c.y);
            }
        }

        private void BuildManualCornerItem(int c, int x, int y)
        {
            StackPanel panel = new StackPanel();
            panel.Margin = new Thickness(1, 0, 1, 0);

            Label poslbl = new Label();
            poslbl.Content = c.ToString();
            poslbl.HorizontalContentAlignment = HorizontalAlignment.Center;
            poslbl.Height = 26;
            poslbl.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            panel.Children.Add(poslbl);

            TextBox box = new TextBox();
            box.Name = "x" + c.ToString();
            box.Text = x.ToString();
            box.Height = 22;
            box.Margin = new Thickness(0, 2, 0, 2);
            box.HorizontalContentAlignment = HorizontalAlignment.Center;
            box.TextChanged += ManualCornerTextbox_Changed;
            box.Width = 44;
            panel.Children.Add(box);

            box = new TextBox();
            box.Name = "y" + c.ToString();
            box.Text = y.ToString();
            box.Height = 22;
            box.Margin = new Thickness(0, 2, 0, 2);
            box.HorizontalContentAlignment = HorizontalAlignment.Center;
            box.TextChanged += ManualCornerTextbox_Changed;
            box.Width = 44;
            panel.Children.Add(box);

            manualCornerPanel.Children.Add(panel);
        }

        private void ManualCornerTextbox_Changed(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text.Length < 1) return;

            bool isX = (sender as TextBox).Name.Substring(0, 1).Equals("x");
            int pos = Int32.Parse((sender as TextBox).Name.Substring(1));

            int value;
            if (!Int32.TryParse((sender as TextBox).Text, out value))
            {
                if (isX) value = chaos.Corners[pos].x;
                else value = chaos.Corners[pos].y;
            }

            chaos.SetCornerCoordinate(pos, isX, value);
        }

        private void DistanceMovedTextbox_Changed(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text.Length < 1) return;

            int value;
            if (!Int32.TryParse((sender as TextBox).Text, out value)) value = 50;

            if (value < 1) value = 1;
            else if (value > 1000) value = 1000;

            (sender as TextBox).Text = value.ToString();

            try
            {
                chaos.DistanceMoved = value / 100.0;
            }
            catch (NullReferenceException) { }
        }

        private void ExcludeFromLast_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Content.ToString().Equals("+"))
            {
                if (chaos.ExcludeFromLast == 3) (sender as Button).IsEnabled = false;
                excludeFromLastDownButton.IsEnabled = true;

                chaos.ExcludeFromLast++;
                excludeFromLastLabel.Content = chaos.ExcludeFromLast.ToString();

                if (chaos.ExcludeFromLast == 1)
                {
                    BuildExcludeIndexesPanel();
                    excludeIndexView.Visibility = Visibility.Visible;
                    excludeDirectionView.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (chaos.ExcludeFromLast == 0) (sender as Button).IsEnabled = false;
                excludeFromLastUpButton.IsEnabled = true;

                chaos.ExcludeFromLast--;
                excludeFromLastLabel.Content = chaos.ExcludeFromLast.ToString();

                if (chaos.ExcludeFromLast == 0)
                {
                    excludeIndexView.Visibility = Visibility.Collapsed;
                    excludeDirectionView.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ExcludeDirection_Checked(object sender, EventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true) chaos.ExcludeClockwise = true;
            else chaos.ExcludeClockwise = false;
        }

        private void BuildExcludeIndexesPanel()
        {
            excludeIndexPanel.Children.Clear();

            for (int i = 0; i < chaos.CornerCount; i++)
            {
                StackPanel panel = new StackPanel();

                Label lbl = new Label();
                lbl.Content = i.ToString();
                lbl.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));
                lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                panel.Children.Add(lbl);

                CheckBox box = new CheckBox();
                box.Name = "eb" + i.ToString();
                if (chaos.ExcludeIndexes.Any(x => x == i)) box.IsChecked = true;
                box.Checked += ExcludeIndex_Checked;
                box.Unchecked += ExcludeIndex_Checked;
                box.Margin = new Thickness(4);
                panel.Children.Add(box);

                excludeIndexPanel.Children.Add(panel);
            }
        }

        private void ExcludeIndex_Checked(object sender, EventArgs e)
        {
            int index = Int32.Parse((sender as CheckBox).Name.Substring(2));

            if ((sender as CheckBox).IsChecked == true)
            {
                chaos.ExcludeIndexes.Add(index);
            }
            else
            {
                chaos.ExcludeIndexes.Remove(index);
            }
        }
    }
}
