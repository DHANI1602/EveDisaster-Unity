using Game.Level.Triggers;
using Game.Utility;

using System.Collections.Generic;
using System.Text.RegularExpressions;

using TMPro;

using UnityEngine;

namespace Game.Level
{
    public sealed class TaskManager : MonoBehaviourSinglenton<TaskManager>
    {
        private static readonly Regex regex = new Regex(".*{(.*?)}.*");

        [SerializeField, Tooltip("Prefab used to create tasks.")]
        private GameObject taskPrefab;

        [SerializeField, Tooltip("Duration in seconds in which task is show during an update of tasks.")]
        private float showDuration = 2;

        private GameObject list;

        private readonly Dictionary<string, TMP_Text> tasks = new Dictionary<string, TMP_Text>();

        private float hideAt = float.PositiveInfinity;

        protected override void Awake_()
        {
            Transform child = transform.GetChild(0);
            if (child == null)
                Debug.LogError($"{nameof(TaskManager)} doesn't have a child.");
            list = child.gameObject;
            list.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                hideAt = float.PositiveInfinity;
                list.SetActive(!list.activeSelf);
            }
            else if (Time.time >= hideAt)
            {
                hideAt = float.PositiveInfinity;
                list.SetActive(false);
            }
        }

        public static void AddTask(string task)
        {
            TaskManager instance = Instance;
            Transform transform = instance.list.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                TMP_Text text = transform.GetChild(i).GetComponentInChildren<TMP_Text>();
                if (text == null)
                    continue;
                if (text.text == task)
                {
                    Debug.LogError("Task already added.");
                    return;
                }
            }

            TMP_Text newTask = Instantiate(instance.taskPrefab, transform).GetComponentInChildren<TMP_Text>();
            instance.tasks.Add(task, newTask);
            SetTaskText(task, newTask);
            instance.ShowForAWhile();
        }

        public static void CompleteTask(string task)
        {
            TaskManager instance = Instance;
            if (instance.tasks.TryGetValue(task, out TMP_Text text))
            {
                text.fontStyle |= FontStyles.Strikethrough;
                instance.ShowForAWhile();
            }
            else
                Debug.LogError($"Task \"{task}\" was not found");
        }

        public static void OnVariablesUpdated()
        {
            TaskManager instance = Instance;
            bool show = false;
            foreach (KeyValuePair<string, TMP_Text> kvp in instance.tasks)
            {
                string task = kvp.Key;
                TMP_Text text = kvp.Value;
                show |= SetTaskText(task, text);
            }
            if (show)
                instance.ShowForAWhile();
        }

        private static bool SetTaskText(string task, TMP_Text text)
        {
            Match match = regex.Match(task);
            while (match.Groups.Count == 2)
            {
                string variable = match.Groups[1].Value;
                task = task.Replace($"{{{variable}}}", CountersManager.GetValue(variable).ToString());
                match = regex.Match(task);
            }
            if (text.text != task)
            {
                text.text = task;
                return true;
            }
            return false;
        }

        private void ShowForAWhile()
        {
            if (!list.activeSelf)
            {
                hideAt = Time.time + showDuration;
                list.SetActive(true);
            }
        }
    }
}