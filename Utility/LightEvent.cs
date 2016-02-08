using System;

namespace Utility
{
    public class LightEvent
    {
        private readonly long _interval;
        private long _currentTime = 0;
        private readonly Action _timedEvent;

        public LightEvent(long interval, Action timedEvent)
        {
            _interval = interval;
            _timedEvent = timedEvent;
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

        public void Reset()
        {
            _currentTime = 0; // TODO consider locking here.
        }
    }
}