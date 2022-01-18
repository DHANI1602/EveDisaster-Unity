using Game.Utility;

using System;

namespace Game.Level.Events
{
    public sealed class EventManager : MonoBehaviourSinglenton<EventManager>
    {
        private readonly Enderlook.EventManager.EventManager eventManager = new Enderlook.EventManager.EventManager();

        public static void Subscribe<T>(Action<T> callback) => Instance.eventManager.Subscribe(callback);

        public static void Raise<T>(T argument) => Instance.eventManager.Raise(argument);
    }
}