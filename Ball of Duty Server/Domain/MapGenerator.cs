using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace Ball_of_Duty_Server.Domain
{
    public class MapGenerator
    {
        public static Random Ran = new Random(); // TODO needs to be able to pick seed
        public static int WallAmount = 50;
        public static int WallSize = 50;
        public static void GenerateMap(Map map)
        {
            int mapWidth = map.Width;
            int mapHeight = map.Height;
            int wallSizeReal = WallSize;
            int mapGridX = mapWidth / wallSizeReal;
            int mapGridY = mapHeight / wallSizeReal;
            Point position;
            position = new Point(0, 0);
            for (int i = 0; i < mapGridX; i++)
            {
                map.Walls.Add(new Wall(position, WallSize));
                position.X += wallSizeReal;
            }
            position = new Point(0, mapHeight - wallSizeReal);
            for (int i = 0; i < mapGridX; i++)
            {
                map.Walls.Add(new Wall(position, WallSize));
                position.X += wallSizeReal;
            }
            position = new Point(0, wallSizeReal);
            for (int i = 0; i < mapGridY - 2; i++)
            {
                map.Walls.Add(new Wall(position, WallSize));
                position.Y += wallSizeReal;
            }
            position = new Point(mapWidth - wallSizeReal, wallSizeReal);
            for (int i = 0; i < mapGridY - 2; i++)
            {
                map.Walls.Add(new Wall(position, WallSize));
                position.Y += wallSizeReal;
            }
            int wallCount = 0;
            while (true)
            {
                Wall tempWall;
                while (true)
                {
                    position = new Point(Ran.Next(1, mapGridX) * wallSizeReal, Ran.Next(1, mapGridY) * wallSizeReal);
                    tempWall = new Wall(position, WallSize);
                    if (checkValidWall(tempWall, map.Walls))
                    {
                        map.Walls.Add(tempWall);
                        wallCount++;
                        break;
                    }
                }
                while (true)
                {
                    Point newPosition = new Point(position.X, position.Y);
                    int r = Ran.Next(100); // Need to make it more likely to make a 5 wall piece than a 1 wall piece. And need to make it more likely to continue on the wall path that was taken
                    if (r < 20)
                    {
                        newPosition.Y -= wallSizeReal;
                        tempWall = new Wall(newPosition, WallSize);
                        if (checkValidWall(tempWall, map.Walls))
                        {
                            map.Walls.Add(tempWall);
                            position = newPosition;
                            wallCount++;
                        }
                    }
                    else if (r < 40)
                    {
                        newPosition.X += wallSizeReal;
                        tempWall = new Wall(newPosition, WallSize);
                        if (checkValidWall(tempWall, map.Walls))
                        {
                            map.Walls.Add(tempWall);
                            position = newPosition;
                            wallCount++;
                        }
                    }
                    else if (r < 60)
                    {
                        newPosition.Y += wallSizeReal;
                        tempWall = new Wall(newPosition, WallSize);
                        if (checkValidWall(tempWall, map.Walls))
                        {
                            map.Walls.Add(tempWall);
                            position = newPosition;
                            wallCount++;
                        }
                    }
                    else if (r < 80)
                    {
                        newPosition.X -= wallSizeReal;
                        tempWall = new Wall(newPosition, WallSize);
                        if (checkValidWall(tempWall, map.Walls))
                        {
                            map.Walls.Add(tempWall);
                            position = newPosition;
                            wallCount++;
                        }
                    }
                    else if (r <= 100)
                    {
                        break;
                    }
                    if (wallCount >= WallAmount)
                    {
                        break;
                    }
                }
                Console.WriteLine(wallCount + " " + WallAmount);
                if (wallCount >= WallAmount)
                {
                    break;
                }
            }
        }
        private static bool checkValidWall(Wall newWall, List<Wall> walls) //TODO
        {
            foreach (var wall in walls)
            {
                if (newWall.ObjectBody.Position.Equals(wall.ObjectBody.Position))
                {
                    return false;
                }
            }
            return true;
        }
    }
}