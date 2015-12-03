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
        public int HealthRegen { get; set; }
        public int Max { get; set; }
        public int Value { get; set; }
        public GameObject GameObject { get; set; }

        public Health(GameObject gameObject, int maxHealth, int healthRegen)
        {
            GameObject = gameObject;
            Max = maxHealth;
            Value = maxHealth;
            HealthRegen = healthRegen;
        }

        public void RegenHealth()
        {
            Value += HealthRegen;

            if (Value > Max)
            {
                Value = Max;
            }
        }

        public void TakeDamage(int amount, GameObject shooter)
        {
            Value -= amount;
            if (Value < 1)
            {
                GameObject.Destroy(shooter);
            }
        }

        public override string ToString()
        {
            return $"Health [maxHealth={Max}]";
        }
    }
}