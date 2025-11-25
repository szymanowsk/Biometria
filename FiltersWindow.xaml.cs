using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace Biometria
{
    public partial class FiltersWindow : Window
    {
        private Bitmap? _bitmap;
        public Bitmap? UpdatedBitmap { get; private set; }
        private List<TextBox> _maskTextBoxes = new List<TextBox>();
        private int _currentMaskSize = 3;

        public FiltersWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _bitmap = bitmap;
            InitializeMaskGrid(3);

            if (CmbMaskSize != null && CmbMaskSize.Items.Count > 0)
            {
                CmbMaskSize.SelectedIndex = 0;
            }
        }

        private void CmbMaskSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMaskSize?.SelectedItem is ComboBoxItem item)
            {
                var content = item.Content?.ToString();
                switch (content)
                {
                    case "3x3":
                        if (CustomSizeContainer != null)
                            CustomSizeContainer.Visibility = Visibility.Collapsed;
                        InitializeMaskGrid(3);
                        break;
                    case "5x5":
                        if (CustomSizeContainer != null)
                            CustomSizeContainer.Visibility = Visibility.Collapsed;
                        InitializeMaskGrid(5);
                        break;
                    case "7x7":
                        if (CustomSizeContainer != null)
                            CustomSizeContainer.Visibility = Visibility.Collapsed;
                        InitializeMaskGrid(7);
                        break;
                    case "Custom":
                        if (CustomSizeContainer != null)
                            CustomSizeContainer.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private void BtnApplyCustomSize_Click(object sender, RoutedEventArgs e)
        {
            if (TxtCustomSize != null && int.TryParse(TxtCustomSize.Text, out int size) && size > 0 && size % 2 == 1)
            {
                InitializeMaskGrid(size);
            }
            else
            {
                MessageBox.Show("Please enter a positive odd number for mask size.");
            }
        }

        private void InitializeMaskGrid(int size)
        {
            _currentMaskSize = size;
            _maskTextBoxes.Clear();

            if (MaskGridContainer != null)
            {
                Grid grid = new Grid();
                grid.HorizontalAlignment = HorizontalAlignment.Center;
                grid.VerticalAlignment = VerticalAlignment.Center;

                for (int i = 0; i < size; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                }

                for (int row = 0; row < size; row++)
                {
                    for (int col = 0; col < size; col++)
                    {
                        var textBox = new TextBox
                        {
                            Width = 40,
                            Height = 25,
                            Margin = new Thickness(2),
                            Text = "0",
                            TextAlignment = TextAlignment.Center
                        };

                        Grid.SetRow(textBox, row);
                        Grid.SetColumn(textBox, col);

                        _maskTextBoxes.Add(textBox);
                        grid.Children.Add(textBox);
                    }
                }

                MaskGridContainer.Content = grid;
            }
        }

        private void BtnApplyCustomMask_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null)
                return;

            try
            {
                double[,] mask = new double[_currentMaskSize, _currentMaskSize];

                for (int i = 0; i < _maskTextBoxes.Count; i++)
                {
                    int row = i / _currentMaskSize;
                    int col = i % _currentMaskSize;
                    if (double.TryParse(_maskTextBoxes[i].Text, out double value))
                    {
                        mask[row, col] = value;
                    }
                    else
                    {
                        MessageBox.Show($"Invalid value at position ({row + 1}, {col + 1})");
                        return;
                    }
                }

                _bitmap = ApplyConvolution(_bitmap, mask);
                UpdatedBitmap = _bitmap;
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filter: {ex.Message}");
            }
        }

        private void BtnLoadMask_Click(object sender, RoutedEventArgs e)
        {
            var maskName = (sender as Button)?.Tag?.ToString();
            if (maskName == null) return;

            double[,] mask;

            switch (maskName)
            {
                case "LowPass":
                    mask = new double[3, 3]
                    {
                        { 1 / 9.0, 1 / 9.0, 1 / 9.0 },
                        { 1 / 9.0, 1 / 9.0, 1 / 9.0 },
                        { 1 / 9.0, 1 / 9.0, 1 / 9.0 }
                    };
                    break;
                case "HighPass":
                    mask = new double[3, 3]
                    {
                        {  0, -1,  0 },
                        { -1,  5, -1 },
                        {  0, -1,  0 }
                    };
                    break;
                case "Prewitt":
                    mask = new double[3, 3]
                    {
                        { -1, 0, 1 },
                        { -1, 0, 1 },
                        { -1, 0, 1 }
                    };
                    break;
                case "Sobel":
                    mask = new double[3, 3]
                    {
                        { -1, 0, 1 },
                        { -2, 0, 2 },
                        { -1, 0, 1 }
                    };
                    break;
                case "Laplace":
                    mask = new double[3, 3]
                    {
                        { 0, -1, 0 },
                        { -1, 4, -1 },
                        { 0, -1, 0 }
                    };
                    break;
                case "Corner":
                    mask = new double[3, 3]
                    {
                        { 1, 1, 1 },
                        { 1, -2, 1 },
                        { 1, 1, 1 }
                    };
                    break;
                case "Gaussian3":
                    mask = new double[3, 3]
                    {
                        { 1/16.0, 2/16.0, 1/16.0 },
                        { 2/16.0, 4/16.0, 2/16.0 },
                        { 1/16.0, 2/16.0, 1/16.0 }
                    };
                    break;

                case "Gaussian5":
                    mask = new double[5, 5]
                    {
                        { 1/273.0,  4/273.0,  7/273.0,  4/273.0, 1/273.0 },
                        { 4/273.0, 16/273.0, 26/273.0, 16/273.0, 4/273.0 },
                        { 7/273.0, 26/273.0, 41/273.0, 26/273.0, 7/273.0 },
                        { 4/273.0, 16/273.0, 26/273.0, 16/273.0, 4/273.0 },
                        { 1/273.0,  4/273.0,  7/273.0,  4/273.0, 1/273.0 }
                    };
                    break;
                default:
                    return;
            }

            InitializeMaskGrid(mask.GetLength(0));
            SetMaskValues(mask);
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
            int size = mask.GetLength(0);
            for (int i = 0; i < size * size && i < _maskTextBoxes.Count; i++)
            {
                int row = i / size;
                int col = i % size;
                _maskTextBoxes[i].Text = mask[row, col].ToString("F4");
            }
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

                    int r = Math.Min(255, Math.Max(0, (int)Math.Round(rSum)));
                    int g = Math.Min(255, Math.Max(0, (int)Math.Round(gSum)));
                    int b = Math.Min(255, Math.Max(0, (int)Math.Round(bSum)));

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