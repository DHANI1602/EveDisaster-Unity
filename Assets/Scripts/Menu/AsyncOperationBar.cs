using Game.Utility;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    public sealed class AsyncOperationBar : MonoBehaviourSinglenton<AsyncOperationBar>
    {
        [SerializeField, Tooltip("Image used by progress bar.")]
        private Image progressBar;

        private Image background;
        private Queue<AsyncOperation[]> tasks = new Queue<AsyncOperation[]>();
        private bool allowBanish = true;

        protected override void Awake_()
        {
            DontDestroyOnLoad(gameObject);

            if (progressBar == null)
            {
                progressBar = null;
                Debug.LogWarning($"{nameof(progressBar)} is null.");
            }

            background = GetComponentInChildren<Image>();
            if (background == null)
            {
                background = null;
                Debug.LogWarning($"No component of type {nameof(Image)} found.");
            }
        }

        private void OnDestroy() => Destroy(gameObject);

        private void Update()
        {
            start:
            if (tasks.TryPeek(out AsyncOperation[] operations))
            {
                float progress = 0;
                for (int i = 0; i < operations.Length; i++)
                {
                    AsyncOperation operation = operations[i];
                    if (operation != null)
                    {
                        float progress_ = operation.progress;
                        progress += progress_;
                        if (progress_ >= .9f)
                            operation.allowSceneActivation = true;
                    }
                }

                progress /= operations.Length;

                if (progress >= .9f)
                {
                    if (!(progressBar is null) && progressBar.fillAmount < 1)
                    {
                        float fill = Mathf.MoveTowards(progressBar.fillAmount, 1, Time.deltaTime);
                        progressBar.fillAmount = fill;
                        background.color = new Color(0, 0, 0, Mathf.Max(fill, Mathf.MoveTowards(background.color.a, 1, Time.deltaTime * 3)));
                    }
                    else
                    {
                        tasks.Dequeue();

                        if (!(progressBar is null))
                            progressBar.fillAmount = 0;
                        goto start;
                    }
                }
                else if (!(progressBar is null))
                {
                    float fill = progressBar.fillAmount;
                    fill = Mathf.MoveTowards(fill, progress, Time.deltaTime) + Mathf.MoveTowards(progressBar.fillAmount - progress, .5f, Time.deltaTime / 5);
                    progressBar.fillAmount = fill;
                    background.color = new Color(0, 0, 0, Mathf.Max(fill, Mathf.MoveTowards(background.color.a, 1, Time.deltaTime * 3)));
                }
            }
            else if (allowBanish)
            {
                gameObject.SetActive(false);
                background.color = new Color(0, 0, 0, 0);
                progressBar.fillAmount = 0;
            }
            else
            {
                progressBar.fillAmount = 1;
                background.color = new Color(0, 0, 0, Mathf.MoveTowards(background.color.a, 1, Time.deltaTime * 3));
            }
        }

        public static void AllowBanish(bool allowBanish)
        {
            AsyncOperationBar instance = Instance;
            if (instance != null)
                instance.allowBanish = allowBanish;
        }

        public static void Enqueue(params AsyncOperation[] operations)
        {
            AsyncOperationBar instance = Instance;
            instance.tasks.Enqueue(operations);
            if (instance.tasks.Count == 1)
                instance.gameObject.SetActive(true);
        }
    }
}
