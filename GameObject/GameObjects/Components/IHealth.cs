namespace GameObject.GameObjects.Components
{
    public interface IHealth
    {
        int HealthRegen { get; set; }
        int Max { get; set; }
        int Value { get; set; }
        IGameObject GameObject { get; set; }

        void RegenHealth();
        void TakeDamage(int amount, IGameObject shooter);
    }
}