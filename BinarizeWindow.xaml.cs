using System.Drawing;
using System.Windows;

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

        private void BtnApplyBinarization_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            int threshold = 128; // domyślny threshold

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

                    // ustawienie nowego piksela jako czarno-biały
                    binaryBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(binary, binary, binary));
                }
            }
            return binaryBitmap;
        }
    }
}
