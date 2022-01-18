namespace Game.Level.Events
{
    public readonly struct CameraFarChanged
    {
        public readonly float Target;
        public readonly float Speed;

        public CameraFarChanged(float target, float speed)
        {
            Target = target;
            Speed = speed;
        }
    }
}