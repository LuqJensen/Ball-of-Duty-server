using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.DTO;
using Timer = System.Timers.Timer;

namespace Ball_of_Duty_Server.Domain
{
    public class Map
    {
        private HashSet<int> _gameObjectsActive; //TODO: possible race conditions.
        private Thread _updateThread;

        public int Width { get; set; }

        public int Height { get; set; }

        public List<Wall> Walls { get; set; }

        public ConcurrentDictionary<int, GameObject> GameObjects { get; set; }

        public Broker Broker { get; set; }

        public Game Game { get; set; }

        public Map()
        {
            Width = 1100;
            Height = 650; // default
            Walls = new List<Wall>();
            MapGenerator.GenerateMap(this);
            Broker = new Broker(this);
            GameObjects = new ConcurrentDictionary<int, GameObject>();
            _updateThread = new Thread(Activate);
            _updateThread.Start();
        }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Walls = new List<Wall>();
            MapGenerator.GenerateMap(this);
            Broker = new Broker(this);
            GameObjects = new ConcurrentDictionary<int, GameObject>();
            _updateThread = new Thread(Activate);
            _updateThread.Start();
        }

        public void Activate()
        {
            Timer timeoutCheck = new Timer();
            timeoutCheck.Elapsed += CheckTimeouts;
            timeoutCheck.Interval = 10000;
            timeoutCheck.Enabled = true;
            _gameObjectsActive = new HashSet<int>();
            while (true)
            {
                Update();
                Thread.Sleep(20);
            }
        }

        public void Deactivate()
        {
            _updateThread.Interrupt();
        }

        public void Update()
        {
            Broker.SendPositionUpdate(GetPositions(), 1 /*Game.Id*/);
        }

        public int AddCharacter()
        {
            Character c = new Character();
            GameObjects.TryAdd(c.Id, c);
            return c.Id;
        }

        public void RemoveCharacter(int characterId)
        {
            GameObject character;
            GameObjects.TryRemove(characterId, out character);
        }

        public GameObjectDTO[] ExportGameObjects()
        {
            List<GameObjectDTO> gameObjects = new List<GameObjectDTO>();

            foreach (GameObject go in GameObjects.Values)
            {
                BodyDTO body = new BodyDTO
                {
                    Position = new PointDTO { X = go.Body.Position.X, Y = go.Body.Position.Y },
                    Type = (int)go.Body.Type
                };

                gameObjects.Add(new GameObjectDTO { Id = go.Id, Body = body });
            }
            foreach (Wall go in Walls) //TODO: remove this along with the Walls property.
            {
                BodyDTO body = new BodyDTO
                {
                    Position = new PointDTO { X = go.Body.Position.X, Y = go.Body.Position.Y },
                    Type = (int)go.Body.Type,
                    Height = go.Body.Height,
                    Width = go.Body.Width
                };

                gameObjects.Add(new GameObjectDTO { Id = go.Id, Body = body });
            }

            return gameObjects.ToArray();
        }

        private List<ObjectPosition> GetPositions()
        {
            var positions = new List<ObjectPosition>();
            foreach (var go in GameObjects.Values)
            {
                positions.Add(new ObjectPosition(go.Id, go.Body.Position));
            }
            return positions;
        }


        private void CheckTimeouts(object sender, ElapsedEventArgs e)
            // An idea to handle it, not sure i like it too much.
        {
            if (_gameObjectsActive.Count == 0)
                return;

            Console.WriteLine("Checking");
            var removeTimeoutObjects = new List<int>();

            foreach (var go in GameObjects.Values)
            {
                if (!_gameObjectsActive.Contains(go.Id))
                {
                    removeTimeoutObjects.Add(go.Id);
                    Console.WriteLine(go.Id + " Hasnt send messages for atleast 10 seconds");
                }
            }

            foreach (var rto in removeTimeoutObjects)
            {
                Console.WriteLine("removing " + rto);
                GameObject go;
                GameObjects.TryRemove(rto, out go);
            }

            _gameObjectsActive.Clear();
        }

        public void UpdatePosition(Point position, int goId)
        {
            GameObject go;
            if (GameObjects.TryGetValue(goId, out go))
            {
                _gameObjectsActive.Add(go.Id);
                go.Body.Position = position;
            }
        }
    }
}