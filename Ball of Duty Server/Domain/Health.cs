using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ball_of_Duty_Server.Domain.Entities;

namespace Ball_of_Duty_Server.Domain
{
    public class Health
    {
        public int Max { get; set; }
        public int Value { get; set; }
        public GameObject GameObject { get; set; }


        public Health(GameObject gameObject, int maxHealth)
        {
            GameObject = gameObject;
            Max = maxHealth;
            Value = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            Value -= amount;
            if (Value < 1)
            {
                GameObject.Destroy();
            }
        }

        public override string ToString()
        {
            return $"Health [maxHealth={Max}]";
        }
    }
}