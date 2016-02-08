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
using Ball_of_Duty_Server.Domain.Factories;
using Ball_of_Duty_Server.Domain.GameObjects;
using Ball_of_Duty_Server.Domain.GameObjects.Components;
using Ball_of_Duty_Server.Domain.GameObjects.Components.Physics.Collision;

namespace Ball_of_Duty_Server.Domain.Maps
{
    public class Map
    {
        private Thread _updateThread;
        private long _lastUpdate;
        // The precision of DateTime.Ticks is given in 100's of nanoseconds as stated here:
        // https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx
        private const long DATETIME_TICKS_TO_MILLISECONDS = 10000;
        private LightEvent _characterStatUpdateEvent;

        public int Width { get; }

        public int Height { get; }

        public ConcurrentDictionary<int, GameObject> GameObjects { get; } = new ConcurrentDictionary<int, GameObject>();

        public Broker Broker { get; }

        public Map(int gameId, int width, int height)
        {
            Width = width;
            Height = height;
            MapGenerator.GenerateMap(this);
            Broker = new Broker(this, gameId);
            _updateThread = new Thread(Activate);
            _updateThread.Start();
        }

        public void Activate()
        {
            _characterStatUpdateEvent = new LightEvent(300, () => { Broker.WriteCharacterStatUpdate(GetCharacterStats()); }); // every 300ms.
            _lastUpdate = DateTime.Now.Ticks;

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
            long currentTime = DateTime.Now.Ticks;
            long deltaTime = (currentTime - _lastUpdate) / DATETIME_TICKS_TO_MILLISECONDS;
            _lastUpdate = currentTime;

            var gameobjects = GameObjects.Values;
            foreach (GameObject go in gameobjects)
            {
                go.Update(deltaTime, gameobjects);
            }
            Broker.WritePositionUpdate(GetPositions());

            _characterStatUpdateEvent.Update(deltaTime);
            Broker.Update(deltaTime);
        }

        private List<GameObjectDAO> GetCharacterStats()
        {
            return GameObjects.Values.OfType<Character>().Select(character => new GameObjectDAO()
            {
                MaxHealth = character.Health.Max, // Yes max health can/should change as you get higher score.
                Health = character.Health.Value,
                Score = character.Score,
                Id = character.Id
            }).ToList();
        }

        public int AddBullet(double x, double y, double velocityX, double velocityY, double diameter, int damage, int bulletType, int ownerId)
        {
            GameObject owner;
            if (GameObjects.TryGetValue(ownerId, out owner))
            {
                Bullet bullet = new Bullet(new Point(x, y), new Vector(velocityX, velocityY), diameter, damage, bulletType, owner);
                if (!GameObjects.TryAdd(bullet.Id, bullet))
                {
                    BoDConsole.WriteLine($"Could not add {bullet.Id} an existing gameobject has type {GameObjects[bullet.Id].Type}");
                    BoDConsole.WriteLine($"Bullet {bullet.Id} dongoofed");
                    return 0;
                }
                bullet.Register(Observation.EXTERMINATION, this, ExterminationNotification);

                bullet.WallId = CollisionHandler.GetFirstObjectIntersectingPath<Wall>(GameObjects.Values,
                    bullet);

                return bullet.Id;
            }
            return 0;
        }

        public Character AddCharacter(string nickname, int specialization)
        {
            Character c = CharacterFactory.CreateCharacter(specialization);

            if (GameObjects.TryAdd(c.Id, c))
            {
                c.Register(Observation.KILLING, this, ExterminationNotification);
                c.Register(Observation.EXTERMINATION, this, ExterminationNotification);

                Body b = c.Body;
                GameObjectDAO data = new GameObjectDAO
                {
                    X = b.Position.X,
                    Y = b.Position.Y,
                    Width = b.Width,
                    Height = b.Height,
                    Id = c.Id,
                    Specialization = (int)c.Specialization,
                    Type = (int)c.Type
                };

                Broker.WriteCreateCharacter(nickname, data);
            }
            return c;
        }

        public GameObjectDTO[] ExportGameObjects()
        {
            return (from go in GameObjects.Values select go.Export()).ToArray();
        }

        private List<GameObjectDAO> GetPositions()
        {
            return (
                from go in GameObjects.Values
                where go is IInhibited
                select new GameObjectDAO()
                {
                    Id = go.Id,
                    X = go.Body.Position.X,
                    Y = go.Body.Position.Y
                }).ToList();
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
//                Console.WriteLine($"Gameobject {victim.Id} died a natural death.");
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
                go.Body.Position = position;
            }
        }
    }
}