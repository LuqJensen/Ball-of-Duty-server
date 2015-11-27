using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Ball_of_Duty_Server.DAO;
using Ball_of_Duty_Server.Domain.Communication;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.DTO;
using Ball_of_Duty_Server.Utility;
using Ball_of_Duty_Server.Domain.Entities.CharacterSpecializations;

namespace Ball_of_Duty_Server.Domain.Maps
{
    public class Map
    {
        private ConcurrentDictionary<int, bool> _gameObjectsActive;

        private Thread _updateThread;
        private long _lastUpdate;
        // The precision of DateTime.Ticks is given in 100's of nanoseconds as stated here:
        // https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx
        private const long DATETIME_TICKS_TO_MILLISECONDS = 10000;
        private LightEvent characterUpdateEvent;
        private LightEvent timeoutEvent;

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
            timeoutEvent = new LightEvent(10000, CheckTimeouts);
            characterUpdateEvent = new LightEvent(300, CharacterStatUpdate); // every 300ms.
            _lastUpdate = DateTime.Now.Ticks;
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

        private void CharacterStatUpdate()
        {
            Broker.WriteHealthUpdate(GetHealthObjects());
            Broker.WriteScoreUpdate(GetScoreObjects());
        }

        public void Update()
        {
            long currentTime = DateTime.Now.Ticks;
            long deltaTime = (currentTime - _lastUpdate) / DATETIME_TICKS_TO_MILLISECONDS;
            _lastUpdate = currentTime;

            var gameobjects = GameObjects.Values;
            foreach (GameObject go in gameobjects)
            {
                go.Update(deltaTime, gameobjects);
            }
            Broker.WritePositionUpdate(GetPositions());

            characterUpdateEvent.Update(deltaTime);
            timeoutEvent.Update(deltaTime);
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
            GameObject owner;
            if (GameObjects.TryGetValue(ownerId, out owner))
            {
                Bullet bullet = new Bullet(new Point(x, y), new Vector(velocityX, velocityY), radius, damage, owner);
                if (!GameObjects.TryAdd(bullet.Id, bullet))
                {
                    Console.WriteLine($"Bullet {bullet.Id} dongoofed");
                    return 0;
                }
                bullet.Register(Observation.EXTERMINATION, this, ExterminationNotification);
                return bullet.Id;
            }
            return 0;
        }

        public Character AddCharacter(string nickname, Specializations specialization)
        {
            Character c = CharacterFactory.CreateCharacter(specialization);

            if (GameObjects.TryAdd(c.Id, c))
            {
                c.Register(Observation.KILLING, this, ExterminationNotification);

                Body b = c.Body;
                GameObjectDAO data = new GameObjectDAO
                {
                    X = b.Position.X,
                    Y = b.Position.Y,
                    Width = b.Width,
                    Height = b.Height,
                    Id = c.Id
                };

                Broker.WriteCreateCharacter(nickname, data);
            }
            return c;
        }

        public GameObjectDTO[] ExportGameObjects()
        {
            return (from go in GameObjects.Values
                let b = go.Body
                let body = new BodyDTO
                {
                    Position = new PointDTO
                    {
                        X = b.Position.X,
                        Y = b.Position.Y
                    },
                    Width = b.Width,
                    Height = b.Height,
                    Type = (int)b.Type
                }
                select new GameObjectDTO
                {
                    Id = go.Id,
                    Body = body
                }).ToArray();
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

        private void CheckTimeouts()
        {
            if (_gameObjectsActive.Count == 0)
                return;

            var removeTimeoutObjects = new List<int>();

            foreach (var go in GameObjects.Values)
            {
                if (!(go is Character))
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

        public void ExterminationNotification(Observable observable, object data)
        {
            GameObject victim = (GameObject)observable;

            GameObject killer = data as GameObject;
            if (killer != null)
            {
                Broker.KillNotification(victim.Id, killer.Id);
            }
            else
            {
                Console.WriteLine($"Gameobject {victim.Id} died a natural death.");
            }
            RemoveObject(victim.Id);
        }

        public void RemoveObject(int id)
        {
            GameObject go;
            if (GameObjects.TryRemove(id, out go))
            {
                Broker.WriteObjectDestruction(go.Id);
                go.UnregisterAll(this);
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
    }
}