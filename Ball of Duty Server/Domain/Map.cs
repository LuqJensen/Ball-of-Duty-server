using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using Ball_of_Duty_Server.DTO;
using Timer = System.Timers.Timer;

namespace Ball_of_Duty_Server.Domain
{
    public class Map
    {
        public HashSet<int> gameObjectsActive;

        public Map(string ip)
        {
            Broker = new Broker(this, ip);
            GameObjects = new ConcurrentDictionary<int, GameObject>();
            var t = new Thread(Activate);
            t.Start();
        }

        public ConcurrentDictionary<int, GameObject> GameObjects { get; set; }

        public Broker Broker { get; set; }

        public Game Game { get; set; }

        public void Activate()
        {
            var timeoutCheck = new Timer();
            timeoutCheck.Elapsed += checkTimeouts;
            timeoutCheck.Interval = 10000;
            timeoutCheck.Enabled = true;
            gameObjectsActive = new HashSet<int>();
            while (true)
            {
                Update();
            }
        }

        public void Deactivate()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            Broker.SendPositionUpdate(GetPositions(), 1 /*Game.Id*/);
        }

        public List<ObjectPosition> GetPositions()
        {
            var positions = new List<ObjectPosition>();
            foreach (var go in GameObjects.Values)
            {
                positions.Add(new ObjectPosition(go.Id, go.Body.Position));
            }
            return positions;
        }


        private void checkTimeouts(object sender, ElapsedEventArgs e)
            // An idea to handle it, not sure i like it too much.
        {
            Console.WriteLine("Checking");
            var removeTimeoutObjects = new List<int>();
            if (gameObjectsActive != null)
            {
                foreach (var go in GameObjects.Values)
                {
                    if (!gameObjectsActive.Contains(go.Id))
                    {
                        removeTimeoutObjects.Add(go.Id);
                        Console.WriteLine(go.Id + " Hasnt send messages for atleast 10 seconds");
                    }
                }
            }
            foreach (var rto in removeTimeoutObjects)
            {
                Console.WriteLine("removing "+rto);
                GameObject go;
                GameObjects.TryRemove(rto, out go);
            }

            gameObjectsActive = new HashSet<int>();
        }

        public void UpdatePosition(Point position, int goId)
        {
            GameObject go;
            if (GameObjects.TryGetValue(goId, out go))
            {
                gameObjectsActive.Add(go.Id);
                go.Body.Position = position;
            }
        }

        public List<GameObject> GetObjects()
        {
            return GameObjects.Values.ToList();
        }
    }
}