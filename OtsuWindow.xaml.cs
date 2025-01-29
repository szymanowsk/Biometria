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
    /// Interaction logic for OtsuWindow.xaml
    /// </summary>
    public partial class OtsuWindow : Window
    {
        private Bitmap? _bitmap;
        public Bitmap? UpdatedBitmap { get; private set; }


        public OtsuWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _bitmap = bitmap;
        }

        private void BtnApplyOtsu_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int threshold = OtsuThreshold(_bitmap);
            UpdatedBitmap = ApplyOtsu(_bitmap, threshold);
            this.DialogResult = true;
        }

        private int OtsuThreshold(Bitmap bitmap)
        {
            int[] histogram = new int[256];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int gray = (bitmap.GetPixel(x, y).R + bitmap.GetPixel(x, y).G + bitmap.GetPixel(x, y).B) / 3;
                    histogram[gray]++;
                }
            }

            int totalPixels = bitmap.Width * bitmap.Height;
            float sum = 0;
            for (int i = 0; i < 256; i++) sum += i * histogram[i];
            float sumB = 0, wB = 0, wF = 0, maxVariance = 0;
            int threshold = 0;

            for (int i = 0; i < 256; i++)
            {
                wB += histogram[i];
                if (wB == 0) continue;
                wF = totalPixels - wB;
                if (wF == 0) break;
                sumB += i * histogram[i];
                float mB = sumB / wB;
                float mF = (sum - sumB) / wF;
                float variance = wB * wF * (mB - mF) * (mB - mF);
                if (variance > maxVariance)
                {
                    maxVariance = variance;
                    threshold = i;
                }
            }
            return threshold;
        }

        private Bitmap ApplyOtsu(Bitmap bitmap, int threshold)
        {
            Bitmap binaryBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    binaryBitmap.SetPixel(x, y, bitmap.GetPixel(x, y).R >= threshold ? System.Drawing.Color.White : System.Drawing.Color.Black);
            return binaryBitmap;
        }
    }
}
