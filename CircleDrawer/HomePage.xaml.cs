using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CircleDrawer
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }
                
        private void addEllipseToCanvas(Canvas canvas, Ellipse ellipse, double centerX, double centerY, double offset)
        {
            if (ellipse.Width != ellipse.Height)
            {
                MessageBox.Show("input is not circle");
                return;
            }
            double windowLeftMargin = 200 + offset;
            double windowTopMargin = 200 + offset;
            Canvas.SetLeft(ellipse, centerX + windowLeftMargin - ellipse.Width / 2);
            Canvas.SetTop(ellipse, centerY + windowTopMargin - ellipse.Width / 2);
            canvas.Children.Add(ellipse);
        }

        private List<double> GetSortedInputDiametersFromFile(OpenFileDialog openFileDialog)
        {
            string line;
            double diameter;
            List<double> sortedDiameters = new List<double>();

            using (StreamReader reader = new StreamReader(openFileDialog.FileName))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (double.TryParse(line, out diameter))
                    {
                        sortedDiameters.Add(ConvertMmToPixels(diameter)); //input in mm here
                    }
                }
            }
            return sortedDiameters.OrderByDescending(i => i).ToList();
        }

        private void SetCanvasSize(Canvas canvas, double size)
        {
            double width = size > SystemParameters.PrimaryScreenHeight ? size : SystemParameters.PrimaryScreenHeight;
            double heigth = size > SystemParameters.PrimaryScreenWidth ? size : SystemParameters.PrimaryScreenWidth;

            canvas.Height = heigth;
            canvas.Width = width;
        }

        private void AddCirclesToCanvas(Canvas canvas, List<Circle> circles)
        {
            double maxRadius = circles.Select(c => c.Radius).Max();
            
            foreach (Circle circle in circles)
            {
                try
                {
                    addEllipseToCanvas(canvas, circle.CircleShape, circle.CenterX, circle.CenterY, maxRadius);
                }
                catch 
                {
                    MessageBox.Show("some circle is null, calculation wrong");
                }
            }
        }

        private void AddInfoLabelsToCanvas(Canvas canvas, Stopwatch stopwatch, List<Circle> circles)
        {
            Label timeElapsedLabel = new Label();
            timeElapsedLabel.Content = string.Format("Elapsed={0} ms", stopwatch.Elapsed.TotalMilliseconds);

            Label circumscribedDiameterLabel = new Label();
            circumscribedDiameterLabel.Content = string.Format("Diameter={0} mm", 2 * ConvertPixelsToMm((circles.Last()?.Radius ?? 0)));
            Canvas.SetTop(circumscribedDiameterLabel, 20);

            Label circumscribedOptimalDiameterLabel = new Label();
            circumscribedOptimalDiameterLabel.Content = string.Format("Optimality rate={0}", CirclePlacementManager.GetOptimalMinimalCircumscribedRadius(circles.Take(circles.Count - 1).ToList()) / circles.Last()?.Radius);
            Canvas.SetTop(circumscribedOptimalDiameterLabel, 30);

            Label numberOfCirclesLabel = new Label();
            numberOfCirclesLabel.Content = string.Format("n={0}", circles.Count - 1);
            Canvas.SetTop(numberOfCirclesLabel, 40);
            
            Label circumElaps = new Label();
            circumElaps.Content = string.Format("time elapsed on finding circumscribed={0} ms", CirclePlacementManager.circumfindingDuration);
            Canvas.SetTop(circumElaps, 50);

            Label circumFindingCount = new Label();
            circumFindingCount.Content = string.Format("number of computations of circumscribed={0}", CirclePlacementManager.circumscribedCalculationCount);
            Canvas.SetTop(circumFindingCount, 60);

            canvas.Children.Add(timeElapsedLabel);
            canvas.Children.Add(circumscribedDiameterLabel);
            canvas.Children.Add(circumscribedOptimalDiameterLabel);
            canvas.Children.Add(numberOfCirclesLabel);
            canvas.Children.Add(circumElaps);
            canvas.Children.Add(circumFindingCount);
        }

        private ScrollViewer SetScrollViewer(Canvas canvas)
        {
            ScrollViewer sv = new ScrollViewer();
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            sv.CanContentScroll = true;
            sv.Content = canvas;

            return sv;
        }

        private double ConvertMmToPixels(double mm)
        {
            //assume dpi 96
            return Math.Round((mm * 96) / 25.4, 3);           
        }

        private double ConvertPixelsToMm(double pixels)
        {
            return Math.Round((pixels * 25.4) / 96);
        }

        private void NavigateToPageWithSolution(Func<List<Double>, List<Circle>> getCircles)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            RenderPage renderedPage = new RenderPage();
            Canvas canvas = new Canvas();
            
            if (openFileDialog.ShowDialog() == true)
            {

                List<Double> sortedDiameters = GetSortedInputDiametersFromFile(openFileDialog);

                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();
                var circles = getCircles(sortedDiameters);
                stopwatch.Stop();

                AddCirclesToCanvas(canvas, circles);
                AddInfoLabelsToCanvas(canvas, stopwatch, circles);
                SetCanvasSize(canvas, circles.Select(c => c.Radius).Max() * 3);
                renderedPage.Content = SetScrollViewer(canvas);
                
                NavigationService.Navigate(renderedPage);
            }
        }

        private void Alg2ButtonClick(object sender, RoutedEventArgs e)
        {            
            NavigateToPageWithSolution(CirclePlacementManager.GetPlacementGreedilyHoldingCircumscribed);
        }

        private void Alg1ButtonClick(object sender, RoutedEventArgs e)
        {
            NavigateToPageWithSolution(CirclePlacementManager.GetPlacementGreedily);
        }
    }
}
