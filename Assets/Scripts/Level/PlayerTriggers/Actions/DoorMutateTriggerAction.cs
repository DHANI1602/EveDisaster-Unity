using Game.Level.Doors;

using System;

using UnityEngine;

namespace Game.Level.Triggers
{
    [Serializable]
    public sealed class DoorMutateTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Tooltip("Doors to mutate.")]
        private Pack[] doors;

        [Serializable]
        private struct Pack
        {
            [Tooltip("Door to mutate.")]
            public Door door;

            [Tooltip("New state of the lock.")]
            public LockState lockState;

            [Tooltip("New state of the door.")]
            public OpeningState openingState;
        }

        private enum LockState
        {
            DoNothing,
            Unlock,
            LockByEvent,
        }

        private enum OpeningState
        {
            DoNothing,
            Open,
            Close
        }

        public override void OnEnter()
        {
            foreach (Pack pack in doors)
            {
                if (pack.door == null)
                    return;

                bool? @lock;
                switch (pack.lockState)
                {
                    case LockState.LockByEvent:
                        @lock = true;
                        break;
                    case LockState.Unlock:
                        @lock = false;
                        break;
                    default:
                        @lock = null;
                        break;
                }

                bool? open;
                switch (pack.openingState)
                {
                    case OpeningState.Open:
                        open = true;
                        break;
                    case OpeningState.Close:
                        open = false;
                        break;
                    default:
                        open = null;
                        break;
                }

                pack.door.Mutate(@lock, open);
            }
        }
    }
}