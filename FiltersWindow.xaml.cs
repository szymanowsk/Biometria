using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace Biometria
{
    public partial class FiltersWindow : Window
    {
        private Bitmap? _bitmap;
        public Bitmap? UpdatedBitmap { get; private set; }

        public FiltersWindow(Bitmap bitmap)
        {
            InitializeComponent();
            //_bitmap = (Bitmap)bitmap.Clone();
            _bitmap = bitmap;
        }

        private void BtnApplyCustomMask_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) 
                return;

            try
            {
                double[,] mask = new double[3, 3]
                {
                    { double.Parse(Mask00.Text), double.Parse(Mask01.Text), double.Parse(Mask02.Text) },
                    { double.Parse(Mask10.Text), double.Parse(Mask11.Text), double.Parse(Mask12.Text) },
                    { double.Parse(Mask20.Text), double.Parse(Mask21.Text), double.Parse(Mask22.Text) }
                };

                _bitmap = ApplyConvolution(_bitmap, mask);
                UpdatedBitmap = _bitmap;
                this.DialogResult = true;
            }
            catch
            {
                MessageBox.Show("Invalid mask values.");
            }
        }

        private void BtnLoadMask_Click(object sender, RoutedEventArgs e)
        {
            var maskName = (sender as Button)?.Tag.ToString();
            switch (maskName)
            {
                case "LowPass":
                    SetMaskValues(new double[3, 3]
                    {
                        { 1 / 9.0, 1 / 9.0, 1 / 9.0 },
                        { 1 / 9.0, 1 / 9.0, 1 / 9.0 },
                        { 1 / 9.0, 1 / 9.0, 1 / 9.0 }
                    });
                    break;
                case "Prewitt":
                    SetMaskValues(new double[3, 3]
                    {
                        { -1, 0, 1 },
                        { -1, 0, 1 },
                        { -1, 0, 1 }
                    }); 
                    break;
                case "Sobel":
                    SetMaskValues(new double[3, 3]
                    {
                        { -1, 0, 1 },
                        { -2, 0, 2 },
                        { -1, 0, 1 }
                    });
                    break;
                case "Laplace":
                    SetMaskValues(new double[3, 3]
                    {   
                        { 0, -1, 0 },
                        { -1, 4, -1 },
                        { 0, -1, 0 }
                    });
                    break;
                case "Corner":
                    SetMaskValues(new double[3, 3]
                    {
                        { 1, 1, 1 },
                        { 1, -2, -1 },
                        { 1, -1, -1 }
                    });
                    break;
            }
        }

        private void BtnApplyMedian3x3_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) 
                return;

            _bitmap = ApplyMedianFilter(_bitmap, 3);
            UpdatedBitmap = _bitmap;
            this.DialogResult = true;
        }

        private void BtnApplyMedian5x5_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) 
                return;

            _bitmap = ApplyMedianFilter(_bitmap, 5);
            UpdatedBitmap = _bitmap;
            this.DialogResult = true;
        }

        private void SetMaskValues(double[,] mask)
        {
            Mask00.Text = mask[0, 0].ToString();
            Mask01.Text = mask[0, 1].ToString();
            Mask02.Text = mask[0, 2].ToString();
            Mask10.Text = mask[1, 0].ToString();
            Mask11.Text = mask[1, 1].ToString();
            Mask12.Text = mask[1, 2].ToString();
            Mask20.Text = mask[2, 0].ToString();
            Mask21.Text = mask[2, 1].ToString();
            Mask22.Text = mask[2, 2].ToString();
        }

        private Bitmap ApplyConvolution(Bitmap bitmap, double[,] mask)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap newBitmap = new Bitmap(width, height);

            int maskWidth = mask.GetLength(1);
            int maskHeight = mask.GetLength(0);
            int offsetX = maskWidth / 2;
            int offsetY = maskHeight / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    for (int ky = 0; ky < maskHeight; ky++)
                    {
                        for (int kx = 0; kx < maskWidth; kx++)
                        {
                            int pixelX = Math.Min(width - 1, Math.Max(0, x + kx - offsetX));
                            int pixelY = Math.Min(height - 1, Math.Max(0, y + ky - offsetY));

                            System.Drawing.Color pixel = bitmap.GetPixel(pixelX, pixelY);

                            double maskValue = mask[ky, kx];
                            rSum += pixel.R * maskValue;
                            gSum += pixel.G * maskValue;
                            bSum += pixel.B * maskValue;
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
                    // zbierz piksele z okna maski
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

                    // znajdź medianę dla każdego kanału
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