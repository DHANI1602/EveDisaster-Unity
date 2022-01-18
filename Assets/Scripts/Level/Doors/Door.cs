using Enderlook.Unity.AudioManager;

using Game.Level.Triggers;
using Game.Player;
using Game.Utility;

using System;
using System.Collections;

using UnityEngine;
using UnityEngine.AI;

namespace Game.Level.Doors
{
    [RequireComponent(typeof(Animator))]
    public sealed class Door : MonoBehaviour, IInteractable
    {
        private static readonly WaitForFixedUpdate wait = new WaitForFixedUpdate();
        private static readonly WaitForSeconds waitASecond = new WaitForSeconds(1);

        [Header("Configuration")]
        [SerializeField, Range(0, 1), Tooltip("Determines the percent of door that is open or closed per second.")]
        private float doorSpeed;

        [SerializeField, Tooltip("Initial state of the door. If true it's opened.")]
        private bool initialStateIsOpened;

        [SerializeField, Tooltip("Key required to open this door. If null or empty, no key is required.")]
        private string key;

        [SerializeField, Tooltip("Determines if the door if locked from one direction. The lock expires when the door is interacted successfully once.")]
        private Locking locking;

        [Header("Setup")]
        [SerializeField, Tooltip("Doors to interact.")]
        private SubDoor[] doors;

        [Header("Sounds")]
        [SerializeField, Tooltip("Sound played when door is opening.")]
        private AudioFile openSound;

        [SerializeField, Tooltip("Sound played when door is closing.")]
        private AudioFile closeSound;

        [SerializeField, Tooltip("Sound played when player try to interact with a locked door and doesn't have the required key.")]
        private AudioFile lockedSound;

        [SerializeField, Tooltip("Sound played when door interacted from the blocked side of the door.")]
        private AudioFile blockSideSound;

        [Header("Animation Triggers")]
        [SerializeField, Tooltip("Name of the animation trigger when door is unlocked.")]
        private string unlockAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when door is locked.")]
        private string lockAnimationTrigger;

        [SerializeField, Tooltip("Name of the animation trigger when door is locked but palyer try to interact.")]
        private string lockedAnimationTrigger;

        [Header("Trigger Actions")]
        [SerializeReference, Tooltip("Actions executed when door is unlocked.")]
        private PlayerTriggerAction[] unlockActions;

        private Animator animator;

        private State state;
        private AnimationMode isInAnimation;

        private enum AnimationMode : byte
        {
            None,
            NotSwitch,
            Switch,
            Open,
            Close,
        }

        public enum State : byte
        {
            Opened,
            Closed,
            Moving,
        }

        public enum Locking : byte
        {
            None = default,
            Back,
            Front,
            RequireUnlockingByEvent,
        }

        private void Awake()
        {
            if (doors.Length == 0)
                return;

            for (int i = 0; i < doors.Length; i++)
                doors[i].Initialize(initialStateIsOpened);

            state = initialStateIsOpened ? State.Opened : State.Closed;

            animator = GetComponent<Animator>();

            if (animator != null)
            {
                SoundPlayer soundPlayer = GetComponent<SoundPlayer>();
                if (soundPlayer != null)
                    soundPlayer.enabled = false;
                if (locking == Locking.None)
                {
                    if (string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(unlockAnimationTrigger))
                    {
                        isInAnimation = AnimationMode.NotSwitch;
                        animator.SetTrigger(unlockAnimationTrigger);
                    }
                }
                else if (locking != Locking.None && !string.IsNullOrEmpty(lockAnimationTrigger))
                {
                    isInAnimation = AnimationMode.NotSwitch;
                    animator.SetTrigger(lockAnimationTrigger);
                }

                if (soundPlayer != null && isInAnimation == AnimationMode.NotSwitch)
                    StartCoroutine(Work());

                IEnumerator Work()
                {
                    while (isInAnimation != AnimationMode.None)
                        yield return null;

                    yield return waitASecond; // This is required because animations not call FromUnlock and FromLock at the exact end of the animation.

                    soundPlayer.enabled = false;
                }
            }
        }

