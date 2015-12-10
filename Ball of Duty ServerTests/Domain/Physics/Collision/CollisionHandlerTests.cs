using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ball_of_Duty_Server.Domain.Physics.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ball_of_Duty_Server.Domain.Physics.Collision.Tests
{
    [TestClass()]
    public class CollisionHandlerTests
    {
        [TestMethod()]
        public void IsCollidingTest()
        {
            Body body1 = new Body(null, new Point(0, 0), 40, 40);
            Assert.IsTrue(CollisionHandler.IsColliding(body1, body1));
            body1 = new Body(null, new Point(0, 0), 40, 40);
            Body body2 = new Body(null, new Point(40, 0), 40, 40);
            Assert.IsTrue(CollisionHandler.IsColliding(body1, body2));
            body1 = new Body(null, new Point(0, 0), 40, 40);
            body2 = new Body(null, new Point(41, 0), 40, 40);
            Assert.IsFalse(CollisionHandler.IsColliding(body1, body2));
            body1 = new Body(null, new Point(50, 40), 20, 20);
            body2 = new Body(null, new Point(50, 61), 40, 40);
            Assert.IsFalse(CollisionHandler.IsColliding(body1, body2));
            // Checking if it differentates properly between circle and rectangles (its circle by default)
            body1 = new Body(null, new Point(50, 40), 20, 20);
            body2 = new Body(null, new Point(50, 59), 40, 40);
            Assert.IsFalse(CollisionHandler.IsColliding(body1, body2));
            body1.Type = Body.Geometry.RECTANGLE;
            body2.Type = Body.Geometry.RECTANGLE;
            Assert.IsTrue(CollisionHandler.IsColliding(body1, body2));
        }

        [TestMethod()]
        public void CollisionLineSquareTest()
        {
            Body body = new Body(null, new Point(0, 0), 40, 40)
            {
                Type = Body.Geometry.RECTANGLE
            };
            Assert.IsTrue(CollisionHandler.CollisionLineSquare(-5, 50, -5, 50, body));
            // Checking if a normal line going straight across the rectangle collides.

            body = new Body(null, new Point(40, 50), 40, 20)
            {
                Type = Body.Geometry.RECTANGLE
            };
            // Checking if it collides with lines just around the square, and just inside the square.
            Assert.IsFalse(CollisionHandler.CollisionLineSquare(81, 81, 50, 70, body));
            Assert.IsTrue(CollisionHandler.CollisionLineSquare(81, 80, 50, 70, body));

            Assert.IsFalse(CollisionHandler.CollisionLineSquare(40, 80, 71, 71, body));
            Assert.IsTrue(CollisionHandler.CollisionLineSquare(40, 80, 71, 70, body));

            Assert.IsFalse(CollisionHandler.CollisionLineSquare(39, 79, 49, 49, body));
            Assert.IsTrue(CollisionHandler.CollisionLineSquare(39, 79, 50, 50, body));

            Assert.IsFalse(CollisionHandler.CollisionLineSquare(39, 39, 50, 70, body));
            Assert.IsTrue(CollisionHandler.CollisionLineSquare(39, 40, 30, 70, body));
        }
    }
}