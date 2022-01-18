using Game.Utility;

using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Level.Triggers
{
    [DefaultExecutionOrder(-1)]
    public sealed class CurrentRoomText : MonoBehaviourSinglenton<CurrentRoomText>
    {
        private static readonly WaitForSeconds wait = new WaitForSeconds(2);

        private object text;
        private int version;

        protected override void Awake_()
        {
            if ((text = GetComponent<Text>()) == null && (text = GetComponent<TMP_Text>()) == null)
                Debug.LogWarning($"No {nameof(Text)} not {nameof(TMP_Text)} component found.");
        }

        public static void SetRoomName(string roomName)
        {
            Instance.StartCoroutine(Work());

            IEnumerator Work()
            {
                const float inDuration = .5f;
                const float outDuration = 1;

                int version_ = ++Instance.version;

                Color color;
                object text = Instance.text;
                {
                    if (text is Text text_)
                    {
                        text_.text = roomName;
                        color = text_.color;
                    }
                    else if (text is TMP_Text text_2)
                    {
                        text_2.text = roomName;
                        color = text_2.color;
                    }
                    else
                    {
                        Debug.LogWarning("No current room text was found.");
                        yield break;
                    }
                }
                float alpha = color.a;

                float time = inDuration;
                while (time > 0 && version_ == Instance.version)
                {
                    color.a = alpha * (1 - (time / inDuration));
                    Set();
                    yield return null;
                    time -= Time.deltaTime;
                }

                color.a = alpha;
                Set();
                yield return wait;

                time = outDuration;
                while (time > 0 && version_ == Instance.version)
                {
                    color.a = alpha * (time / outDuration);
                    Set();
                    yield return null;
                    time -= Time.deltaTime;
                }

                {
                    if (text is Text text_)
                    {
                        text_.text = "";
                        color = text_.color;
                    }
                    else if (text is TMP_Text text_2)
                    {
                        text_2.text = "";
                        color = text_2.color;
                    }
                }

                color.a = alpha;
                Set();

                void Set()
                {
                    object text = Instance.text;
                    if (text is Text text_)
                        text_.color = color;
                    else if (text is TMP_Text text_2)
                        text_2.color = color;
                }
            }
        }
    }
}