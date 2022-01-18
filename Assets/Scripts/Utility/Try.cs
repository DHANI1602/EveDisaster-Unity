using Enderlook.Unity.AudioManager;

using System.Collections.Generic;

using UnityEngine;

namespace Game.Utility
{
    public static class Try
    {
        public static bool SetAnimationBool(Animator animator, bool value, string parameterName, string metaParameterName, string animatorName = "")
        {
            if (animator == null)
            {
                if (string.IsNullOrEmpty(animatorName))
                    Debug.LogWarning("Missing animator.");
                else
                    Debug.LogWarning($"Missing {animatorName} animator.");
                return false;
            }
            if (string.IsNullOrEmpty(parameterName))
            {
                Debug.LogWarning($"Missing {metaParameterName} animation boolean parameter.");
                return false;
            }
            else
            {
                animator.SetBool(parameterName, value);
                return true;
            }
        }

        public static bool SetAnimationTrigger(Animator animator, string triggerName, string metaTriggerName, string animatorName = "")
        {
            if (animator == null)
            {
                if (string.IsNullOrEmpty(animatorName))
                    Debug.LogWarning("Missing animator.");
                else
                    Debug.LogWarning($"Missing {animatorName} animator.");
                return false;
            }
            if (string.IsNullOrEmpty(triggerName))
            {
                Debug.LogWarning($"Missing {metaTriggerName} animation trigger.");
                return false;
            }
            else
            {
                animator.SetTrigger(triggerName);
                return true;
            }
        }

        public static bool SetAnimationName(Animator animator, string animationName, string metaAnimationName, string animatorName = "")
        {
            if (animator == null)
            {
                if (string.IsNullOrEmpty(animatorName))
                    Debug.LogWarning("Missing animator.");
                else
                    Debug.LogWarning($"Missing {animatorName} animator.");
                return false;
            }
            if (string.IsNullOrEmpty(animationName))
            {
                Debug.LogWarning($"Missing {metaAnimationName} animation name.");
                return false;
            }
            else
            {
                animator.Play(animationName);
                return true;
            }
        }

        public static bool PlayOneShoot(Transform transform, AudioFile file, string fileName, string transformName = "")
        {
            if (file == null)
            {
                Debug.LogWarning($"Missing {fileName} sound.");
                return false;
            }

            if (transform == null)
            {
                if (string.IsNullOrEmpty(transformName))
                    Debug.LogWarning("Missing transform.");
                else
                    Debug.LogWarning($"Missing {transformName} transform.");
                return false;
            }

            AudioController.PlayOneShoot(file, transform);
            return true;
        }

        public static bool PlayOneShoot(Transform transform, AudioFile file, out AudioPlay audioPlay, string fileName, string transformName = "")
        {
            if (file == null)
            {
                Debug.LogWarning($"Missing {fileName} sound.");
                audioPlay = default;
                return false;
            }

            if (transform == null)
            {
                if (string.IsNullOrEmpty(transformName))
                    Debug.LogWarning("Missing transform.");
                else
                    Debug.LogWarning($"Missing {transformName} transform.");
                audioPlay = default;
                return false;
            }

            audioPlay = AudioController.PlayOneShoot(file, transform);
            return true;
        }

        public static bool PlayOneShoot(Transform transform, AudioFile file, List<AudioPlay> plays, string fileName, string transformName = "")
        {
            if (plays == null)
                return PlayOneShoot(transform, file, fileName, transformName);
            else if (PlayOneShoot(transform, file, out AudioPlay play, fileName, transformName))
            {
                play.Volume = 0;
                for (int i = 0; i < plays.Count; i++)
                {
                    AudioPlay element = plays[i];
                    if (!element.IsPlaying)
                    {
                        plays[i] = play;
                        return true;
                    }
                }
                plays.Add(play);
                return true;
            }
            else
                return false;
        }

        public static bool PlayLoop(Transform transform, AudioFile file, out AudioPlay audioPlay, string fileName, string transformName)
        {
            if (file == null)
            {
                Debug.LogWarning($"Missing {fileName} sound.");
                audioPlay = default;
                return false;
            }

            if (transform == null)
            {
                Debug.LogWarning($"Missing {transformName} transform.");
                audioPlay = default;
                return false;
            }

            audioPlay = AudioController.PlayLoop(file, transform);
            return true;
        }

        public static bool PlayOneShoot(Vector3 position, AudioFile file, string fileName)
        {
            if (file == null)
            {
                Debug.LogWarning($"Missing {fileName} sound.");
                return false;
            }
            else
            {
                AudioController.PlayOneShoot(file, position);
                return true;
            }
        }
    }
}