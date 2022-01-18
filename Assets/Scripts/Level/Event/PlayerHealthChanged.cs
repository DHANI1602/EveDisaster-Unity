namespace Game.Level.Events
{
    public readonly struct PlayerHealthChanged
    {
        public readonly float OldHealth;
        public readonly float NewHealth;
        public readonly float MaxHealth;

        public bool IsAlive => NewHealth > 0;

        public float NewHealthPercentage => NewHealth / MaxHealth;

        public float OldHealthPercentage => OldHealth / MaxHealth;

        public PlayerHealthChanged(float oldHealth, float newHealth, float maxHealth)
        {
            OldHealth = oldHealth;
            NewHealth = newHealth;
            MaxHealth = maxHealth;
        }
    }
}