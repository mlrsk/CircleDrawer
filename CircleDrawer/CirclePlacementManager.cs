using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace CircleDrawer
{
    static class CirclePlacementManager
    {
        public static double circumfindingDuration = 0;
        public static double circumscribedCalculationCount = 0;

        public static List<Circle> GetPlacementGreedily(List<double> radii)
        {
            List<Circle> placedCircles = new List<Circle>();
            
            placedCircles.Add(new Circle(0, 0, radii[0]));

            if (radii.Count > 1)
            {
                placedCircles.Add(new Circle(radii[0] + radii[1], 0, radii[1]));
            }
                      
            Circle circleToPlace = null;

            for (int i = 2; i < radii.Count; i++)
            {
                circleToPlace = null;
                for (int j = 0; j < placedCircles.Count; j++)
                {
                    for (int k = j + 1; k < placedCircles.Count; k++)
                    {
                        var touchingCircles = Circle.GetOutsideCirclesWithRadiusTouching(placedCircles[j], placedCircles[k], radii[i]);

                        if (touchingCircles.Count != 2)
                        {
                            continue;
                        }

                        if (CirclePlacableAmongCircles(touchingCircles[0], placedCircles))
                        {
                            circleToPlace = touchingCircles[0];
                            break;
                        }

                        if (CirclePlacableAmongCircles(touchingCircles[1], placedCircles))
                        {
                            circleToPlace = touchingCircles[1];
                            break;
                        }
                    }

                    if (circleToPlace != null)
                    {
                        placedCircles.Add(circleToPlace);                        
                        break;
                    }
                }
            }

            placedCircles.Add(GetMinimalCircumscribedOfCircles(placedCircles, new Stopwatch()));
            
            return placedCircles;
        }

        public static List<Circle> GetPlacementGreedilyHoldingCircumscribed(List<double> radii)
        {
            List<Circle> placedCircles = new List<Circle>();
            List<Circle> badCandidates = new List<Circle>();
            List<Circle> goodCandidates = new List<Circle>();

            placedCircles.Add(new Circle(0, 0, radii[0]));

            if (radii.Count > 1)
            {
                placedCircles.Add(new Circle(radii[0] + radii[1], 0, radii[1]));
            }

            circumfindingDuration = 0;
            circumscribedCalculationCount = 0;
            Stopwatch sw = new Stopwatch();
            Circle circumscribed = GetMinimalCircumscribedOfCircles(placedCircles, sw);

            for (int i = 2; i < radii.Count; i++)
            {
                goodCandidates.Clear();
                badCandidates.Clear();
                for (int j = 0; j < placedCircles.Count; j++)
                {
                    for (int k = j + 1; k < placedCircles.Count; k++)
                    {
                        var touchingCircles = Circle.GetOutsideCirclesWithRadiusTouching(placedCircles[j], placedCircles[k], radii[i]);

                        if (touchingCircles.Count != 2)
                        {
                            continue;
                        }

                        for (int l = 0; l < 2; l++)
                        {
                            if (Circle.FirstInsideSecond(touchingCircles[l], circumscribed) && CirclePlacableAmongCircles(touchingCircles[l], placedCircles))
                            {
                                goodCandidates.Add(touchingCircles[l]);
                                break;
                            }

                            if (CirclePlacableAmongCircles(touchingCircles[l], placedCircles))
                            {
                                badCandidates.Add(touchingCircles[l]);
                                break;
                            }
                        }
                    }                    
                }
                if (goodCandidates.Any())
                {
                    placedCircles.Add(GetClosestCircle(goodCandidates, circumscribed));                   
                }
                else if (badCandidates.Any())
                {
                    placedCircles.Add(GetClosestCircle(badCandidates, circumscribed));
                    circumscribed = GetMinimalCircumscribedOfCircles(placedCircles, sw);                    
                }
            }
            placedCircles.Add(circumscribed);
            return placedCircles;
        }

        public static double GetOptimalMinimalCircumscribedRadius(List<Circle> circles)
        {            
            double totalAreaOfCircles = circles.Select(c => c.GetArea()).Sum();

            return Math.Sqrt(totalAreaOfCircles / (2 * Math.PI));
        }

        private static Circle GetClosestCircle(List<Circle> candidates, Circle circumscribed)
        {
            int indexOfMinimal = 0;
            double distance = 0;
            double minimalDistance = Double.PositiveInfinity;
            
            for (int i = 0; i< candidates.Count; i++)
            {
                distance = Circle.GetDistanceOfCenters(candidates[i], circumscribed) + candidates[i].Radius;
                if (distance < minimalDistance)
                {
                    minimalDistance = distance;
                    indexOfMinimal = i;
                }
            }

            return candidates[indexOfMinimal];
        }

        private static bool CirclePlacableAmongCircles(Circle circleToPlace, List<Circle> circles)
        {
            foreach (Circle circle in circles)
            {
                if (Circle.GetNumberOfIntersectionPoints(circleToPlace, circle) >= 2 || Circle.FirstInsideSecond(circleToPlace, circle))
                {
                    return false;                    
                }
            }

            return true;
        }

        private static Circle GetMinimalCircumscribedOfCircles(List<Circle> circles, Stopwatch stopwatch)
        {
            stopwatch.Start();
            Circle circumscribed = GetMinimalCircumscribedOfCircles(circles);
            stopwatch.Stop();
            
            circumfindingDuration = stopwatch.ElapsedMilliseconds;
            circumscribedCalculationCount++;
            return circumscribed;
        }

        private static Circle GetMinimalCircumscribedOfCircles(List<Circle> circles)
        {
            double minimalCircumscribedRadius = double.PositiveInfinity;
            Circle currentCircumscribed = null; 
            Circle minimalCircumscribed = null;
            List<Circle> usedCircles = new List<Circle>();
            for (int i = 0; i < circles.Count; i++)
            {
                for (int j = i + 1; j < circles.Count; j++)
                {
                   currentCircumscribed = Circle.GetCircumscribedCircle(circles[i], circles[j]);
                    
                    if (currentCircumscribed.Radius < minimalCircumscribedRadius && circles.All(c => Circle.FirstInsideSecond(c, currentCircumscribed)))
                    {
                        minimalCircumscribedRadius = currentCircumscribed.Radius;
                        minimalCircumscribed = currentCircumscribed;
                        usedCircles.Clear();
                        usedCircles.AddRange(new Circle[]{ circles[i], circles[j] });
                    }

                    for (int k = j + 1; k < circles.Count; k++)
                    {
                        currentCircumscribed = Circle.FindApollonianCircle(circles[i], circles[j], circles[k]);
                        
                        if (currentCircumscribed != null && currentCircumscribed.Radius < minimalCircumscribedRadius && circles.All(c => Circle.FirstInsideSecond(c, currentCircumscribed)))
                        {
                            minimalCircumscribedRadius = currentCircumscribed.Radius;
                            minimalCircumscribed = currentCircumscribed;
                            usedCircles.Clear();
                            usedCircles.AddRange(new Circle[] { circles[i], circles[j], circles[k] });
                        }

                    }
                }
            }

            circles.ForEach(c => c.CircleShape.Stroke = Brushes.Black);
            usedCircles.ForEach(c => c.CircleShape.Stroke = Brushes.Red);
            minimalCircumscribed.CircleShape.Stroke = Brushes.Blue;

            return minimalCircumscribed;
        }        
        
    }
}
