using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Utility
{
    public class LightEvent
    {
        private readonly long _interval;
        private long _currentTime = 0;
        private readonly Action _timedEvent;
        private long decay_INTERVAL;
        private void v;

        public LightEvent(long interval, Action timedEvent)
        {
            _interval = interval;
            _timedEvent = timedEvent;
        }

        public LightEvent(long decay_INTERVAL, void v)
        {
            this.decay_INTERVAL = decay_INTERVAL;
            this.v = v;
        }

        public void Update(long deltaTime)
        {
            _currentTime += deltaTime;
            if (_currentTime >= _interval)
            {
                _timedEvent();
                _currentTime = 0;
            }
        }
    }
}