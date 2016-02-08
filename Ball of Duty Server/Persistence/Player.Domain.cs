using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Utility;
using Entity.Entities;
using Utility;

namespace Ball_of_Duty_Server.Persistence
{
    public partial class Player
    {
        private ICharacter _currentCharacter = null;

        public ICharacter CurrentCharacter
        {
            get { return _currentCharacter; }
            set
            {
                _currentCharacter?.UnregisterAll(this);
                _currentCharacter = value;
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
        private void AddGold(IObservable observable, object data)
        {
            Gold += 10;
            using (DatabaseContainer dc = new DatabaseContainer())
            {
                dc.Entry(this).State = EntityState.Modified;
                dc.SaveChanges();
            }
        }

        private void DestroyEvent(IObservable observable, object data)
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