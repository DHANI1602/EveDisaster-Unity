using Game.Level;

using System.Collections.Generic;

using UnityEngine;

namespace Game.Player
{
    public sealed class InteractionVolume : MonoBehaviour
    {
        private List<(IInteractable, Collider)> collection = new List<(IInteractable, Collider)>();

        public bool ClosestTo(Ray ray, out IInteractable interactable, out Transform transform)
        {
            interactable = null;
            transform = null;

            Collider collider = null;
            float closestDistance = float.PositiveInfinity;

            for (int j = collection.Count - 1; j >= 0; j--)
            {
                (IInteractable i, Collider c) = collection[j];
                if (i == null || c == null)
                    collection.RemoveAt(j);
                else
                {
                    float distance = float.PositiveInfinity;
                    if (c.Raycast(ray, out RaycastHit info, float.PositiveInfinity))
                        distance = info.distance;

                    transform = c != null ? c.transform : ((MonoBehaviour)i).transform;
                    distance = Mathf.Min(distance, Vector3.Cross(ray.direction, transform.position - ray.origin).sqrMagnitude);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        interactable = i;
                        collider = c;
                    }
                }
            }

            if (collider != null)
                transform = collider.transform;

            return interactable != null;
        }

        private void OnTriggerEnter(Collider other)
        {
            IInteractable interactable = other.GetComponentInParent<IInteractable>();
            if (interactable != null && !collection.Contains((interactable, other)))
                collection.Add((interactable, other));
        }

        private void OnTriggerExit(Collider other)
        {
            IInteractable interactable = other.GetComponentInParent<IInteractable>();
            if (interactable != null)
                collection.Remove((interactable, other));
        }
    }
}