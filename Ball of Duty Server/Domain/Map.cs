using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using Ball_of_Duty_Server.Domain.Communication;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.DTO;
using Timer = System.Timers.Timer;

namespace Ball_of_Duty_Server.Domain
{
    public class Map : IObserver
    {
        private ConcurrentDictionary<int, bool> _gameObjectsActive;
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

            Timer decayCheck = new Timer();
            decayCheck.Elapsed += DecayScores;
            decayCheck.Interval = 5000;
            decayCheck.Enabled = true;

            _gameObjectsActive = new ConcurrentDictionary<int, bool>();
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
            foreach (GameObject go in GameObjects.Values)
            {
                if (go is Bullet)
                {
                    go.UpdateWithCollision(GameObjects.Values);
                }
            }
            Broker.SendPositionUpdate(GetPositions());
        }

        public int AddBullet(double x, double y, double velocityX, double velocityY, double radius, int damage,
            int ownerId)
        {
            Bullet bullet = new Bullet(new Point(x, y), new Vector(velocityX, velocityY), radius, damage, ownerId);
            GameObjects.TryAdd(bullet.Id, bullet);
            bullet.Register(this);
            return bullet.Id;
        }


        public Character AddCharacter()
        {
            Character c = new Character();
            GameObjects.TryAdd(c.Id, c);
            c.Register(this);
            return c;
        }

        public void RemoveCharacter(int characterId)
        {
            GameObject character;
            if (GameObjects.TryRemove(characterId, out character))
            {
                character.UnRegister(this);
            }
        }

        public GameObjectDTO[] ExportGameObjects()
        {
            List<GameObjectDTO> gameObjects = new List<GameObjectDTO>();

            foreach (GameObject go in GameObjects.Values)
            {
                BodyDTO body = new BodyDTO
                {
                    Position = new PointDTO { X = go.Body.Position.X, Y = go.Body.Position.Y },
                    Width = go.Body.Width,
                    Height = go.Body.Height,
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
                if (go is Bullet)
                {
                    continue;
                }
                positions.Add(new ObjectPosition(go.Id, go.Body.Position));
            }
            return positions;
        }

        private void CheckTimeouts(object sender, ElapsedEventArgs e)
            // An idea to handle it, not sure i like it too much.
        {
            if (_gameObjectsActive.Count == 0)
                return;

            var removeTimeoutObjects = new List<int>();

            foreach (var go in GameObjects.Values)
            {
                if (!_gameObjectsActive.Keys.Contains(go.Id))
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
                _gameObjectsActive.TryAdd(go.Id, true);
                go.Body.Position = position;
            }
        }

        public void Update(Observable observable)
        {
            GameObject destroyed = (GameObject)observable;
            Console.WriteLine("GameObjects before count: " + GameObjects.Count);
            GameObjects.TryRemove(destroyed.Id, out destroyed);
            Console.WriteLine("GameObjects after count: " + GameObjects.Count);
        }

        /// <summary>
        /// Called when an observable object calls the overloaded NotifyObservers which takes data.
        /// If observable.hp is below 1, killer is found using data (which should be killerID)
        /// On the killer character AddKill is then called
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="data"></param>
        public void Update(Observable observable, object data)
        {
            Character victim = observable as Character;
            // TODO normal cast, we want an exception if we pass an invalid object to this method.
            if (victim != null && victim.Health.Value < 1)
            {
                int killerId = Convert.ToInt32(data);
                GameObject go;
                if (GameObjects.TryGetValue(killerId, out go))
                {
                    Character killer = (Character)go;
                    killer.AddKill(victim);
                }
                else
                {
                    Console.WriteLine($"Map observed the death of {victim.Id}, but failed to find the killer.");
                }
            }
        }

        /// <summary>
        /// Checks for each GameObject in GameObjects, if it's a Character.
        /// If it is, the DecayScore is called on the Character object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DecayScores(object sender, ElapsedEventArgs e)
        {
            foreach (var go in GameObjects.Values)
            {
                if (go is Character)
                {
                    Character character = go as Character;
                    character.DecayScore();
                }
            }
        }
    }
}