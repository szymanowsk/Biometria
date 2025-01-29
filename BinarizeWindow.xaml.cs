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
    /// Interaction logic for BinarizeWindow.xaml
    /// </summary>
    public partial class BinarizeWindow : Window
    {
        private Bitmap? _bitmap;
        public Bitmap? UpdatedBitmap { get; private set; }

        public BinarizeWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _bitmap = bitmap;
        }

        private void BtnApplyBinarization_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int threshold = 128; // Default threshold

            if (int.TryParse(ThresholdTextBox.Text, out int parsedThreshold))
            {
                threshold = parsedThreshold;
            }

            UpdatedBitmap = Binarize(_bitmap, threshold);
            this.DialogResult = true;
        }

        private Bitmap Binarize(Bitmap bitmap, int threshold)
        {
            Bitmap binaryBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    System.Drawing.Color pixel = bitmap.GetPixel(x, y);

                    int gray = (pixel.R + pixel.G + pixel.B) / 3;

                    int binary = gray >= threshold ? 255 : 0;

                    // Ustawienie nowego piksela jako czarno-biały
                    binaryBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(binary, binary, binary));
                }
            }
            return binaryBitmap;
        }
    }
}
