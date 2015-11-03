using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ball_of_Duty_Server.Domain.Entities;

namespace Ball_of_Duty_Server.Domain
{
    public class MapGenerator
    {
        private static Random _rand = new Random(); // TODO needs to be able to pick seed
        private static int _wallAmount = 50;
        private static int _wallSize = 50;

        public static void GenerateMap(Map map)
        {
            int mapWidth = map.Width;
            int mapHeight = map.Height;
            int wallSizeReal = _wallSize; // TODO get rid of either variable, this is pointless.
            int mapGridX = mapWidth / wallSizeReal;
            int mapGridY = mapHeight / wallSizeReal;

            Point position = new Point(0, 0);

            for (int i = 0; i < mapGridX; i++)
            {
                map.Walls.Add(new Wall(position, _wallSize));
                position.X += wallSizeReal;
            }

            position = new Point(0, mapHeight - wallSizeReal);

            for (int i = 0; i < mapGridX; i++)
            {
                map.Walls.Add(new Wall(position, _wallSize));
                position.X += wallSizeReal;
            }

            position = new Point(0, wallSizeReal);

            for (int i = 0; i < mapGridY - 2; i++)
            {
                map.Walls.Add(new Wall(position, _wallSize));
                position.Y += wallSizeReal;
            }

            position = new Point(mapWidth - wallSizeReal, wallSizeReal);

            for (int i = 0; i < mapGridY - 2; i++)
            {
                map.Walls.Add(new Wall(position, _wallSize));
                position.Y += wallSizeReal;
            }

            int wallCount = 0;

            while (true)
            {
                Wall tempWall;
                while (true)
                {
                    Point p = new Point(_rand.Next(1, mapGridX) * wallSizeReal, _rand.Next(1, mapGridY) * wallSizeReal);
                    tempWall = new Wall(p, _wallSize);
                    if (checkValidWall(tempWall, map.Walls))
                    {
                        map.Walls.Add(tempWall);
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
                        newPosition.Y -= wallSizeReal;
                    }
                    else if (r < 40)
                    {
                        newPosition.X += wallSizeReal;
                    }
                    else if (r < 60)
                    {
                        newPosition.Y += wallSizeReal;
                    }
                    else if (r < 80)
                    {
                        newPosition.X -= wallSizeReal;
                    }
                    else
                    {
                        break;
                    }

                    tempWall = new Wall(newPosition, _wallSize);
                    if (checkValidWall(tempWall, map.Walls))
                    {
                        map.Walls.Add(tempWall);
                        position = newPosition;
                        ++wallCount;
                    }

                    if (wallCount >= _wallAmount)
                    {
                        break;
                    }
                }
                Console.WriteLine($"WallAmount: {wallCount}");
                if (wallCount >= _wallAmount)
                {
                    break;
                }
            }
        }

        private static bool checkValidWall(Wall newWall, List<Wall> walls) //TODO
        {
            foreach (var wall in walls)
            {
                if (newWall.Body.Position.Equals(wall.Body.Position))
                {
                    return false;
                }
            }
            return true;
        }
    }
}