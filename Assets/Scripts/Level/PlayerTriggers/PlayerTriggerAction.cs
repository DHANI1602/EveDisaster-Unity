using System;

using UnityEngine;

namespace Game.Level.Triggers
{
    [Serializable]
    public abstract class PlayerTriggerAction
    {
        public virtual void Initialize(MonoBehaviour monoBehaviour) { }

        public virtual void OnEnter() { }

        public virtual void OnExit() { }
    }
}