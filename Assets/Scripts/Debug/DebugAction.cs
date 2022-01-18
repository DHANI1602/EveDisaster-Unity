using Enderlook.Unity.Toolset.Attributes;

using Game.Level.Triggers;
using Game.Player;

using UnityEngine;
using UnityEngine.AI;

namespace Game
{
    public sealed class DebugAction : MonoBehaviour
    {
        [SerializeField, Tooltip("Key required to execute action.")]
        private KeyCode key;

        [SerializeField, Tooltip("Whenever player should be teleported into a different location.")]
        private bool teleport;

        [SerializeField, ShowIf(nameof(teleport)), DrawVectorRelativeToTransform, Tooltip("New location of player.")]
        private Vector3 newLocation;

        [SerializeReference, Tooltip("Additional actions to execute.")]
        private PlayerTriggerAction[] actions;

        private void Update()
        {
            if (Input.GetKeyDown(key))
            {
                if (teleport)
                {
                    NavMeshAgent agent = FindObjectOfType<PlayerController>().GetComponent<NavMeshAgent>();
                    agent.Warp(transform.position + newLocation);
                }

                foreach (PlayerTriggerAction action in actions)
                    action.OnEnter();
            }
        }
    }
}