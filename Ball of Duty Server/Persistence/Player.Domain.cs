using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain;
using Ball_of_Duty_Server.Domain.Entities;
using Ball_of_Duty_Server.Utility;

namespace Ball_of_Duty_Server.Persistence
{
    public partial class Player : IObserver
    {
        private Character _currentCharacter = null;
        public int Gold { get; private set; } = 0;

        public Character CurrentCharacter
        {
            get { return _currentCharacter; }
            set
            {
                _currentCharacter?.UnRegister(this);
                _currentCharacter = value;
                _currentCharacter.Register(this);
            }
        }

        public void Update(Observable observable)
        {
            // throw new NotImplementedException();
        }

        /// <summary>
        /// Called when CurrentCharacter gets killed
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="data"></param>
        public void Update(Observable observable, object data)
        {
            AddGold();
        }

        /// <summary>
        /// Increments gold by 10
        /// </summary>
        private void AddGold()
        {
            Gold += 10;
        }
    }
}