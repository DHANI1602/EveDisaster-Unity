namespace Game.Level.Events
{
    public readonly struct LanternBatteryChanged
    {
        public readonly float OldValue;
        public readonly float NewValue;
        public readonly float MaxValue;
        public readonly bool IsRunningOutOfBattery;

        public float OldValuePercentage => OldValue / MaxValue;

        public float NewValuePercentage => NewValue / MaxValue;

        public LanternBatteryChanged(float oldValue, float newValue, float maxValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
            MaxValue = maxValue;
            IsRunningOutOfBattery = false;
        }

        public LanternBatteryChanged(float oldValue, float maxValue)
        {
            OldValue = oldValue;
            NewValue = 0;
            MaxValue = maxValue;
            IsRunningOutOfBattery = true;
        }

        public LanternBatteryChanged(float maxValue)
        {
            OldValue = 0;
            NewValue = 0;
            MaxValue = maxValue;
            IsRunningOutOfBattery = false;
        }
    }
}