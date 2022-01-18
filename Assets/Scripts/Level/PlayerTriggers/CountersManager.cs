using Game.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Level.Triggers
{
    public sealed class CountersManager : MonoBehaviourSinglenton<CountersManager>
    {
        [SerializeField, Tooltip("Variables to track.")]
        private Variable[] variables;

        [Serializable]
        private struct Variable
        {
            [SerializeField, Tooltip("Name of the variable.")]
            private string name;

            [SerializeField, Tooltip("Initial value of the variable.")]
            private float value;

            [SerializeField, Tooltip("Conditions to check for.")]
            private Conditions[] conditions;

            public string Name => name;

            public bool TryMutate(string name, float mutation)
            {
                if (Name != name)
                    return false;

                value += mutation;
                for (int i = 0; i < conditions.Length; i++)
                    conditions[i].Test(value);

                return true;
            }

            public bool TryGetValue(string variableName, out float value)
            {
                value = this.value;
                return Name == variableName;
            }
        }

        [Serializable]
        private struct Conditions
        {
            [SerializeField, Tooltip("Comparison mode between variable value and compared value.")]
            private Comparand comparand;

            [SerializeField, Tooltip("Value to compare with the variable value.")]
            private float comparedValue;

            [SerializeReference, Tooltip("Actions to execute if value is comparison is expected.")]
            private PlayerTriggerAction[] actions;

            private bool hasEntered;

            private enum Comparand
            {
                EqualTo,
                UnequalTo,
                GreaterThan,
                GreaterThanOrEqual,
                LessThan,
                LessThanOrEqual,
            }

            public void Test(float value)
            {
                bool meet;
                switch (comparand)
                {
                    case Comparand.EqualTo:
                        meet = value == comparedValue;
                        break;
                    case Comparand.UnequalTo:
                        meet = value != comparedValue;
                        break;
                    case Comparand.GreaterThan:
                        meet = value > comparedValue;
                        break;
                    case Comparand.GreaterThanOrEqual:
                        meet = value >= comparedValue;
                        break;
                    case Comparand.LessThan:
                        meet = value < comparedValue;
                        break;
                    case Comparand.LessThanOrEqual:
                        meet = value <= comparedValue;
                        break;
                    default:
                        Debug.Assert(false, "Impossible State.");
                        meet = false;
                        break;
                }

                if (meet)
                {
                    if (hasEntered)
                        return;

                    foreach (PlayerTriggerAction action in actions)
                        action.OnEnter();
                }
                else
                {
                    if (!hasEntered)
                        return;

                    foreach (PlayerTriggerAction action in actions)
                        action.OnExit();
                }

                hasEntered = meet;
            }
        }

        public static void Mutate(string variableName, float mutation)
        {
            CountersManager instance = Instance;
            for (int i = 0; i < instance.variables.Length; i++)
            {
                if (instance.variables[i].TryMutate(variableName, mutation))
                {
                    TaskManager.OnVariablesUpdated();
                    return;
                }
            }

            Debug.LogWarning($"Variable with name {variableName} was not found.");
        }

        public static float GetValue(string variableName)
        {
            CountersManager instance = Instance;
            for (int i = 0; i < instance.variables.Length; i++)
                if (instance.variables[i].TryGetValue(variableName, out float value))
                    return value;

            Debug.LogWarning($"Variable with name {variableName} was not found.");
            return default;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if ((variables?.Length ?? 0) == 0)
                return;

            HashSet<string> names = new HashSet<string>();
            variables = variables.Where(e => names.Add(e.Name)).ToArray();
        }
#endif
    }
}