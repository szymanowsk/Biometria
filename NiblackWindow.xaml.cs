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

namespace Biometria
{
    /// <summary>
    /// Interaction logic for NiblackWindow.xaml
    /// </summary>
    public partial class NiblackWindow : Window
    {
        private Bitmap? _bitmap;
        public Bitmap? UpdatedBitmap { get; private set; }

        public NiblackWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _bitmap = bitmap;
        }

        private void BtnApplyNiblack_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int windowSize = 15; // default value
            double k = -0.2; // default value

            if (int.TryParse(SizeTextBox.Text, out int parsedSize))
            {
                windowSize = parsedSize;
            }

            if (double.TryParse(KTextBox.Text, out double parsedK))
            {
                k = parsedK;
            }

            UpdatedBitmap = ApplyNiblackThreshold(_bitmap, windowSize, k);
            this.DialogResult = true;
        }

        private Bitmap ApplyNiblackThreshold(Bitmap bitmap, int windowSize, double k)
        {
            Bitmap binaryBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            int halfSize = windowSize / 2;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    List<int> pixelValues = new List<int>();
                    for (int wy = -halfSize; wy <= halfSize; wy++)
                    {
                        for (int wx = -halfSize; wx <= halfSize; wx++)
                        {
                            int px = Math.Clamp(x + wx, 0, bitmap.Width - 1);
                            int py = Math.Clamp(y + wy, 0, bitmap.Height - 1);
                            pixelValues.Add(bitmap.GetPixel(px, py).R);
                        }
                    }
                    double mean = pixelValues.Average();
                    double stdDev = Math.Sqrt(pixelValues.Average(p => Math.Pow(p - mean, 2)));
                    int threshold = (int)(mean + k * stdDev);

                    binaryBitmap.SetPixel(x, y, bitmap.GetPixel(x, y).R >= threshold ? System.Drawing.Color.White : System.Drawing.Color.Black);
                }
            }
            return binaryBitmap;
        }
    }
}
