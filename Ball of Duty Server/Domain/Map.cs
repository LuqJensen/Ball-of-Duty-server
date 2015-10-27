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
        private HashSet<int> _gameObjectsActive; // possible race conditions.
        private Thread _updateThread;

        public Map()
        {
            Broker = new Broker(this);
            GameObjects = new ConcurrentDictionary<int, GameObject>();
            _updateThread = new Thread(Activate);
            _updateThread.Start();
        }

        public ConcurrentDictionary<int, GameObject> GameObjects { get; set; }

        public Broker Broker { get; set; }

        public Game Game { get; set; }

        public void Activate()
        {
            var timeoutCheck = new Timer();
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
                Console.WriteLine("removing "+rto);
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