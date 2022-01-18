using System;

using UnityEngine;

namespace Game.Level.Triggers
{
    [Serializable]
    public sealed class SetActiveGameObjectPlayerTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Tooltip("GameObjects to affect.")]
        private Pack[] gameObjects;

        [Serializable]
        private struct Pack
        {
            [Tooltip("Game object to toggle.")]
            public GameObject gameObject;

            [Tooltip("True if the object will be activated when trigger enter. False if the object will be desactivad instead.")]
            public bool activate;

            [Tooltip("If it should do the opposite of `Activate` when player get out of the trigger.")]
            public bool invertWhenGetOut;
        }

        public override void OnEnter()
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                Pack pack = gameObjects[i];

                if (pack.gameObject != null)
                    pack.gameObject.SetActive(pack.activate);
            }
        }

        public override void OnExit()
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                Pack pack = gameObjects[i];

                if (!pack.invertWhenGetOut)
                    continue;

                if (pack.gameObject != null)
                    pack.gameObject.SetActive(!pack.activate);
            }
        }
    }
}