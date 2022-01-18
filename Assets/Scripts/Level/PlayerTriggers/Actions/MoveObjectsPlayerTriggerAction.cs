using Enderlook.Unity.Toolset.Attributes;

using System;
using System.Collections;

using UnityEngine;

namespace Game.Level.Triggers
{
    [Serializable]
    public sealed class MoveObjectsPlayerTriggerAction : PlayerTriggerAction
    {
        private static readonly WaitForFixedUpdate wait = new WaitForFixedUpdate();

        [SerializeField, Tooltip("Object to move.")]
        private Pack[] objects;

        private MonoBehaviour monoBehaviour;

#pragma warning disable CS0649
        [Serializable]
        private struct Pack
        {
            [Tooltip("Object to move.")]
            public Transform transform;

            [Tooltip("Locations where it must move on enter.")]
            public Location[] locationsOnEnter;

            [Tooltip("Locations where it must move on exit.")]
            public Location[] locationsOnExit;

            [NonSerialized]
            public Coroutine coroutine;
        }

        [Serializable]
        private struct Location
        {
            [Tooltip("New position."), DrawVectorRelativeToTransform(true)]
            public Vector3 position;

            [Tooltip("Movement speed per second.")]
            public float speed;
        }
#pragma warning restore CS0649

        public override void Initialize(MonoBehaviour monoBehaviour)
        {
            this.monoBehaviour = monoBehaviour;
            Vector3 offet = monoBehaviour.transform.position;
            for (int i = 0; i < objects.Length; i++)
            {
                ref Pack pack = ref objects[i];
                for (int j = 0; j < pack.locationsOnEnter.Length; j++)
                    pack.locationsOnEnter[j].position += offet;
                for (int j = 0; j < pack.locationsOnExit.Length; j++)
                    pack.locationsOnExit[j].position += offet;
            }
        }

        public override void OnEnter()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                Pack pack = objects[i];
                if (pack.transform == null || pack.locationsOnEnter.Length == 0)
                    continue;
                objects[i].coroutine = monoBehaviour.StartCoroutine(Move(pack.transform, pack.locationsOnEnter, pack.coroutine));
            }
        }

        public override void OnExit()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                Pack pack = objects[i];
                if (pack.transform == null || pack.locationsOnExit.Length == 0)
                    continue;
                objects[i].coroutine = monoBehaviour.StartCoroutine(Move(pack.transform, pack.locationsOnExit, pack.coroutine));
            }
        }

        private IEnumerator Move(Transform transform, Location[] locations, Coroutine coroutine)
        {
            if (coroutine != null)
                yield return coroutine;

            foreach (Location location in locations)
            {
                while (true)
                {
                    if (transform == null)
                        yield break;

                    if (transform.position == location.position)
                        break;

                    transform.position = Vector3.MoveTowards(transform.position, location.position, location.speed * Time.fixedDeltaTime);

                    yield return wait;
                }
            }
        }
    }
}