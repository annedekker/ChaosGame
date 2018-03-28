using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace ChaosGame
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        WriteableBitmap wbmp;
        ChaosCode chaos;

        string pictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        // color settings

        SolidColorBrush drawCornerColor = Brushes.OrangeRed;
        SolidColorBrush drawShapeColor = Brushes.HotPink;
        List<Shape> cornerVisuals = new List<Shape>();
        List<Shape> shapeVisuals = new List<Shape>();
        List<Color> cornerColors = new List<Color>() { Colors.Aqua, Colors.Gold, Colors.Lime };

        // generation settings

        int generateManyCount = 15000;
        int generateAutoCount = 25;

        // color control

        string ccontrolCurrentColor;
        byte ccontrolRed;
        byte ccontrolGreen;
        byte ccontrolBlue;

        // init

        public MainWindow()
        {
            chaos = new ChaosCode();
            InitializeComponent();
            
            wbmp = new WriteableBitmap(chaos.RenderSize, chaos.RenderSize, 333, 333, PixelFormats.Bgra32, null);
            wbmpImage.Width = wbmpImage.Height = theCanvas.Width = theCanvas.Height = chaos.RenderSize;
            wbmpImage.Source = wbmp;

            timer = new DispatcherTimer();
            timer.Tick += AutoGenerateTimer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            drawCornerColorButton.Background = drawCornerColor;
            drawShapeColorButton.Background = drawShapeColor;

            BuildColorListPanel();
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

        private void GenerateAuto_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Content.ToString().Contains("Start"))
            {
                timer.Start();

                (sender as Button).Content = "Stop Auto-Generation";
            }
            else
            {
                timer.Stop();

                (sender as Button).Content = "Start Auto-Generation";
            }
        }

        private void SideViewControl_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Name.Equals("settingsButton"))
            {
                if (settingsPanel.Visibility == Visibility.Visible) settingsPanel.Visibility = Visibility.Collapsed;
                else settingsPanel.Visibility = Visibility.Visible;
            }
            else if ((sender as Button).Name.Equals("generationButton"))
            {
                if (generationPanel.Visibility == Visibility.Visible) generationPanel.Visibility = Visibility.Collapsed;
                else generationPanel.Visibility = Visibility.Visible;
            }
            else if ((sender as Button).Name.Equals("drawingButton"))
            {
                if (drawingPanel.Visibility == Visibility.Visible) drawingPanel.Visibility = Visibility.Collapsed;
                else drawingPanel.Visibility = Visibility.Visible;
            }
            else if ((sender as Button).Name.Equals("exportButton"))
            {
                if (exportPanel.Visibility == Visibility.Visible) exportPanel.Visibility = Visibility.Collapsed;
                else exportPanel.Visibility = Visibility.Visible;
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
            if (cornerColors.Count > chaos.CornerCount)
            {
                cornerColors = cornerColors.GetRange(0, chaos.CornerCount);
                BuildColorListPanel();
                colorCountLabel.Content = cornerColors.Count.ToString();
            }
            if (cornerVisuals.Count > 0) DrawCornerVisuals();
            if (shapeVisuals.Count > 0) DrawShapeVisual();
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
                    if (cornerColors.Count > chaos.CornerCount)
                    {
                        cornerColors = cornerColors.GetRange(0, chaos.CornerCount);
                        BuildColorListPanel();
                        colorCountLabel.Content = cornerColors.Count.ToString();
                    }
                    if (cornerVisuals.Count > 0) DrawCornerVisuals();
                    if (shapeVisuals.Count > 0) DrawShapeVisual();
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
                    if (shapeVisuals.Count > 0) DrawShapeVisual();
                }
                else
                {
                    BuildManualCornerPanel();
                    manualCornerView.Visibility = Visibility.Visible;
                    shapeSizeView.Visibility = Visibility.Collapsed;

                    chaos.AutoCorners = false;
                    if (shapeVisuals.Count > 0) DrawShapeVisual();
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

            if (cornerVisuals.Count > 0) DrawCornerVisuals();
            if (shapeVisuals.Count > 0) DrawShapeVisual();
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

            if (cornerVisuals.Count > 0) DrawCornerVisuals();
            if (shapeVisuals.Count > 0) DrawShapeVisual();
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

        // side view - drawing

        private void DrawCorners_Checked(object sender, EventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                DrawCornerVisuals();
            }
            else
            {
                foreach (Shape s in cornerVisuals)
                {
                    theCanvas.Children.Remove(s);
                }
                cornerVisuals.Clear();
            }
        }

        private void DrawCornerVisuals()
        {
            foreach (Shape s in cornerVisuals)
            {
                theCanvas.Children.Remove(s);
            }
            cornerVisuals.Clear();

            foreach (XYC c in chaos.Corners)
            {
                Rectangle rect = new Rectangle();
                rect.Width = rect.Height = chaos.RenderSize / 75;
                rect.Fill = drawCornerColor;

                rect.Margin = new Thickness(
                    c.x - rect.Width / 2, c.y - rect.Height / 2, 0, 0);

                cornerVisuals.Add(rect);
                theCanvas.Children.Add(rect);
            }
        }

        private void DrawCornerColor_Click(object sender, EventArgs e)
        {
            ccontrolCurrentColor = "drawcorner";
            ccontrolLabel.Content = "Currently Editing Corner Color";

            ccontrolRed = drawCornerColor.Color.R;
            ccontrolRSlider.Value = ccontrolRed;
            ccontrolRTextbox.Text = ccontrolRed.ToString();

            ccontrolGreen = drawCornerColor.Color.G;
            ccontrolGSlider.Value = ccontrolGreen;
            ccontrolGTextbox.Text = ccontrolGreen.ToString();

            ccontrolBlue = drawCornerColor.Color.B;
            ccontrolBSlider.Value = ccontrolBlue;
            ccontrolBTextbox.Text = ccontrolBlue.ToString();

            colorControlPanel.Visibility = Visibility.Visible;
        }

        private void DrawShape_Checked(object sender, EventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                DrawShapeVisual();
            }
            else
            {
                foreach (Shape s in shapeVisuals) theCanvas.Children.Remove(s);
                shapeVisuals.Clear();
            }
        }

        private void DrawShapeVisual()
        {
            foreach (Shape s in shapeVisuals) theCanvas.Children.Remove(s);
            shapeVisuals.Clear();

            if (chaos.AutoCorners)
            {
                Ellipse sVisual = new Ellipse();
                sVisual.Stroke = drawShapeColor;
                sVisual.StrokeThickness = chaos.RenderSize / 400;
                sVisual.Width = sVisual.Height = chaos.ShapeSize;
                sVisual.Margin = new Thickness((chaos.RenderSize - chaos.ShapeSize) / 2, (chaos.RenderSize - chaos.ShapeSize) / 2, 0, 0);

                shapeVisuals.Add(sVisual);
                theCanvas.Children.Add(sVisual);
            }
            else
            {
                for (int i = 0; i < chaos.CornerCount; i++)
                {
                    Line line = new Line();
                    line.X1 = chaos.Corners[i].x;
                    line.Y1 = chaos.Corners[i].y;
                    if (i == chaos.CornerCount - 1)
                    {
                        line.X2 = chaos.Corners[0].x;
                        line.Y2 = chaos.Corners[0].y;
                    }
                    else
                    {
                        line.X2 = chaos.Corners[i + 1].x;
                        line.Y2 = chaos.Corners[i + 1].y;
                    }
                    line.Stroke = drawShapeColor;
                    line.StrokeThickness = chaos.RenderSize / 400;

                    shapeVisuals.Add(line);
                    theCanvas.Children.Add(line);
                }
            }
        }

        private void DrawShapeColor_Click(object sender, EventArgs e)
        {
            ccontrolCurrentColor = "drawshape";
            ccontrolLabel.Content = "Currently Editing Shape Color";

            ccontrolRed = drawShapeColor.Color.R;
            ccontrolRSlider.Value = ccontrolRed;
            ccontrolRTextbox.Text = ccontrolRed.ToString();

            ccontrolGreen = drawShapeColor.Color.G;
            ccontrolGSlider.Value = ccontrolGreen;
            ccontrolGTextbox.Text = ccontrolGreen.ToString();

            ccontrolBlue = drawShapeColor.Color.B;
            ccontrolBSlider.Value = ccontrolBlue;
            ccontrolBTextbox.Text = ccontrolBlue.ToString();

            colorControlPanel.Visibility = Visibility.Visible;
        }

        private void ColorCountButton_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Content.ToString().Equals("+"))
            {
                if (cornerColors.Count == chaos.CornerCount - 1) (sender as Button).IsEnabled = false;
                colorCountDownButton.IsEnabled = true;

                if (cornerColors.Count >= chaos.CornerCount)
                {
                    (sender as Button).IsEnabled = false;
                    cornerColors = cornerColors.GetRange(0, chaos.CornerCount);
                }
                else
                {
                    cornerColors.Add(Colors.Black);
                }
            }
            else
            {
                if (cornerColors.Count == 2) (sender as Button).IsEnabled = false;
                colorCountUpButton.IsEnabled = true;

                cornerColors.RemoveAt(cornerColors.Count - 1);
            }

            BuildColorListPanel();
            colorCountLabel.Content = cornerColors.Count.ToString();
        }

        private void BuildColorListPanel()
        {
            colorListPanel.Children.Clear();

            for (int i = 0; i < cornerColors.Count; i++)
            {
                DockPanel panel = new DockPanel();
                panel.Margin = new Thickness(0, 2, 0, 2);

                Button btn = new Button();
                btn.Name = "cc" + i.ToString();
                btn.Width = btn.Height = 28;
                btn.Background = new SolidColorBrush(cornerColors[i]);
                btn.Click += CornerColorButton_Click;
                DockPanel.SetDock(btn, Dock.Right);
                panel.Children.Add(btn);

                Label lbl = new Label();
                lbl.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204));
                lbl.Content = "Color ";
                if (i < 10) lbl.Content += "0" + i.ToString();
                else lbl.Content += i.ToString();
                panel.Children.Add(lbl);

                colorListPanel.Children.Add(panel);
            }
        }

        private void CornerColorButton_Click(object sender, EventArgs e)
        {
            int index = Int32.Parse((sender as Button).Name.Substring(2));

            ccontrolCurrentColor = (sender as Button).Name;
            ccontrolLabel.Content = "Currently Editing Color ";
            if (index < 10) ccontrolLabel.Content += "0" + index.ToString();
            else ccontrolLabel.Content += index.ToString();

            ccontrolRed = cornerColors[index].R;
            ccontrolRSlider.Value = ccontrolRed;
            ccontrolRTextbox.Text = ccontrolRed.ToString();

            ccontrolGreen = cornerColors[index].G;
            ccontrolGSlider.Value = ccontrolGreen;
            ccontrolGTextbox.Text = ccontrolGreen.ToString();

            ccontrolBlue = cornerColors[index].B;
            ccontrolBSlider.Value = ccontrolBlue;
            ccontrolBTextbox.Text = ccontrolBlue.ToString();

            colorControlPanel.Visibility = Visibility.Visible;
        }

        // Color control

        private void ColorControlSlider_Changed(object sender, EventArgs e)
        {
            byte value = (Byte)(sender as Slider).Value;

            if ((sender as Slider).Name.Contains("RSlider"))
            {
                ccontrolRed = value;
                ccontrolRTextbox.Text = ccontrolRed.ToString();
            }
            else if ((sender as Slider).Name.Contains("GSlider"))
            {
                ccontrolGreen = value;
                ccontrolGTextbox.Text = ccontrolGreen.ToString();
            }
            else
            {
                ccontrolBlue = value;
                ccontrolBTextbox.Text = ccontrolBlue.ToString();
            }

            ccontrolVisual.Fill = new SolidColorBrush(Color.FromRgb(ccontrolRed, ccontrolGreen, ccontrolBlue));
        }

        private void ColorControlTextbox_Changed(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text.Length < 1) return;

            byte value;
            if (!Byte.TryParse((sender as TextBox).Text, out value)) value = 0;

            try
            {
                if ((sender as TextBox).Name.Contains("RTextbox"))
                {
                    ccontrolRed = value;
                    ccontrolRTextbox.Text = value.ToString();
                    ccontrolRSlider.Value = ccontrolRed;
                }
                else if ((sender as TextBox).Name.Contains("GTextbox"))
                {
                    ccontrolGreen = value;
                    ccontrolGTextbox.Text = value.ToString();
                    ccontrolGSlider.Value = ccontrolGreen;
                }
                else
                {
                    ccontrolBlue = value;
                    ccontrolBTextbox.Text = value.ToString();
                    ccontrolBSlider.Value = ccontrolBlue;
                }
            }
            catch (NullReferenceException) { }

            ccontrolVisual.Fill = new SolidColorBrush(Color.FromRgb(ccontrolRed, ccontrolGreen, ccontrolBlue));
        }

        private void ColorControlCancel_Click(object sender, EventArgs e)
        {
            colorControlPanel.Visibility = Visibility.Collapsed;
        }

        private void ColorControlSet_Click(object sender, EventArgs e)
        {
            if (ccontrolCurrentColor.Equals("drawcorner"))
            {
                drawCornerColor = new SolidColorBrush(Color.FromRgb(ccontrolRed, ccontrolGreen, ccontrolBlue));
                drawCornerColorButton.Background = drawCornerColor;
                if (cornerVisuals.Count > 0) DrawCornerVisuals();
            }
            else if (ccontrolCurrentColor.Equals("drawshape"))
            {
                drawShapeColor = new SolidColorBrush(Color.FromRgb(ccontrolRed, ccontrolGreen, ccontrolBlue));
                drawShapeColorButton.Background = drawShapeColor;
                if (shapeVisuals.Count > 0) DrawShapeVisual();
            }
            else
            {
                int index = Int32.Parse(ccontrolCurrentColor.Substring(2));

                cornerColors[index] = Color.FromRgb(ccontrolRed, ccontrolGreen, ccontrolBlue);
                BuildColorListPanel();
            }

            colorControlPanel.Visibility = Visibility.Collapsed;
        }

        // side view - generation

        private void AutoGenerateTimer_Tick(object sender, EventArgs e)
        {
            List<XYC> points = chaos.GetChaosPoints(generateAutoCount);
            DrawPoints(points);
        }

        private void AutoGenerateSpeed_Click(object sender, EventArgs e)
        {
            if ((sender as Button).Content.ToString().Equals("+"))
            {
                if (generateAutoCount == 70) (sender as Button).IsEnabled = false;
                autoGenerateDownButton.IsEnabled = true;

                generateAutoCount += 5;
                autoGenerateSpeedLabel.Content = generateAutoCount.ToString();
            }
            else
            {
                if (generateAutoCount == 10) (sender as Button).IsEnabled = false;
                autoGenerateDownButton.IsEnabled = true;

                generateAutoCount -= 5;
                autoGenerateSpeedLabel.Content = generateAutoCount.ToString();
            }
        }

        private void GenerateManyTextbox_Changed(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text.Length < 1) return;

            int value;
            if (!Int32.TryParse((sender as TextBox).Text, out value)) value = 500;

            if (value == 0) return;
            if (value < 0) value = value * -1;
            if (value > 50000) value = 50000;

            generateManyCount = value;
            (sender as TextBox).Text = value.ToString();
            generateManyButton.Content = "Generate [" + value.ToString() + "] Points";
        }

        // side view - export

        private void ExportSize_Changed(object sender, EventArgs e)
        {
            if ((sender as TextBox).Text.Length < 1) return;

            int value;
            if (!Int32.TryParse((sender as TextBox).Text, out value)) value = 512;

            if (value < 1) value = 1;
            else if (value > 32768) value = 32768;

            (sender as TextBox).Text = value.ToString();
        }

        private void ExportImage_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG File (*.png)|*.png";
            sfd.InitialDirectory = pictureFolder;

            if (sfd.ShowDialog() == true)
            {
                int size = Int32.Parse(exportSizeTextbox.Text);
                pictureFolder = sfd.FileName;

                using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                {
                    PngBitmapEncoder enc = new PngBitmapEncoder();
                    enc.Interlace = PngInterlaceOption.On;
                    enc.Frames.Add(BitmapFrame.Create(wbmp.Resize(size, size, WriteableBitmapExtensions.Interpolation.Bilinear)));
                    enc.Save(fs);
                    fs.Close();
                }
            }
        }
    }
}
