using System.Drawing;
using System.Windows;
using Color = System.Drawing.Color;

namespace Biometria
{
    public partial class PointOperationsWindow : Window
    {
        private Bitmap? _bitmap;
        public Bitmap? UpdatedBitmap { get; private set; }

        public PointOperationsWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _bitmap = bitmap;
        }

        private int Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }

        private Bitmap ApplyOperation(Func<Color, Color> operation)
        {
            if (_bitmap == null) return null;

            Bitmap newBitmap = new Bitmap(_bitmap.Width, _bitmap.Height);

            for (int y = 0; y < _bitmap.Height; y++)
                for (int x = 0; x < _bitmap.Width; x++)
                {
                    Color original = _bitmap.GetPixel(x, y);
                    Color modified = operation(original);
                    newBitmap.SetPixel(x, y, modified);
                }

            return newBitmap;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(AddBox.Text, out int value) || _bitmap == null)
                return;

            UpdatedBitmap = ApplyOperation(p =>
                Color.FromArgb(Clamp(p.R + value), Clamp(p.G + value), Clamp(p.B + value)));

            DialogResult = true;
        }

        private void BtnSub_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SubBox.Text, out int value) || _bitmap == null)
                return;

            UpdatedBitmap = ApplyOperation(p =>
                Color.FromArgb(Clamp(p.R - value), Clamp(p.G - value), Clamp(p.B - value)));

            DialogResult = true;
        }

        private void BtnMul_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(MulBox.Text, out double value) || _bitmap == null)
                return;

            UpdatedBitmap = ApplyOperation(p =>
                Color.FromArgb(Clamp((int)(p.R * value)), Clamp((int)(p.G * value)), Clamp((int)(p.B * value))));

            DialogResult = true;
        }

        private void BtnDiv_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(DivBox.Text, out double value) || _bitmap == null || value == 0)
                return;

            UpdatedBitmap = ApplyOperation(p =>
                Color.FromArgb(Clamp((int)(p.R / value)), Clamp((int)(p.G / value)), Clamp((int)(p.B / value))));

            DialogResult = true;
        }

        private void BtnBrightness_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(BrightnessBox.Text, out int value) || _bitmap == null)
                return;

            UpdatedBitmap = ApplyOperation(p =>
                Color.FromArgb(Clamp(p.R + value), Clamp(p.G + value), Clamp(p.B + value)));

            DialogResult = true;
        }

        private void BtnGrayAvg_Click(object sender, RoutedEventArgs e)
        {
            UpdatedBitmap = ApplyOperation(p =>
            {
                int g = (p.R + p.G + p.B) / 3;
                return Color.FromArgb(g, g, g);
            });

            DialogResult = true;
        }

        private void BtnGrayNtsc_Click(object sender, RoutedEventArgs e)
        {
            UpdatedBitmap = ApplyOperation(p =>
            {
                int g = Clamp((int)(0.299 * p.R + 0.587 * p.G + 0.114 * p.B));
                return Color.FromArgb(g, g, g);
            });

            DialogResult = true;
        }
    }
}
