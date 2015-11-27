using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ball_of_Duty_Server.Domain.Entities;

namespace Ball_of_Duty_Server.Domain.Maps
{
    public class MapGenerator
    {
        private static Random _rand = new Random(); // TODO needs to be able to pick seed
        private const int WALLS_PER_MEGA_PIXEL = 70;
        private const int MEGA_PIXEL = 1000 * 1000;
        private const int WALL_SIZE = 50;

        public static void GenerateMap(Map map)
        {
            int mapWidth = map.Width;
            int mapHeight = map.Height;
            int wallAmount = WALLS_PER_MEGA_PIXEL * mapWidth * mapHeight / MEGA_PIXEL;
            int mapGridX = mapWidth / WALL_SIZE;
            int mapGridY = mapHeight / WALL_SIZE;

            Point position = new Point(0, 0);

            for (int i = 0; i < mapGridX; i++)
            {
                Wall wall = new Wall(position, WALL_SIZE);
                map.GameObjects.TryAdd(wall.Id, wall);
                position.X += WALL_SIZE;
            }

            position = new Point(0, mapHeight - WALL_SIZE);

            for (int i = 0; i < mapGridX; i++)
            {
                Wall wall = new Wall(position, WALL_SIZE);
                map.GameObjects.TryAdd(wall.Id, wall);
                position.X += WALL_SIZE;
            }

            position = new Point(0, WALL_SIZE);

            for (int i = 0; i < mapGridY - 2; i++)
            {
                Wall wall = new Wall(position, WALL_SIZE);
                map.GameObjects.TryAdd(wall.Id, wall);
                position.Y += WALL_SIZE;
            }

            position = new Point(mapWidth - WALL_SIZE, WALL_SIZE);

            for (int i = 0; i < mapGridY - 2; i++)
            {
                Wall wall = new Wall(position, WALL_SIZE);
                map.GameObjects.TryAdd(wall.Id, wall);
                position.Y += WALL_SIZE;
            }

            int wallCount = 0;

            while (true)
            {
                Wall tempWall;
                while (true)
                {
                    position = new Point(_rand.Next(1, mapGridX) * WALL_SIZE, _rand.Next(1, mapGridY) * WALL_SIZE);
                    tempWall = new Wall(position, WALL_SIZE);
                    if (CheckValidWall(tempWall, map.GameObjects.Values))
                    {
                        map.GameObjects.TryAdd(tempWall.Id, tempWall);
                        ++wallCount;
                        break;
                    }
                }
                while (true)
                {
                    Point newPosition = new Point(position.X, position.Y);
                    int r = _rand.Next(100);
                    //TODO Need to make it more likely to make a 5 wall piece than a 1 wall piece. And need to make it more likely to continue on the wall path that was taken

                    if (r < 20)
                    {
                        newPosition.Y -= WALL_SIZE;
                    }
                    else if (r < 40)
                    {
                        newPosition.X += WALL_SIZE;
                    }
                    else if (r < 60)
                    {
                        newPosition.Y += WALL_SIZE;
                    }
                    else if (r < 80)
                    {
                        newPosition.X -= WALL_SIZE;
                    }
                    else
                    {
                        break;
                    }

                    tempWall = new Wall(newPosition, WALL_SIZE);
                    if (CheckValidWall(tempWall, map.GameObjects.Values))
                    {
                        map.GameObjects.TryAdd(tempWall.Id, tempWall);
                        position = newPosition;
                        ++wallCount;
                    }

                    if (wallCount >= wallAmount)
                    {
                        break;
                    }
                }
                if (wallCount >= wallAmount)
                {
                    break;
                }
            }
        }

        private static bool CheckValidWall(Wall newWall, ICollection<GameObject> walls)
        {
            return walls.All(wall => !newWall.Body.Position.Equals(wall.Body.Position));
        }
    }
}