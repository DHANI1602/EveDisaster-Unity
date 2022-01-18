using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Player.Weapons
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraIronsight : MonoBehaviour
    {
        private static readonly List<CameraIronsight> elements = new List<CameraIronsight>();

        private new Camera camera;
        private float fov;
        private Coroutine coroutine;

        private void Awake()
        {
            elements.Add(this);

            camera = GetComponent<Camera>();
            if (camera == null)
            {
                Debug.LogWarning($"Component of type {nameof(Camera)} not found.");
                return;
            }
            fov = camera.fieldOfView;
        }

        public static void AimIn(float zoom, float speed) => Aim(zoom, speed);

        public static void AimOut(float speed) => Aim(0, speed);

        private static void Aim(float offset, float speed)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                CameraIronsight element = elements[i];
                if (element == null)
                    elements.RemoveAt(i);
                else
                    element.Aim_(offset, speed);
            }
        }

        private void Aim_(float offset, float speed)
        {
            coroutine = StartCoroutine(Work());

            IEnumerator Work()
            {
                float target = fov - offset;

                if (coroutine != null)
                    yield return coroutine;

                while (true)
                {
                    camera.fieldOfView = Mathf.MoveTowards(camera.fieldOfView, target, speed * Time.deltaTime);

                    if (target == camera.fieldOfView)
                        yield break;

                    yield return null;
                }
            }
        }
    }
}