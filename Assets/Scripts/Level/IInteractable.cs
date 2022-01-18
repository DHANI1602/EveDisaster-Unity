namespace Game.Level
{
    public interface IInteractable
    {
        void Interact();
    }

    public interface IInteractableFeedback
    {
        void Highlight();

        void Unhighlight();

        void InSight();

        void OutOfSight();
    }
}