using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain;
using Ball_of_Duty_Server.Domain.Entities;

namespace Ball_of_Duty_Server.Persistence
{
    public partial class Player : IObserver
    {
        private int gold { get; set; }
        public Player()
        {

        }
        // Lucas, kan det her gøres bedre/pænere?
        private Character _currentCharacter = null;
        public Character CurrentCharacter
        {
            get { return _currentCharacter; }
            set
            {
                if (_currentCharacter != null)
                { _currentCharacter.UnRegister(this); }
                _currentCharacter = value;
                _currentCharacter.Register(this);
            }
        }

        public void Update(Observable observable)
        {
            throw new NotImplementedException();
        }
        /*
        Called when CurrentCharacter gets killed
        */
        public void Update(Observable observable, object data)
        {
            AddGold();
        }
        /*
        Increments gold by 10
        */
        private void AddGold()
        {
            gold = gold + 10;
        }
    }
}