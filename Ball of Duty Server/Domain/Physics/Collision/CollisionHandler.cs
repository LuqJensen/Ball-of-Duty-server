using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Domain.Physics.Collision
{
    public class CollisionHandler
    {
        public static bool IsColliding(GameObject o1, GameObject o2)
        {
            bool retval = false;
            if (o1.Body.Type == Body.Geometry.CIRCLE && o2.Body.Type == Body.Geometry.CIRCLE)
            {
                retval = CollisionCircleCircle(o1, o2);
            }
            else if (o1.Body.Type == Body.Geometry.CIRCLE && o2.Body.Type == Body.Geometry.RECTANGLE)
            {
                retval = CollisionCircleRectangle(o1, o2);
            }
            else if (o1.Body.Type == Body.Geometry.RECTANGLE &&
                     o2.Body.Type == Body.Geometry.CIRCLE)
            {
                retval = CollisionCircleRectangle(o2, o1);
            }
            else if (o1.Body.Type == Body.Geometry.RECTANGLE &&
                     o2.Body.Type == Body.Geometry.RECTANGLE)
            {
                retval = CollisionRectangleRectangle(o1, o2);
            }

            return retval;
        }

        private static bool CollisionCircleCircle(GameObject o1, GameObject o2)
        {
            double c1x = o1.Body.Center.X;
            double c2x = o2.Body.Center.X;
            double c1y = o1.Body.Center.Y;
            double c2y = o2.Body.Center.Y;

            double dx = c1x - c2x;
            double dy = c1y - c2y;
            double c1r = o1.Body.Height / 2;
            double c2r = o2.Body.Height / 2;
            return Math.Sqrt((dx * dx) + (dy * dy)) <= (c1r + c2r);
        }

        public static bool CollisionCircleRectangle(GameObject circle, GameObject rect)
        {
            double circleDistanceX = Math.Abs(rect.Body.Center.X - circle.Body.Center.X);
            double circleDistanceY = Math.Abs(rect.Body.Center.Y - circle.Body.Center.Y);

            if (circleDistanceY >= (rect.Body.Height / 2 + circle.Body.Height / 2))
            {
                return false;
            }
            if (circleDistanceX >= (rect.Body.Width / 2 + circle.Body.Width / 2))
            {
                return false;
            }
            if (circleDistanceY < (rect.Body.Height / 2))
            {
                return true;
            }
            if (circleDistanceX < (rect.Body.Width / 2))
            {
                return true;
            }
            double cornerDistanceSq = Math.Sqrt(
                Math.Pow((circleDistanceX - (rect.Body.Width / 2)), 2) +
                Math.Pow((circleDistanceY - (rect.Body.Height / 2)), 2));

            return (cornerDistanceSq < circle.Body.Height / 2);
        }

        public static bool CollisionRectangleRectangle(GameObject rect1, GameObject rect2)
        {
            double r1X = rect1.Body.Position.X;
            double r1Y = rect1.Body.Position.Y;
            double r1H = rect1.Body.Width;
            double r1L = rect1.Body.Height;
            double r2X = rect2.Body.Position.X;
            double r2Y = rect2.Body.Position.Y;
            double r2H = rect2.Body.Width;
            double r2L = rect2.Body.Height;

            bool xOverlap = r1X.IsInRange(r2X, r2X + r2L) || r2X.IsInRange(r1X, r1X + r1L);
            bool yOverlap = r1Y.IsInRange(r2Y, r2Y + r2H) || r2Y.IsInRange(r1Y, r1Y + r1H);
            return xOverlap && yOverlap;
        }
    }
}