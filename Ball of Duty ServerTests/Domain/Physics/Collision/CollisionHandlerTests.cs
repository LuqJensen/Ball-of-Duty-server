using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ball_of_Duty_ServerTests.Domain.Physics.Collision
{
    [TestClass]
    public class CollisionHandlerTests
    {
        [TestMethod]
        public void IsCollidingSelfTest()
        {
            // An object collides with itself.
            Body body1 = new Body(null, new Point(0, 0), 40, 40);
            Assert.IsTrue(CollisionHandler.IsColliding(body1, body1));
        }

        [TestMethod]
        public void IsCollidingTest2()
        {
            // The bodies collide in (20, 0).
            Body body1 = new Body(null, new Point(0, 0), 40, 40);
            Body body2 = new Body(null, new Point(40, 0), 40, 40);
            Assert.IsTrue(CollisionHandler.IsColliding(body1, body2));
        }

        [TestMethod]
        public void IsCollidingTest3()
        {
            // The bodies are next to each other.
            Body body1 = new Body(null, new Point(0, 0), 40, 40);
            Body body2 = new Body(null, new Point(41, 0), 40, 40);
            Assert.IsFalse(CollisionHandler.IsColliding(body1, body2));
        }

        [TestMethod]
        public void IsCollidingTest4()
        {
            Body body1 = new Body(null, new Point(50, 40), 20, 20);
            Body body2 = new Body(null, new Point(50, 61), 40, 40);
            Assert.IsFalse(CollisionHandler.IsColliding(body1, body2));
        }

        [TestMethod]
        public void IsCollidingCircleVsRectangleTest() // We should have a set of separate tests for each set of Body.Geometry type.
        {
            // Checking if it differentiates properly between circle and rectangles (its circle by default)
            Body body1 = new Body(null, new Point(50, 40), 20, 20);
            Body body2 = new Body(null, new Point(50, 59), 40, 40);
            Assert.IsFalse(CollisionHandler.IsColliding(body1, body2));
            body1.Type = Body.Geometry.RECTANGLE;
            body2.Type = Body.Geometry.RECTANGLE;
            Assert.IsTrue(CollisionHandler.IsColliding(body1, body2));
        }

        [TestMethod]
        public void CollisionLineSquareTest()
        {
            Body body = new Body(null, new Point(0, 0), 40, 40)
            {
                Type = Body.Geometry.RECTANGLE
            };
            // Checking if a normal line, going straight across the rectangle, collides.
            Assert.IsTrue(CollisionHandler.CollisionLineSquare(-5, 50, -5, 50, body));
        }

        [TestMethod]
        public void CollisionLineSquareTest2()
        {
            const int BODY_X = 40;
            const int BODY_Y = 50;
            const int BODY_WIDTH = 40;
            const int BODY_HEIGHT = 20;
            Body body = new Body(null, new Point(BODY_X, BODY_Y), BODY_WIDTH, BODY_HEIGHT)
            {
                Type = Body.Geometry.RECTANGLE
            };
            // Checking if it collides with lines just around the square.
            Assert.IsFalse(CollisionHandler.CollisionLineSquare(81, 81, 50, 70, body));

            Assert.IsFalse(CollisionHandler.CollisionLineSquare(40, 80, 71, 71, body));

            Assert.IsFalse(CollisionHandler.CollisionLineSquare(39, 79, 49, 49, body));

            Assert.IsFalse(CollisionHandler.CollisionLineSquare(39, 39, 50, 70, body));
        }

        [TestMethod]
        public void CollisionLineSquareTest3()
        {
            const int BODY_X = 40;
            const int BODY_Y = 50;
            const int BODY_WIDTH = 40;
            const int BODY_HEIGHT = 20;
            Body body = new Body(null, new Point(BODY_X, BODY_Y), BODY_WIDTH, BODY_HEIGHT)
            {
                Type = Body.Geometry.RECTANGLE
            };
            // Checking if it collides with lines just inside the square.
            Assert.IsTrue(CollisionHandler.CollisionLineSquare(81, 80, 50, 70, body));

            Assert.IsTrue(CollisionHandler.CollisionLineSquare(40, 80, 71, 70, body));

            Assert.IsTrue(CollisionHandler.CollisionLineSquare(39, 79, 50, 50, body));

            Assert.IsTrue(CollisionHandler.CollisionLineSquare(39, 40, 30, 70, body));
        }
    }
}