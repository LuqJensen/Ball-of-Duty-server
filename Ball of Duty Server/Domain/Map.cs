using System;
using Ball_of_Duty_Server.DTO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Ball_of_Duty_Server.Domain
{
    public class Map
    {
        public Map(string ip)
        {
           Broker = new Broker(this, ip);
            GameObjects = new ConcurrentDictionary<int, GameObject>();
            Thread t = new Thread(Activate);
            t.Start();
        }

        public ConcurrentDictionary<int, GameObject> GameObjects { get; set; }

        public Broker Broker { get; set; }

        public Game Game { get; set; }

        public void Activate()
        {
            while (true)
            {
                this.Update();
            }
        }

        public void Deactivate()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            Broker.SendPositionUpdate(GetPositions(), 1/*Game.Id*/);
        }

        public List<ObjectPosition> GetPositions()
        {
            List<ObjectPosition> positions = new List<ObjectPosition>();
            foreach (GameObject go in GameObjects.Values)
            {
                positions.Add(new ObjectPosition(go.Id, go.Body.Position));
            }
            return positions;
        }

        public void UpdatePosition(Point position, int goId)
        {
            GameObject go;
            if (GameObjects.TryGetValue(goId, out go))
            {
                go.Body.Position = position;
            }
        }

        public List<GameObject> GetObjects()
        {
            return GameObjects.Values.ToList();
        }
    }
}