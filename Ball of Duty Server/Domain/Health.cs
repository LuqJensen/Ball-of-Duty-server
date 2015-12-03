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
        public double HealthIncreaseFactor { get; set; }
        public int HealthRegen { get; set; }
        public int Max { get; set; }
        public int Value { get; set; }
        public GameObject GameObject { get; set; }

        public Health(GameObject gameObject, int maxHealth, int healthRegen, double healthIncreaseFactor)
        {
            GameObject = gameObject;
            Max = maxHealth;
            Value = maxHealth;
            HealthIncreaseFactor = healthIncreaseFactor;
            HealthRegen = healthRegen;
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