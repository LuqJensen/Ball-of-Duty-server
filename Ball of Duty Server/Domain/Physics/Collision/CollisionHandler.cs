using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Domain.Physics.Collision
{
    public class CollisionHandler
    {
        public static bool IsColliding(Body b1, Body b2)
        {
            bool retval = false;
            if (b1.Type == Body.Geometry.CIRCLE && b2.Type == Body.Geometry.CIRCLE)
            {
                retval = CollisionCircleCircle(b1, b2);
            }
            else if (b1.Type == Body.Geometry.CIRCLE && b2.Type == Body.Geometry.RECTANGLE)
            {
                retval = CollisionCircleRectangle(b1, b2);
            }
            else if (b1.Type == Body.Geometry.RECTANGLE && b2.Type == Body.Geometry.CIRCLE)
            {
                retval = CollisionCircleRectangle(b2, b1);
            }
            else if (b1.Type == Body.Geometry.RECTANGLE && b2.Type == Body.Geometry.RECTANGLE)
            {
                retval = CollisionRectangleRectangle(b1, b2);
            }

            return retval;
        }

        public static bool CollisionLineSquare(double x1, double x2, double y1, double y2, Body b2)
        {
            Point position = b2.Position;
            double xR = position.X + b2.Width;
            double xL = position.X;
            double yB = position.Y + b2.Height;
            double yT = position.Y;

            double minX = Math.Min(x1, x2);
            double maxX = Math.Max(x1, x2);

            // Find the intersection of the segment's and rectangle's x-projections
            maxX = Math.Min(maxX, xR);
            minX = Math.Max(minX, xL);

            if (minX > maxX) // If their projections do not intersect return false
            {
                return false;
            }

            // Find corresponding min and max Y for min and max X we found before
            double minY = y1;
            double maxY = y2;

            double dx = x2 - x1;

            if (Math.Abs(dx) > 0.0000001)
            {
                double a = (y2 - y1) / dx;
                double b = y1 - a * x1;
                minY = a * minX + b;
                maxY = a * maxX + b;
            }

            if (minY > maxY)
            {
                double tmp = maxY;
                maxY = minY;
                minY = tmp;
            }

            // Find the intersection of the segment's and rectangle's y-projections
            maxY = Math.Min(maxY, yB);
            minY = Math.Max(minY, yT);

            return !(minY > maxY);
        }

        private static bool CollisionCircleCircle(Body b1, Body b2)
        {
            Point center1 = b1.Center;
            double c1X = center1.X;
            double c1Y = center1.Y;

            Point center2 = b2.Center;
            double c2X = center2.X;
            double c2Y = center2.Y;

            double dx = c1X - c2X;
            double dy = c1Y - c2Y;
            double c1R = b1.Height / 2;
            double c2R = b2.Height / 2;
            return Math.Sqrt((dx * dx) + (dy * dy)) <= (c1R + c2R);
        }


        public static int GetFirstObjectIntersectingPath<T>(ICollection<GameObject> gameobjects, GameObject gameobjectWithPath)
        {
            double closestDistance = -1;
            int wallId = 0;
            double cx = gameobjectWithPath.Body.Center.X;
            double cy = gameobjectWithPath.Body.Center.Y;
            double diameter = gameobjectWithPath.Body.Height;
            double radius = diameter / 2;

            // Rotating point1 90 deg clockwise around point2 = point1.newX = point1.deltaY+point2.X, point.newY = -point1.deltaX+point2.Y
            // Rotating point1 90 deg counterclockwise around point2 = point1.newX = -point1.deltaY+point2.X, point.newY = point1.deltaX +point2.Y
            // Delta in our case is always = radius.
            Vector radiusVec = gameobjectWithPath.Physics.Velocity;
            radiusVec.Normalize();

            radiusVec = Vector.Multiply(radiusVec, radius);

            double deltaX = gameobjectWithPath.Physics.Velocity.X * Game.MAP_SIZE;
            double deltaY = gameobjectWithPath.Physics.Velocity.Y * Game.MAP_SIZE;

            double rotXClockwise = radiusVec.Y + cx; // Startpoint of a line(path) starting  at one of the gameobjects edges
            double rotYClockwise = -radiusVec.X + cy;
            double x2Clockwise = rotXClockwise + deltaX; // End point of a line(path) starting at one of the gameobjects edges
            double y2Clockwise = rotYClockwise + deltaY;

            double rotXCounter = -radiusVec.Y + cx; // Startpoint of a line(path) starting  at one of the gameobjects edges
            double rotYCounter = radiusVec.X + cy;
            double x2Counter = rotXCounter + deltaX; // End point of a line(path) starting at one of the gameobjects edges
            double y2Counter = rotYCounter + deltaY;

            foreach (GameObject go in gameobjects)
            {
                if (!(go is T))
                {
                    continue;
                }

                Point center = go.Body.Center;
                if (CollisionLineSquare(rotXClockwise, x2Clockwise, rotYClockwise, y2Clockwise, go.Body) ||
                    CollisionLineSquare(rotXCounter, x2Counter, rotYCounter, y2Counter, go.Body))
                {
                    double dx = Math.Abs(cx - (center.X));
                    double dy = Math.Abs(cy - (center.Y));
                    double distance = Math.Sqrt((dx * dx) + (dy * dy));
                    if (distance < closestDistance || closestDistance < 0)
                    {
                        wallId = go.Id;
                        closestDistance = distance;
                    }
                }
            }
            return wallId;
        }

        public static bool CollisionCircleRectangle(Body circle, Body rect)
        {
            Point centerCircle = circle.Center;
            double circleRadius = circle.Height / 2;

            Point rectangleCenter = rect.Center;
            double halfRectangleWidth = rect.Width / 2;
            double halfRectangleHeight = rect.Height / 2;

            double circleDistanceX = Math.Abs(rectangleCenter.X - centerCircle.X);
            double circleDistanceY = Math.Abs(rectangleCenter.Y - centerCircle.Y);

            if (circleDistanceY >= (halfRectangleHeight + circleRadius))
            {
                return false;
            }
            if (circleDistanceX >= (halfRectangleWidth + circleRadius))
            {
                return false;
            }
            if (circleDistanceY < halfRectangleHeight)
            {
                return true;
            }
            if (circleDistanceX < halfRectangleWidth)
            {
                return true;
            }

            double cornerDistanceSq = Math.Sqrt(
                Math.Pow((circleDistanceX - halfRectangleWidth), 2) +
                Math.Pow((circleDistanceY - halfRectangleHeight), 2));

            return (cornerDistanceSq < circleRadius);
        }

        public static bool CollisionRectangleRectangle(Body rect1, Body rect2)
        {
            Point r1 = rect1.Position;
            double r1X = r1.X;
            double r1Y = r1.Y;
            double r1W = rect1.Width;
            double r1H = rect1.Height;

            Point r2 = rect2.Position;
            double r2X = r2.X;
            double r2Y = r2.Y;
            double r2W = rect2.Width;
            double r2H = rect2.Height;

            bool xOverlap = r1X.IsInRange(r2X, r2X + r2W) || r2X.IsInRange(r1X, r1X + r1W);
            bool yOverlap = r1Y.IsInRange(r2Y, r2Y + r2H) || r2Y.IsInRange(r1Y, r1Y + r1H);
            return xOverlap && yOverlap;
        }
    }
}