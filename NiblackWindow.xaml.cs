using System.Drawing;
using System.Windows;

namespace Biometria
{
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

            int windowSize = 15; // domyślna wartość
            double k = -0.2; // domyślna wartość

            if (int.TryParse(SizeTextBox.Text, out int parsedSize))
            {
                windowSize = parsedSize;
            }

            if (double.TryParse(KTextBox.Text, out double parsedK))
            {
                k = parsedK;
            }

            UpdatedBitmap = ApplyNiblack(_bitmap, windowSize, k);
            this.DialogResult = true;
        }

        private Bitmap ApplyNiblack(Bitmap bitmap, int windowSize, double k)
        {
            Bitmap binaryBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            int halfSize = windowSize / 2;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int sum = 0;
                    List<int> values = new List<int>();

                    for (int wy = -halfSize; wy <= halfSize; wy++)
                    {
                        for (int wx = -halfSize; wx <= halfSize; wx++)
                        {
                            int pX = Math.Clamp(x + wx, 0, bitmap.Width - 1);
                            int pY = Math.Clamp(y + wy, 0, bitmap.Height - 1);

                            System.Drawing.Color pColor = bitmap.GetPixel(pX, pY);
                            int gray = (pColor.R + pColor.G + pColor.B) / 3;

                            sum += gray;
                            values.Add(gray);
                        }
                    }

                    double avg = (double)sum / values.Count;
                    double varSum = values.Sum(value => Math.Pow(value - avg, 2));
                    double variance = Math.Sqrt(varSum / values.Count);
                    double threshold = avg + k * variance;

                    System.Drawing.Color currentColor = bitmap.GetPixel(x, y);
                    int currentGray = (currentColor.R + currentColor.G + currentColor.B) / 3;

                    System.Drawing.Color newColor = currentGray > threshold ? System.Drawing.Color.White : System.Drawing.Color.Black;
                    binaryBitmap.SetPixel(x, y, newColor);
                }
            }
            return binaryBitmap;
        }
    }
}
