using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CircleDrawer
{
    public class Circle
    {
        public double CenterX { get; set; }

        public double CenterY { get; set; }

        public double Radius { get; set; }

        public Ellipse CircleShape { get; set; }

        private const double Threshold = 0.001d;

        public Circle(double centerX, double centerY, double radius)
        {
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
            CircleShape = new Ellipse();
            CircleShape.Width = radius * 2;
            CircleShape.Height = radius * 2;
            CircleShape.StrokeThickness = 1;
            CircleShape.Stroke = Brushes.Black;
            CircleShape.HorizontalAlignment = HorizontalAlignment.Center;
            CircleShape.VerticalAlignment = VerticalAlignment.Center;
        }

        public double GetArea()
        {
            return 2 * Math.PI * Radius * Radius;
        }

        public static double GetDistanceOfCenters(Circle circle1, Circle circle2)
        {
            double differenceX = circle1.CenterX - circle2.CenterX;
            double differenceY = circle1.CenterY - circle2.CenterY;
            return Math.Round(Math.Sqrt(differenceX * differenceX + differenceY * differenceY), 3);
        }

        public static bool EqualsGeometrically(Circle circle1, Circle circle2)
        {
            return (circle1.CenterX == circle2.CenterX && circle1.CenterY == circle2.CenterY && circle1.Radius == circle2.Radius);
        }

        public static int GetNumberOfIntersectionPoints(Circle circle1, Circle circle2)
        {
            if (EqualsGeometrically(circle1, circle2))
            {
                return 3; //temporary
            }
            
            double radiiSum = Math.Round(circle1.Radius + circle2.Radius, 4);
            double distanceOfCenters = GetDistanceOfCenters(circle1, circle2);

            return (distanceOfCenters > radiiSum || distanceOfCenters < Math.Abs(Math.Round(circle2.Radius - circle1.Radius, 4))) ? 0 : (distanceOfCenters == radiiSum ? 1 : 2);
        }

        public static bool FirstInsideSecond(Circle circleToCheck, Circle circle)
        {
            return 
                (EqualsGeometrically(circle, circleToCheck) || 
                (GetNumberOfIntersectionPoints(circleToCheck, circle) <= 1 && GetDistanceOfCenters(circle, circleToCheck) - circle.Radius  < 0 ));
            
        }

        public static List<Circle> GetOutsideCirclesWithRadiusTouching(Circle circle1, Circle circle2, double radius)
        {            
            double radiiSum = circle1.Radius + circle2.Radius;
            double distC1C2 = GetDistanceOfCenters(circle1, circle2);
            
            if (distC1C2 > radiiSum + radius * 2)
            {                
                return new List<Circle>();
            }
            
            double distC1C3 = circle1.Radius + radius;
            double distC2C3 = circle2.Radius + radius;

            double absC1P = Math.Abs((distC1C3 * distC1C3 - distC2C3 * distC2C3 + distC1C2 * distC1C2) / (2 * distC1C2));
            double heigth = Math.Sqrt(distC1C3 * distC1C3 - absC1P * absC1P);

            double coefNormal = absC1P / distC1C2;
            double coefPerp = heigth / distC1C2;

            double center3X1 = coefNormal * (circle2.CenterX - circle1.CenterX) + coefPerp * (circle1.CenterY - circle2.CenterY);
            double center3Y1 = coefNormal * (circle2.CenterY - circle1.CenterY) + coefPerp * (circle2.CenterX - circle1.CenterX);

            double center3X2 = coefNormal * (circle2.CenterX - circle1.CenterX) - coefPerp * (circle1.CenterY - circle2.CenterY);
            double center3Y2 = coefNormal * (circle2.CenterY - circle1.CenterY) - coefPerp * (circle2.CenterX - circle1.CenterX);

            return new List<Circle> { new Circle(circle1.CenterX + center3X1, circle1.CenterY + center3Y1, radius), new Circle(circle1.CenterX + center3X2, circle1.CenterY + center3Y2, radius)};
        }

        public static double GetCircumscribedRadius(Circle circle1, Circle circle2)
        {
            return (GetDistanceOfCenters(circle1, circle2) + circle2.Radius + circle1.Radius) / 2;
        }

        public static Circle GetCircumscribedCircle(Circle circle1, Circle circle2)
        {
            double distC1C2 = GetDistanceOfCenters(circle1, circle2);
            double translationCoef = Math.Round((distC1C2 + circle2.Radius - circle1.Radius) / (2 * distC1C2), 6);
            double circumsribedRadius = GetCircumscribedRadius(circle1, circle2);
            double circumscribedCenterX = circle1.CenterX + translationCoef * (circle2.CenterX - circle1.CenterX);
            double circumscribedCenterY = circle1.CenterY + translationCoef * (circle2.CenterY - circle1.CenterY);

            return new Circle(circumscribedCenterX, circumscribedCenterY, circumsribedRadius + Threshold);
        }
              
        public static Circle FindApollonianCircle(Circle c1, Circle c2, Circle c3)
        {
            // Make sure c2 doesn't have the same X or Y coordinate as the others.            
            if ((Math.Abs(c2.CenterX - c1.CenterX) < Threshold) ||
                (Math.Abs(c2.CenterY - c1.CenterY) < Threshold))
            {
                Circle temp_circle = c2;
                c2 = c3;
                c3 = temp_circle;
            }
            if ((Math.Abs(c2.CenterX - c3.CenterX) < Threshold) ||
                (Math.Abs(c2.CenterY - c3.CenterY) < Threshold))
            {
                Circle temp_circle = c2;
                c2 = c1;
                c1 = temp_circle;
            }
            double x1 = c1.CenterX;
            double y1 = c1.CenterY;
            double r1 = c1.Radius;
            double x2 = c2.CenterX;
            double y2 = c2.CenterY;
            double r2 = c2.Radius;
            double x3 = c3.CenterX;
            double y3 = c3.CenterY;
            double r3 = c3.Radius;

            double v11 = 2 * x2 - 2 * x1;
            double v12 = 2 * y2 - 2 * y1;
            double v13 = x1 * x1 - x2 * x2 + y1 * y1 - y2 * y2 - r1 * r1 + r2 * r2;
            double v14 = 2 * r2 - 2 * r1;

            double v21 = 2 * x3 - 2 * x2;
            double v22 = 2 * y3 - 2 * y2;
            double v23 = x2 * x2 - x3 * x3 + y2 * y2 - y3 * y3 - r2 * r2 + r3 * r3;
            double v24 = 2 * r3 - 2 * r2;

            double w12 = v12 / v11;
            double w13 = v13 / v11;
            double w14 = v14 / v11;

            double w22 = v22 / v21 - w12;
            double w23 = v23 / v21 - w13;
            double w24 = v24 / v21 - w14;

            double P = -w23 / w22;
            double Q = w24 / w22;
            double M = -w12 * P - w13;
            double N = w14 - w12 * Q;

            double a = N * N + Q * Q - 1;
            double b = 2 * M * N - 2 * N * x1 + 2 * P * Q - 2 * Q * y1 + 2 * r1;
            double c = x1 * x1 + M * M - 2 * M * x1 + P * P + y1 * y1 - 2 * P * y1 - r1 * r1;

            // take maximal solution which should be positive
            double? solution = QuadraticSolutions(a, b, c)?.Max();
            if (solution == null || solution < 0) return null;
            double rs = solution.Value;
            double xs = M + N * rs;
            double ys = P + Q * rs;

            return new Circle(xs, ys, rs + Threshold);
        }
        
        private static double[] QuadraticSolutions(double a, double b, double c)
        {
            const double epsilon = 0.000001d;
            double discriminant = b * b - 4 * a * c;

            if (a == 0 && b == 0)
            {
                return null;
            }

            if (a == 0)
            {
                return new double[]
                {
                    (-c) / (2 * b)
                };
            }
            if (discriminant < 0)
            {
                return null;
            }
            
            if (discriminant < epsilon)
            {
                return new double[] { -b / (2 * a) };
            }
            
            return new double[]
            {
                (-b + Math.Sqrt(discriminant)) / (2 * a),
                (-b - Math.Sqrt(discriminant)) / (2 * a),
            };
        }

    }
}
