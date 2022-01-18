using System;

using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class SetTasksPlayerTriggerAction : PlayerTriggerAction
    {
        [SerializeField, Tooltip("Tasks to add.")]
        private string[] tasksToAdd;

        [SerializeField, Tooltip("Tasks to complete.")]
        private string[] tasksToComplete;

        public override void OnEnter()
        {
            foreach (string toAdd in tasksToAdd)
                TaskManager.AddTask(toAdd);

            foreach (string toComplete in tasksToComplete)
                TaskManager.CompleteTask(toComplete);

            tasksToAdd = tasksToComplete = Array.Empty<string>();
        }
    }
}