using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestureGUI.Pages
{
    
    public partial class LandingPage : UserControl
    {
        public event Action<string> OnPageSelected;

        private double centerX = 200;
        private double centerY = 200;
        private double radius = 150;

        public LandingPage()
        {
            InitializeComponent();
            CreatePieMenu();
        }

        private void CreatePieMenu()
        {
            AddSlice("TUIO", 0, 120, Brushes.MediumSeaGreen, "tuio.png");
            AddSlice("Bluetooth", 120, 240,Brushes.DodgerBlue, "bluetooth.png");
            AddSlice("Tutorial", 240, 360, Brushes.IndianRed, "tutorial.png");
        }

        private void AddSlice(string name, double startAngle, double endAngle, Brush color, string iconPath)
        {
            Path slice = new Path
            {
                Fill = color,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Tag = name
            };

            slice.Data = CreatePieSliceGeometry(startAngle, endAngle);
            slice.MouseLeftButtonDown += Slice_Click;

            PieCanvas.Children.Add(slice);

          
            double midAngle = (startAngle + endAngle) / 2;
            double midRad = midAngle * Math.PI / 180;

            double contentRadius = radius * 0.6;

            double x = centerX + contentRadius * Math.Cos(midRad);
            double y = centerY + contentRadius * Math.Sin(midRad);

            
            Image icon = new Image
            {
                Width = 50,
                Height = 50,
                Source = new System.Windows.Media.Imaging.BitmapImage(
                         new Uri($"pack://application:,,,/Images/{iconPath}")
)
            };

            Canvas.SetLeft(icon, x - 25);
            Canvas.SetTop(icon, y - 40);

            PieCanvas.Children.Add(icon);

            
            TextBlock label = new TextBlock
            {
                Text = name,
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Width = 100
            };

            Canvas.SetLeft(label, x - 50);
            Canvas.SetTop(label, y + 10);

            PieCanvas.Children.Add(label);
        }

        private Geometry CreatePieSliceGeometry(double startAngle, double endAngle)
        {
            double startRad = startAngle * Math.PI / 180;
            double endRad = endAngle * Math.PI / 180;

            Point startPoint = new Point(
                centerX + radius * Math.Cos(startRad),
                centerY + radius * Math.Sin(startRad));

            Point endPoint = new Point(
                centerX + radius * Math.Cos(endRad),
                centerY + radius * Math.Sin(endRad));

            bool isLargeArc = (endAngle - startAngle) > 180;

            PathFigure figure = new PathFigure
            {
                StartPoint = new Point(centerX, centerY),
                IsClosed = true
            };

            figure.Segments.Add(new LineSegment(startPoint, true));
            figure.Segments.Add(new ArcSegment(
                endPoint,
                new Size(radius, radius),
                0,
                isLargeArc,
                SweepDirection.Clockwise,
                true));
            figure.Segments.Add(new LineSegment(new Point(centerX, centerY), true));

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            return geometry;
        }

        private void Slice_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Path slice)
            {
                string selected = slice.Tag.ToString();
                OnPageSelected?.Invoke(selected);
            }
        }
    }
}
