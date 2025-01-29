using Microsoft.Win32;
using Svg; // Biblioteka SVG.Net
using System.Drawing;
//using System.Windows.Shapes;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Biometria
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap? _bitmap;

        private System.Drawing.Point? selectedPixel = null; // zmienna do przechowywania zaznaczonego piksela

        public MainWindow()
        {
            InitializeComponent();
        }

        //zadanie 1.1
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.tiff;*.svg";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".svg")
                {
                    _bitmap = LoadSvg(filePath);
                }
                else
                {
                    _bitmap = new Bitmap(filePath);
                }

                ImageDisplay.Source = BitmapToImageSource(_bitmap);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp|TIFF Image|*.tiff|SVG Image|*.svg";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                string extension = Path.GetExtension(filePath).ToLower();

                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        _bitmap.Save(filePath, ImageFormat.Jpeg);
                        break;
                    case ".png":
                        _bitmap.Save(filePath, ImageFormat.Png);
                        break;
                    case ".bmp":
                        _bitmap.Save(filePath, ImageFormat.Bmp);
                        break;
                    case ".tiff":
                        _bitmap.Save(filePath, ImageFormat.Tiff);
                        break;
                    case ".svg":
                        //SaveSvg(filePath);
                        break;
                    default:
                        MessageBox.Show("NIeprawidłowy format");
                        break;
                }
            }
        }

        // Konwertowanie Bitmapy na ImageSource (do wyświetlania w WPF)
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private Bitmap LoadSvg(string filePath)
        {
            var svgDoc = SvgDocument.Open(filePath);

            return svgDoc.Draw();
        }

        private void SaveSvg(string filePath)
        {
            //nie ma
        }

        //zadanie 1.2
        private void ImageDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (_bitmap == null)
                return;

            var position = e.GetPosition(ImageDisplay);
            int x = (int)(position.X * _bitmap.Width / ImageDisplay.ActualWidth);
            int y = (int)(position.Y * _bitmap.Height / ImageDisplay.ActualHeight);

            if (x >= 0 && x < _bitmap.Width && y >= 0 && y < _bitmap.Height)
            {
                System.Drawing.Color color = _bitmap.GetPixel(x, y);
                PixelInfoText.Text = $"X: {x}, Y: {y} - R: {color.R}, G: {color.G}, B: {color.B}";
            }
        }

        private void ImageDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_bitmap == null)
                return;

            var position = e.GetPosition(ImageDisplay);

            int x = (int)(position.X * _bitmap.Width / ImageDisplay.ActualWidth);
            int y = (int)(position.Y * _bitmap.Height / ImageDisplay.ActualHeight);

            if (x >= 0 && x < _bitmap.Width && y >= 0 && y < _bitmap.Height)
            {
                selectedPixel = new System.Drawing.Point(x, y);
            }
        }

        private void ApplyColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int x = (int)selectedPixel.Value.X;
            int y = (int)selectedPixel.Value.Y;

            int r = int.Parse(RedTextBox.Text);
            int g = int.Parse(GreenTextBox.Text);
            int b = int.Parse(BlueTextBox.Text);

            _bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(r, g, b));

            ImageDisplay.Source = BitmapToImageSource(_bitmap);
        }

        //zadanie 1.3
        private void ImageDisplay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_bitmap == null)
                return;

            double zoomFactor = (e.Delta > 0) ? 1.1 : 0.9;

            double newScaleX = imageScaleTransform.ScaleX * zoomFactor;
            double newScaleY = imageScaleTransform.ScaleY * zoomFactor;

            var mousePosition = e.GetPosition(ImageDisplay);

            double offsetX = (mousePosition.X - ImageDisplay.ActualWidth / 2);
            double offsetY = (mousePosition.Y - ImageDisplay.ActualHeight / 2);

            imageTranslateTransform.X = (imageTranslateTransform.X - offsetX) * zoomFactor + offsetX;
            imageTranslateTransform.Y = (imageTranslateTransform.Y - offsetY) * zoomFactor + offsetY;

            imageScaleTransform.ScaleX = newScaleX;
            imageScaleTransform.ScaleY = newScaleY;
        }

        //zadanie 2
        private void BtnHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int[] redHistogram = new int[256];
            int[] greenHistogram = new int[256];
            int[] blueHistogram = new int[256];
            int[] averageHistogram = new int[256];

            for (int y = 0; y < _bitmap.Height; y++)
            {
                for (int x = 0; x < _bitmap.Width; x++)
                {
                    System.Drawing.Color pixel = _bitmap.GetPixel(x, y);
                    redHistogram[pixel.R]++;
                    greenHistogram[pixel.G]++;
                    blueHistogram[pixel.B]++;
                    int average = (pixel.R + pixel.G + pixel.B) / 3;
                    averageHistogram[average]++;
                }
            }

            HistogramWindow histogramWindow = new HistogramWindow();

            ShowHistogram(histogramWindow.HistogramTabControl, redHistogram, System.Windows.Media.Brushes.Red, "Red");
            ShowHistogram(histogramWindow.HistogramTabControl, greenHistogram, System.Windows.Media.Brushes.Green, "Green");
            ShowHistogram(histogramWindow.HistogramTabControl, blueHistogram, System.Windows.Media.Brushes.Blue, "Blue");
            ShowHistogram(histogramWindow.HistogramTabControl, averageHistogram, System.Windows.Media.Brushes.Gray, "Average");

            histogramWindow.Show();
        }

        private void ShowHistogram(TabControl tabcontrol, int[] histogram, System.Windows.Media.Brush color, string channel)
        {
            Canvas canvas = new Canvas
            {
                Width = 500,
                Height = 250,
                Background = System.Windows.Media.Brushes.White
            };

            int max = histogram.Max();

            //oś x
            var xAxis = new System.Windows.Shapes.Line
            {
                X1 = 40,
                Y1 = canvas.Height - 40,
                X2 = canvas.Width - 20,
                Y2 = canvas.Height - 40,
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 1
            };
            canvas.Children.Add(xAxis);

            //oś y
            var yAxis = new System.Windows.Shapes.Line
            {
                X1 = 40,
                Y1 = canvas.Height - 40,
                X2 = 40,
                Y2 = 10,
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 1
            };
            canvas.Children.Add(yAxis);

            //podpisy na x
            for (int i = 0; i <= 256; i += 64)
            {
                TextBlock label = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 10
                };
                Canvas.SetLeft(label, 40 + i * (canvas.Width - 60) / 256 - 10);
                Canvas.SetTop(label, canvas.Height - 35);
                canvas.Children.Add(label);
            }

            //podisy na y
            for (int i = 0; i <= 10; i++)
            {
                TextBlock label = new TextBlock
                {
                    Text = ((int)(max * i / 10.0)).ToString(),
                    FontSize = 10
                };
                Canvas.SetLeft(label, 5);
                Canvas.SetTop(label, canvas.Height - 40 - i * (canvas.Height - 50) / 10 - 5);
                canvas.Children.Add(label);
            }

            for (int i = 0; i < histogram.Length; i++)
            {
                double normalizedHeight = (histogram[i] / (double)max) * (canvas.Height - 50); // Leave space for axes
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                {
                    Width = (canvas.Width - 60) / 256,
                    Height = normalizedHeight,
                    Fill = color,
                    Margin = new Thickness(40 + i * (canvas.Width - 60) / 256, canvas.Height - 40 - normalizedHeight, 0, 0)
                };
                canvas.Children.Add(rect);
            }

            TabItem tabItem = new TabItem { Header = channel, Content = canvas };
            tabcontrol.Items.Add(tabItem);
        }

        //zadanie 2.1
        private Bitmap ApplyLUT(Bitmap bitmap, byte[] lut)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    System.Drawing.Color pixel = bitmap.GetPixel(x, y);
                    System.Drawing.Color newPixel = System.Drawing.Color.FromArgb(
                        lut[pixel.R],
                        lut[pixel.G],
                        lut[pixel.B]
                    );
                    newBitmap.SetPixel(x, y, newPixel);
                }
            }

            return newBitmap;
        }

        //metody do rozjasniania/ściemniania obrazu
        private byte[] BrighteningLUT()
        {
            byte[] lut = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                lut[i] = (byte)(Math.Pow(i / 255.0, 0.5) * 255); ; // pierwistek
            }
            return lut;
        }

        private byte[] DarkeningLUT()
        {
            byte[] lut = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                lut[i] = (byte)(Math.Pow(i / 255.0, 2) * 255); // kwadrat
            }
            return lut;
        }

        private void BtnBrighten_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            byte[] brighteningLUT = BrighteningLUT();
            _bitmap = ApplyLUT(_bitmap, brighteningLUT);
            ImageDisplay.Source = BitmapToImageSource(_bitmap);
        }

        private void BtnDarken_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            byte[] darkeningLUT = DarkeningLUT();
            _bitmap = ApplyLUT(_bitmap, darkeningLUT);
            ImageDisplay.Source = BitmapToImageSource(_bitmap);
        }

        //zadanie 2.2
        private void BtnHistogramStretch_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int kmin = 255, kmax = 0;
            for (int y = 0; y < _bitmap.Height; y++)
            {
                for (int x = 0; x < _bitmap.Width; x++)
                {
                    var pixel = _bitmap.GetPixel(x, y);
                    //int gray = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B); // Skala szarości w modelu YUV. Ma on tą zaletę, że bierze pod uwagę, że ludzkie oko jest bardziej wyczulone na kolor zielony, a najmniej na kolor niebieski.
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    kmin = Math.Min(kmin, gray);
                    kmax = Math.Max(kmax, gray);
                }
            }

            byte[] lut = new byte[256];

            if (kmin == kmax)
            {
                for (int i = 0; i < 256; i++)
                {
                    lut[i] = (byte)i;
                }
            }
            else
            {
                for (int i = 0; i < 256; i++)
                {
                    double value = ((i - kmin) / (double)(kmax - kmin)) * 255;

                    if (value < 0) value = 0;
                    if (value > 255) value = 255;

                    lut[i] = (byte)value;
                }
            }

            _bitmap = ApplyLUT(_bitmap, lut);
            ImageDisplay.Source = BitmapToImageSource(_bitmap);

            BtnHistogram_Click(sender, e);
        }

        //zadanie 2.3
        private void BtnHistogramEqualize_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int[] histogram = new int[256];
            for (int y = 0; y < _bitmap.Height; y++)
            {
                for (int x = 0; x < _bitmap.Width; x++)
                {
                    System.Drawing.Color pixel = _bitmap.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    histogram[gray]++;
                }
            }

            //Obliczenie dystrybuanty dla obrazu na podstawie jego histogramu (cumulative distribution function)
            double[] cdf = new double[256];
            double cumulativeSum = 0;
            int totalPixels = _bitmap.Width * _bitmap.Height;

            for (int k = 0; k < histogram.Length; k++)
            {
                double pr = (double)histogram[k] / totalPixels; //Prawdopodobieństwo wystąpienia r_k
                cumulativeSum += pr;
                cdf[k] = cumulativeSum;
            }

            //Obliczenie wartości w tablicy LUT
            byte[] lut = new byte[256];
            double s0 = cdf[0]; //Wartość dystrybuanty dla S_0
            double mMinus1 = 256 - 1; //m - 1, gdzie m to liczba składowych

            for (int k = 0; k < cdf.Length; k++)
            {
                lut[k] = (byte)(((cdf[k] - s0) / (1 - s0)) * mMinus1);
            }

            _bitmap = ApplyLUT(_bitmap, lut);
            ImageDisplay.Source = BitmapToImageSource(_bitmap);

            BtnHistogram_Click(sender, e);
        }

        //zadanie 3
        private void BtnFiltersWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) 
                return;

            FiltersWindow filtersWindow = new FiltersWindow(_bitmap);
            filtersWindow.ShowDialog();

            if (filtersWindow.UpdatedBitmap != null)
            {
                _bitmap = filtersWindow.UpdatedBitmap;
                ImageDisplay.Source = BitmapToImageSource(_bitmap);
            }
        }

        //zadanie 4
        private void BtnBinarizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            BinarizeWindow binarizeWindow = new BinarizeWindow(_bitmap);
            binarizeWindow.ShowDialog();

            if (binarizeWindow.UpdatedBitmap != null)
            {
                _bitmap = binarizeWindow.UpdatedBitmap;
                ImageDisplay.Source = BitmapToImageSource(_bitmap);
            }
        }

        private void BtnBinarizeOtsuWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            OtsuWindow otsuWindow = new OtsuWindow(_bitmap);
            otsuWindow.ShowDialog();

            if (otsuWindow.UpdatedBitmap != null)
            {
                _bitmap = otsuWindow.UpdatedBitmap;
                ImageDisplay.Source = BitmapToImageSource(_bitmap);
            }
        }

        private void BtnBinarizeNiblackWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            NiblackWindow niblackWindow = new NiblackWindow(_bitmap);
            niblackWindow.ShowDialog();

            if (niblackWindow.UpdatedBitmap != null)
            {
                _bitmap = niblackWindow.UpdatedBitmap;
                ImageDisplay.Source = BitmapToImageSource(_bitmap);
            }
        }
    }
}