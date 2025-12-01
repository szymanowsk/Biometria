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

        private void BtnApplyEntropy_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) return;
            int threshold = EntropyThreshold(_bitmap);
            UpdatedBitmap = BinarizeManual(_bitmap, threshold);
            this.DialogResult = true;
        }

        private int EntropyThreshold(Bitmap bmp)
        {
            int[] hist = new int[256];
            int width = bmp.Width;
            int height = bmp.Height;
            int total = width * height;

            // histogram
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Color p = bmp.GetPixel(x, y);
                    int g = (p.R + p.G + p.B) / 3;
                    hist[g]++;
                }

            double[] pHist = hist.Select(h => (double)h / total).ToArray();

            double maxEntropy = double.NegativeInfinity;
            int bestThreshold = 0;

            for (int t = 1; t < 255; t++)
            {
                double pT = pHist.Take(t).Sum();
                double pB = 1 - pT;

                if (pT == 0 || pB == 0) continue;

                double hT = 0.0;
                double hB = 0.0;

                for (int i = 0; i < t; i++)
                    if (pHist[i] > 0)
                        hT -= (pHist[i] / pT) * Math.Log(pHist[i] / pT);

                for (int i = t; i < 256; i++)
                    if (pHist[i] > 0)
                        hB -= (pHist[i] / pB) * Math.Log(pHist[i] / pB);

                double entropy = hT + hB;

                if (entropy > maxEntropy)
                {
                    maxEntropy = entropy;
                    bestThreshold = t;
                }
            }

            return bestThreshold;
        }

        private void BtnApplyMinError_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) return;
            int threshold = MinimumErrorThreshold(_bitmap);
            UpdatedBitmap = BinarizeManual(_bitmap, threshold);
            this.DialogResult = true;
        }

        private int MinimumErrorThreshold(Bitmap bmp)
        {
            int[] hist = new int[256];
            int width = bmp.Width, height = bmp.Height;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Color p = bmp.GetPixel(x, y);
                    int g = (p.R + p.G + p.B) / 3;
                    hist[g]++;
                }

            int total = width * height;

            double minJ = double.PositiveInfinity;
            int threshold = 0;

            for (int t = 1; t < 255; t++)
            {
                double w0 = hist.Take(t).Sum() / (double)total;
                double w1 = 1 - w0;

                if (w0 == 0 || w1 == 0) continue;

                double mean0 = 0, mean1 = 0;
                double var0 = 0, var1 = 0;

                for (int i = 0; i < t; i++)
                    mean0 += i * hist[i];
                mean0 /= hist.Take(t).Sum();

                for (int i = t; i < 256; i++)
                    mean1 += i * hist[i];
                mean1 /= hist.Skip(t).Sum();

                for (int i = 0; i < t; i++)
                    var0 += hist[i] * (i - mean0) * (i - mean0);
                var0 /= hist.Take(t).Sum();

                for (int i = t; i < 256; i++)
                    var1 += hist[i] * (i - mean1) * (i - mean1);
                var1 /= hist.Skip(t).Sum();

                if (var0 <= 0 || var1 <= 0) continue;

                double J = 1 + 2 * (w0 * Math.Log(Math.Sqrt(var0)) + w1 * Math.Log(Math.Sqrt(var1)))
                           - 2 * (w0 * Math.Log(w0) + w1 * Math.Log(w1));

                if (J < minJ)
                {
                    minJ = J;
                    threshold = t;
                }
            }

            return threshold;
        }

        private void BtnApplyFuzzy_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) return;
            int threshold = FuzzyMinimumErrorThreshold(_bitmap);
            UpdatedBitmap = BinarizeManual(_bitmap, threshold);
            this.DialogResult = true;
        }

        private int FuzzyMinimumErrorThreshold(Bitmap bmp)
        {
            int[] hist = new int[256];
            int width = bmp.Width, height = bmp.Height;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Color p = bmp.GetPixel(x, y);
                    int g = (p.R + p.G + p.B) / 3;
                    hist[g]++;
                }

            int total = width * height;
            int bestT = 0;
            double bestCost = double.PositiveInfinity;

            for (int t = 1; t < 255; t++)
            {
                double μ0 = hist.Take(t).Sum() / (double)total;
                double μ1 = 1 - μ0;

                if (μ0 == 0 || μ1 == 0) continue;

                double mean0 = 0, mean1 = 0;
                double var0 = 0, var1 = 0;

                for (int i = 0; i < t; i++)
                    mean0 += i * hist[i];
                mean0 /= hist.Take(t).Sum();

                for (int i = t; i < 256; i++)
                    mean1 += i * hist[i];
                mean1 /= hist.Skip(t).Sum();

                for (int i = 0; i < t; i++)
                    var0 += hist[i] * Math.Pow(i - mean0, 2);
                var0 /= hist.Take(t).Sum();

                for (int i = t; i < 256; i++)
                    var1 += hist[i] * Math.Pow(i - mean1, 2);
                var1 /= hist.Skip(t).Sum();

                if (var0 <= 0 || var1 <= 0) continue;

                // fuzzy entropy-based cost function
                double cost = μ0 * Math.Log(var0) + μ1 * Math.Log(var1)
                              - μ0 * Math.Log(μ0) - μ1 * Math.Log(μ1);

                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestT = t;
                }
            }

            return bestT;
        }
    }
}