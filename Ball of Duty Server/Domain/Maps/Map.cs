using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using Ball_of_Duty_Server.DAO;
using Ball_of_Duty_Server.Domain.Communication;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.DTO;
using Ball_of_Duty_Server.Utility;
using Timer = System.Timers.Timer;

namespace Ball_of_Duty_Server.Domain.Maps
{
    public class Map : IObserver
    {
        private ConcurrentDictionary<int, bool> _gameObjectsActive;
        private Thread _updateThread;

        public int Width { get; set; }

        public int Height { get; set; }


        public ConcurrentDictionary<int, GameObject> GameObjects { get; set; } =
            new ConcurrentDictionary<int, GameObject>();

        public Broker Broker { get; set; }

        public Game Game { get; set; }

        public Map()
        {
            Width = 1100;
            Height = 650; // default
            MapGenerator.GenerateMap(this);
            Broker = new Broker(this);
            _updateThread = new Thread(Activate);
            _updateThread.Start();
        }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            MapGenerator.GenerateMap(this);
            Broker = new Broker(this);
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
            var gameobjects = GameObjects.Values;
            foreach (GameObject go in gameobjects)
            {
                go.Update(gameobjects);
            }
            Broker.WritePositionUpdate(GetPositions());
            Broker.WriteHealthUpdate(GetHealthObjects());
            Broker.WriteScoreUpdate(GetScoreObjects());
            //TODO Shouldnt be send each update, Health and score doesnt at all update that many times.
        }

        private List<GameObjectDAO> GetHealthObjects()
        {
            return GameObjects.Values.OfType<Character>().Select(character => new GameObjectDAO()
            {
                MaxHealth = character.Health.Max, // Yes max health can/should change as you get higher score.
                Health = character.Health.Value,
                Id = character.Id
            }).ToList();
        }

        private List<GameObjectDAO> GetScoreObjects()
        {
            return GameObjects.Values.OfType<Character>().Select(character => new GameObjectDAO()
            {
                Score = character.Score,
                Id = character.Id
            }).ToList();
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


            return gameObjects.ToArray();
        }

        private List<GameObjectDAO> GetPositions()
        {
            return (
                from go in GameObjects.Values
                where !(go is Bullet) && !(go is Wall)
                select new GameObjectDAO()
                {
                    Id = go.Id,
                    X = go.Body.Position.X,
                    Y = go.Body.Position.Y
                }).ToList();
        }

        private void CheckTimeouts(object sender, ElapsedEventArgs e)
        {
            if (_gameObjectsActive.Count == 0)
                return;

            var removeTimeoutObjects = new List<int>();

            foreach (var go in GameObjects.Values)
            {
                if (go is Wall || go is Bullet) // Bullet needs anbother kind of timeout check
                {
                    continue;
                }
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
                if (GameObjects.TryGetValue(rto, out go))
                {
                    if (go.Destroy())
                    {
                        RemoveObject(go.Id);
                    }
                }
            }

            _gameObjectsActive.Clear();
        }

        private void RemoveObject(int id)
        {
            GameObject go;
            if (GameObjects.TryRemove(id, out go))
            {
                Broker.WriteObjectDestruction(go.Id);
                go.UnRegister(this);
            }
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
            RemoveObject(destroyed.Id);
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
                    Broker.KillNotification(victim.Id, killer.Id);
                    RemoveObject(victim.Id);

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
            foreach (GameObject go in GameObjects.Values)
            {
                Character character = go as Character;
                character?.DecayScore();
            }
        }
    }
}