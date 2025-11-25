using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace Biometria
{
    public partial class BinarizeWindow : Window
    {
        private Bitmap? _bitmap;
        public Bitmap? UpdatedBitmap { get; private set; }

        public BinarizeWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _bitmap = bitmap;
        }

        private void BtnApplyManual_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int threshold = 128;

            if (int.TryParse(ThresholdTextBox.Text, out int parsed))
                threshold = parsed;

            UpdatedBitmap = BinarizeManual(_bitmap, threshold);
            this.DialogResult = true;
        }

        private Bitmap BinarizeManual(Bitmap bitmap, int threshold)
        {
            Bitmap result = new Bitmap(bitmap.Width, bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var p = bitmap.GetPixel(x, y);
                    int g = (p.R + p.G + p.B) / 3;
                    int bw = g >= threshold ? 255 : 0;

                    result.SetPixel(x, y, Color.FromArgb(bw, bw, bw));
                }
            }

            return result;
        }

        private void BtnApplyPBS_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int percent = 50;

            if (int.TryParse(PercentBlackTextBox.Text, out int p))
                percent = Math.Clamp(p, 0, 100);

            UpdatedBitmap = BinarizePBS(_bitmap, percent);
            this.DialogResult = true;
        }

        private Bitmap BinarizePBS(Bitmap bmp, int percentBlack)
        {
            int[] hist = new int[256];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var p = bmp.GetPixel(x, y);
                    int g = (p.R + p.G + p.B) / 3;
                    hist[g]++;
                }
            }

            int totalPixels = bmp.Width * bmp.Height;
            int requiredBlack = (percentBlack * totalPixels) / 100;

            int cumulative = 0;
            int threshold = 0;

            for (int i = 0; i < 256; i++)
            {
                cumulative += hist[i];
                if (cumulative >= requiredBlack)
                {
                    threshold = i;
                    break;
                }
            }

            return BinarizeManual(bmp, threshold);
        }

        private void BtnApplyMIS_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            UpdatedBitmap = BinarizeMIS(_bitmap);
            this.DialogResult = true;
        }

        private Bitmap BinarizeMIS(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            int[] hist = new int[256];
            long[] histTimesIntensity = new long[256];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var p = bmp.GetPixel(x, y);
                    int g = (p.R + p.G + p.B) / 3;

                    hist[g]++;
                    histTimesIntensity[g] += g;
                }
            }

            long totalPixels = width * height;
            long totalSum = 0;

            for (int i = 0; i < 256; i++)
                totalSum += histTimesIntensity[i];

            int threshold = (int)(totalSum / totalPixels);

            while (true)
            {
                long sumA = 0, countA = 0;
                long sumB = 0, countB = 0;

                for (int i = 0; i < threshold; i++)
                {
                    sumA += histTimesIntensity[i];
                    countA += hist[i];
                }

                for (int i = threshold; i < 256; i++)
                {
                    sumB += histTimesIntensity[i];
                    countB += hist[i];
                }

                if (countA == 0 || countB == 0)
                    break;

                int meanA = (int)(sumA / countA);
                int meanB = (int)(sumB / countB);

                int newThreshold = (meanA + meanB) / 2;

                if (newThreshold == threshold)
                    break;

                threshold = newThreshold;
            }

            return BinarizeManual(bmp, threshold);
        }

        private void BtnApplyOtsu_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) return;

            int threshold = OtsuThreshold(_bitmap);
            UpdatedBitmap = ApplyOtsu(_bitmap, threshold);
            this.DialogResult = true;
        }

        private int OtsuThreshold(Bitmap bitmap)
        {
            int[] histogram = new int[256];

            for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color p = bitmap.GetPixel(x, y);
                int gray = (p.R + p.G + p.B) / 3;
                histogram[gray]++;
            }

            int total = bitmap.Width * bitmap.Height;
            float sum = histogram.Select((t, i) => i * t).Sum();
            float sumB = 0, wB = 0, maxVar = 0;
            int threshold = 0;

            for (int i = 0; i < 256; i++)
            {
                wB += histogram[i];
                if (wB == 0) continue;
                float wF = total - wB;
                if (wF == 0) break;
                sumB += i * histogram[i];
                float mB = sumB / wB;
                float mF = (sum - sumB) / wF;
                float varBetween = wB * wF * (mB - mF) * (mB - mF);
                if (varBetween > maxVar)
                {
                    maxVar = varBetween;
                    threshold = i;
                }
            }
            return threshold;
        }

        private Bitmap ApplyOtsu(Bitmap bitmap, int threshold)
        {
            return BinarizeManual(bitmap, threshold);
        }

        private void BtnApplyNiblack_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) return;

            int windowSize = 15;
            double k = -0.2;

            int.TryParse(NiblackSizeTextBox.Text, out windowSize);
            double.TryParse(NiblackKTextBox.Text, out k);

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