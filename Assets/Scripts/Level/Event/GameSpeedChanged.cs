namespace Game.Level.Events
{

    public readonly struct GameSpeedChanged
    {
        public readonly bool IsPaused;

        public GameSpeedChanged(bool isPaused) => IsPaused = isPaused;
    }
}