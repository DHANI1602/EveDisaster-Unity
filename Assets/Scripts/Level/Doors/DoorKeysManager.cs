using Game.Utility;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Level.Doors
{
    public sealed class DoorKeysManager : MonoBehaviourSinglenton<DoorKeysManager>
    {
        [SerializeField, Tooltip("Keys owned by the player")]
        private List<string> ownedKeys = new List<string>();

        public static bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return true;
            return Instance.ownedKeys.Contains(key);
        }

        public static void AddKey(string key)
        {
            Debug.Assert(!Instance.ownedKeys.Contains(key), "Already contains that key.");
            Instance.ownedKeys.Add(key);
        }

#if UNITY_EDITOR
        private void OnValidate() => ownedKeys = ownedKeys.Distinct().ToList();
#endif
    }
}