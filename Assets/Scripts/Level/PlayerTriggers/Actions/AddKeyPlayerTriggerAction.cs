using Game.Level.Doors;

using System;

using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class AddKeyPlayerTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Tooltip("Keys to unlock.")]
        private string[] keys;

        public override void OnEnter()
        {
            foreach (string key in keys)
                DoorKeysManager.AddKey(key);

            keys = Array.Empty<string>();
        }
    }
}