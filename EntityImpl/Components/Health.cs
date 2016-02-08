using Entity;
using Entity.Components;

namespace EntityImpl.Components
{
    public class Health : IHealth
    {
        public int HealthRegen { get; set; }
        public int Max { get; set; }
        public int Value { get; set; }
        public IGameObject GameObject { get; set; }

        public Health(IGameObject gameObject, int maxHealth, int healthRegen)
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

        public void TakeDamage(int amount, IGameObject shooter)
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