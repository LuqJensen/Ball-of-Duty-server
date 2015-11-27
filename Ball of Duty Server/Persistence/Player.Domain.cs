using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                _currentCharacter?.Unregister(Observation.EXTERMINATION, this);
                _currentCharacter?.Unregister(Observation.KILLING, this);
                _currentCharacter = value;
                _currentCharacter.HighScore = this.HighScore;
                _currentCharacter.Register(Observation.ACQUISITION_OF_GOLD, this, AddGold);
                _currentCharacter.Register(Observation.EXTERMINATION, this, DestroyEvent);
                _currentCharacter.Register(Observation.KILLING, this, DestroyEvent);
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

        private void DestroyEvent(Observable observable, object data)
        {
            if (CurrentCharacter.HighScore > this.HighScore)
            {
                this.HighScore = CurrentCharacter.HighScore;
            }

            using (DatabaseContainer dc = new DatabaseContainer())
            {
                dc.Entry(this).State = EntityState.Modified;
                dc.SaveChanges();
            }
        }
    }
}