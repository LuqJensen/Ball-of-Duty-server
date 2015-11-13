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
    public partial class Player
    {
        private Character _currentCharacter = null;

        public Character CurrentCharacter
        {
            get { return _currentCharacter; }
            set
            {
                _currentCharacter?.Unregister(Observation.ACQUISITION_OF_GOLD, this);
                _currentCharacter = value;
                _currentCharacter.Register(Observation.ACQUISITION_OF_GOLD, this, AddGold);
            }
        }

        /// <summary>
        /// Adds gold to the player.
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="data"></param>
        private void AddGold(Observable observable, object data)
        {
            Gold += 10; // TODO save to db here???
        }
    }
}