using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;

namespace Biometria_Zad_1_Kamila_Szymanowska
{
    /// <summary>
    /// Interaction logic for FiltersWindow.xaml
    /// </summary>
    public partial class FiltersWindow : Window
    {
        private Bitmap? _bitmap;
        public Bitmap? UpdatedBitmap { get; private set; }

        public FiltersWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _bitmap = (Bitmap)bitmap.Clone();
        }

        private void ApplyCustomKernel_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) return;

            double[,] kernel = new double[3, 3]
            {
            { double.Parse(Kernel00.Text), double.Parse(Kernel01.Text), double.Parse(Kernel02.Text) },
            { double.Parse(Kernel10.Text), double.Parse(Kernel11.Text), double.Parse(Kernel12.Text) },
            { double.Parse(Kernel20.Text), double.Parse(Kernel21.Text), double.Parse(Kernel22.Text) }
            };

            _bitmap = ApplyConvolution(_bitmap, kernel);
            UpdatedBitmap = _bitmap;
            MessageBox.Show("Filter applied!");
        }

        private void LoadKernel_Click(object sender, RoutedEventArgs e)
        {
            var kernelName = (sender as Button)?.Tag.ToString();
            switch (kernelName)
            {
                case "LowPass":
                    SetKernelValues(new double[3, 3]
                    {
                    { 1 / 9.0, 1 / 9.0, 1 / 9.0 },
                    { 1 / 9.0, 1 / 9.0, 1 / 9.0 },
                    { 1 / 9.0, 1 / 9.0, 1 / 9.0 }
                    });
                    break;
                case "Prewitt":
                    SetKernelValues(new double[3, 3]
                    {
                    { -1, 0, 1 },
                    { -1, 0, 1 },
                    { -1, 0, 1 }
                    });
                    break;
                case "Sobel":
                    SetKernelValues(new double[3, 3]
                    {
                    { -1, 0, 1 },
                    { -2, 0, 2 },
                    { -1, 0, 1 }
                    });
                    break;
                case "Laplacian":
                    SetKernelValues(new double[3, 3]
                    {
                    { 0, -1, 0 },
                    { -1, 4, -1 },
                    { 0, -1, 0 }
                    });
                    break;
                case "Corner":
                    SetKernelValues(new double[3, 3]
                    {
                    { -1, -1, -1 },
                    { -1, 8, -1 },
                    { -1, -1, -1 }
                    });
                    break;
            }
        }

        private void ApplyMedian3x3_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) return;

            _bitmap = ApplyMedianFilter(_bitmap, 3);
            UpdatedBitmap = _bitmap;
            MessageBox.Show("Median 3x3 applied!");
        }

        private void ApplyMedian5x5_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) return;

            _bitmap = ApplyMedianFilter(_bitmap, 5);
            UpdatedBitmap = _bitmap;
            MessageBox.Show("Median 5x5 applied!");
        }

        private void SetKernelValues(double[,] kernel)
        {
            Kernel00.Text = kernel[0, 0].ToString();
            Kernel01.Text = kernel[0, 1].ToString();
            Kernel02.Text = kernel[0, 2].ToString();
            Kernel10.Text = kernel[1, 0].ToString();
            Kernel11.Text = kernel[1, 1].ToString();
            Kernel12.Text = kernel[1, 2].ToString();
            Kernel20.Text = kernel[2, 0].ToString();
            Kernel21.Text = kernel[2, 1].ToString();
            Kernel22.Text = kernel[2, 2].ToString();
        }

        private Bitmap ApplyConvolution(Bitmap bitmap, double[,] kernel)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap newBitmap = new Bitmap(width, height);

            // Convolution kernel dimensions
            int kernelWidth = kernel.GetLength(1);
            int kernelHeight = kernel.GetLength(0);
            int offsetX = kernelWidth / 2;
            int offsetY = kernelHeight / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    for (int ky = 0; ky < kernelHeight; ky++)
                    {
                        for (int kx = 0; kx < kernelWidth; kx++)
                        {
                            int pixelX = Math.Min(width - 1, Math.Max(0, x + kx - offsetX));
                            int pixelY = Math.Min(height - 1, Math.Max(0, y + ky - offsetY));

                            System.Drawing.Color pixel = bitmap.GetPixel(pixelX, pixelY);

                            double kernelValue = kernel[ky, kx];
                            rSum += pixel.R * kernelValue;
                            gSum += pixel.G * kernelValue;
                            bSum += pixel.B * kernelValue;
                        }
                    }

                    int r = Math.Min(255, Math.Max(0, (int)rSum));
                    int g = Math.Min(255, Math.Max(0, (int)gSum));
                    int b = Math.Min(255, Math.Max(0, (int)bSum));

                    newBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(r, g, b));
                }
            }

            return newBitmap;
        }

        private Bitmap ApplyMedianFilter(Bitmap bitmap, int maskSize)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap newBitmap = new Bitmap(width, height);

            int offset = maskSize / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Zbierz piksele z okna maski
                    List<int> rValues = new List<int>();
                    List<int> gValues = new List<int>();
                    List<int> bValues = new List<int>();

                    for (int ky = -offset; ky <= offset; ky++)
                    {
                        for (int kx = -offset; kx <= offset; kx++)
                        {
                            int pixelX = Math.Min(width - 1, Math.Max(0, x + kx));
                            int pixelY = Math.Min(height - 1, Math.Max(0, y + ky));

                            System.Drawing.Color pixel = bitmap.GetPixel(pixelX, pixelY);

                            rValues.Add(pixel.R);
                            gValues.Add(pixel.G);
                            bValues.Add(pixel.B);
                        }
                    }

                    // Znajdź medianę dla każdego kanału
                    rValues.Sort();
                    gValues.Sort();
                    bValues.Sort();

                    int rMedian = rValues[rValues.Count / 2];
                    int gMedian = gValues[gValues.Count / 2];
                    int bMedian = bValues[bValues.Count / 2];

                    newBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(rMedian, gMedian, bMedian));
                }
            }

            return newBitmap;
        }
    }
}