        public void Interact()
        {
            if (isInAnimation != AnimationMode.None)
                return;

            if (locking != Locking.None)
            {
                if (locking == Locking.RequireUnlockingByEvent)
                    goto error;

                Vector3 toPlayer = (PlayerBody.Instance.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(toPlayer, transform.forward);
                if (dot > 0)
                {
                    if (locking == Locking.Front)
                        goto error;
                    goto next;
                }
                else
                {
                    if (dot == 0)
                        Debug.LogWarning("Door checking produced 0, which is neither front nor back, but it was fallbacked as if it were back.");

                    if (locking == Locking.Back)
                        goto error;
                    goto next;
                }

                error:
                Try.PlayOneShoot(transform, blockSideSound, "blockSide");
                return;

                next:;
            }

            if (!string.IsNullOrEmpty(key))
            {
                if (DoorKeysManager.HasKey(key))
                {
                    key = "";
                    locking = Locking.None;
                    isInAnimation = AnimationMode.Switch;
                    if (!Try.SetAnimationTrigger(animator, unlockAnimationTrigger, "unlock"))
                        FromUnlock();
                }
                else
                {
                    Try.PlayOneShoot(transform, lockedSound, "locked");
                    Try.SetAnimationTrigger(animator, lockedAnimationTrigger, "locked");
                }
                return;
            }

            Switch();
        }

        public void Mutate(bool? @lock, bool? open)
        {
            switch (@lock)
            {
                case null:
                {
                    if (open is bool open_)
                        Switch(open_);
                    else
                        Debug.LogWarning($"Both {nameof(@lock)} and {nameof(open)} were set to do nothing. This call made nothing.");
                    break;
                }
                case true:
                {
                    locking = Locking.RequireUnlockingByEvent;
                    switch (open)
                    {
                        case null:
                            isInAnimation = AnimationMode.NotSwitch;
                            break;
                        case true:
                            isInAnimation = AnimationMode.Open;
                            break;
                        case false:
                            isInAnimation = AnimationMode.Close;
                            break;
                    }
                    if (!Try.SetAnimationTrigger(animator, lockAnimationTrigger, "lock"))
                        FromLock();
                    break;
                }
                case false:
                {
                    if (locking != Locking.None || !string.IsNullOrEmpty(key))
                    {
                        locking = Locking.None;
                        key = "";
                        switch (open)
                        {
                            case null:
                                isInAnimation = AnimationMode.NotSwitch;
                                break;
                            case true:
                                isInAnimation = AnimationMode.Open;
                                break;
                            case false:
                                isInAnimation = AnimationMode.Close;
                                break;
                        }
                        if (!Try.SetAnimationTrigger(animator, unlockAnimationTrigger, "unlock"))
                            FromUnlock();
                    }
                    else if (open is bool open_)
                        Switch(open_);
                    break;
                }
            }
        }

        private void Switch()
        {
            switch (state)
            {
                case State.Moving:
                    return;
                case State.Opened:
                    Switch(false);
                    break;
                case State.Closed:
                    Switch(true);
                    break;
            }
        }

        private void Switch(bool open)
        {
            state = State.Moving;

            if (open)
                Try.PlayOneShoot(transform, openSound, "open");
            else
                Try.PlayOneShoot(transform, closeSound, "close");

            StartCoroutine(Work());

            IEnumerator Work()
            {
                for (float progress = 0; progress < 1; progress += doorSpeed * Time.fixedDeltaTime)
                {
                    for (int i = 0; i < doors.Length; i++)
                        doors[i].Move(open, progress);
                    yield return wait;
                }
                state = open ? State.Opened : State.Closed;
            }
        }

        public void FromUnlock()
        {
            foreach (PlayerTriggerAction action in unlockActions)
                action.OnEnter();
            switch (isInAnimation)
            {
                case AnimationMode.Switch:
                    Switch();
                    break;
                case AnimationMode.Open:
                    Switch(true);
                    break;
                case AnimationMode.Close:
                    Switch(false);
                    break;
            }
            isInAnimation = AnimationMode.None;
        }

        public void FromLock()
        {
            switch (isInAnimation)
            {
                case AnimationMode.Switch:
                    Switch();
                    break;
                case AnimationMode.Open:
                    Switch(true);
                    break;
                case AnimationMode.Close:
                    Switch(false);
                    break;
            }
            isInAnimation = AnimationMode.None;
        }

        public void Highlight() { }

        public void Unhighlight() { }

        public void InSight() { }

        public void OutOfSight() { }

        [Serializable]
        public struct SubDoor
        {
            [SerializeField, Tooltip("Door to move.")]
            private Transform door;

            [SerializeField, Tooltip("Open state of the door.")]
            private Transform opened;
            private Vector3 opened_;

            [SerializeField, Tooltip("Closed state of the door.")]
            private Transform closed;
            private Vector3 closed_;

            private NavMeshObstacle obstacle;

            public void Initialize(bool isOpened)
            {
                opened_ = opened.position;
                closed_ = closed.position;
                Destroy(opened.gameObject);
                Destroy(closed.gameObject);
                opened = null;
                closed = null;

                obstacle = door.GetComponentInChildren<NavMeshObstacle>();
                if (obstacle == null)
                    Debug.LogWarning($"Component {nameof(NavMeshObstacle)} not found in children of {door}.", door);

                door.gameObject.SetActive(true);

                if (isOpened)
                {
                    door.position = opened_;
                    if (obstacle != null)
                        obstacle.enabled = false;
                }
                else
                {
                    door.position = closed_;
                    if (obstacle != null)
                        obstacle.enabled = true;
                }
            }

            public void Move(bool open, float progress)
            {
                if (open)
                {
                    door.position = Vector3.Lerp(closed_, opened_, progress);
                    if (progress == 1 && obstacle != null)
                        obstacle.enabled = false;
                }
                else
                {
                    door.position = Vector3.Lerp(opened_, closed_, progress);
                    if (obstacle != null)
                        obstacle.enabled = true;
                }
            }
        }
    }
}