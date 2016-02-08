using System.Collections.Generic;

namespace Entity.Components.Physics.Collision
{
    public interface ICollisionHandler
    {
        bool IsColliding(IBody b1, IBody b2);

        bool CollisionLineSquare(double x1, double x2, double y1, double y2, IBody b2);

        int GetFirstObjectIntersectingPath<T>(ICollection<IGameObject> gameobjects, IGameObject gameobjectWithPath, int mapSize);

        bool CollisionCircleRectangle(IBody circle, IBody rect);

        bool CollisionRectangleRectangle(IBody rect1, IBody rect2);
    }
}