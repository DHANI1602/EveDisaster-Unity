using Game.Level;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Player
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("Key used to interact with an object.")]
        private KeyCode interactKey;

        [SerializeField, Min(0), Tooltip("Maximum distance of an object to be interactable.")]
        private float maximumInteractionDistance = 2;

        [SerializeField, Tooltip("Whenever it should auto pickup items when collide with them.")]
        private bool autoPickupItems;

        [SerializeField, Min(0), Tooltip("Maximum distance of an object to be in sight.")]
        private float maximumInSightDistance = 10;

        [Header("Setup")]
        [SerializeField, Tooltip("Camera where interaction ray is produced.")]
        private Camera interationSource;

        [SerializeField, Tooltip("Layers that collide with interaction ray.")]
        private LayerMask blockCollisionMask;

        [SerializeField, Tooltip("Layers that accept the interaction volume.")]
        private LayerMask interactCollisionMask;

        [SerializeField, Tooltip("Fallback used when raycast can't find object to interact.")]
        private InteractionVolume volume;

        private List<IInteractableFeedback> lastInteractions = new List<IInteractableFeedback>();
        private List<IInteractableFeedback> tmp = new List<IInteractableFeedback>();
        private List<IInteractableFeedback> tmp2 = new List<IInteractableFeedback>();
        private List<IInteractableFeedback> tmp3 = new List<IInteractableFeedback>();

        private Collider[] colliders = new Collider[1];
        private HashSet<IInteractableFeedback> interactables = new HashSet<IInteractableFeedback>();
        private HashSet<IInteractableFeedback> lastInteractables = new HashSet<IInteractableFeedback>();

#if UNITY_EDITOR
        private List<Vector3> gizmos = new List<Vector3>();
#endif

        private void FixedUpdate()
        {
            if (!GameManager.IsGameRunning)
                return;

            Ray ray = interationSource.ViewportPointToRay(new Vector3(.5f, .5f));
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo, maximumInteractionDistance, blockCollisionMask | interactCollisionMask, QueryTriggerInteraction.Collide))
            {
                IInteractable interactable = hitInfo.transform.GetComponentInParent<IInteractable>();
                if (interactable != null)
                    GetComponentsInHierarchy(hitInfo.transform, tmp);
                else if (volume != null && volume.ClosestTo(ray, out interactable, out Transform interactableTransform))
                    GetComponentsInHierarchy(interactableTransform, tmp);
                else
                    goto fallback;

                for (int i = 0; i < tmp.Count; i++)
                {
                    IInteractableFeedback feedback = tmp[i];
                    if (!lastInteractions.Contains(feedback))
                        feedback.Highlight();
                }

                for (int i = 0; i < lastInteractions.Count; i++)
                {
                    IInteractableFeedback feedback = lastInteractions[i];
                    if (feedback.Equals(null)) // Object was destroyed.
                        continue;
                    if (!tmp.Contains(feedback))
                        feedback.Unhighlight();
                }

                lastInteractions.Clear();
                List<IInteractableFeedback> list = lastInteractions;
                lastInteractions = tmp;
                tmp = list;

                if (Input.GetKeyDown(interactKey))
                    interactable.Interact();

                goto next;
            }

            fallback:
            for (int i = 0; i < lastInteractions.Count; i++)
            {
                IInteractableFeedback feedback = lastInteractions[i];
                if (feedback.Equals(null)) // Object was destroyed.
                    continue;
                lastInteractions[i].Unhighlight();
            }
            lastInteractions.Clear();

            next:
            int count = Physics.OverlapSphereNonAlloc(transform.position, maximumInSightDistance, colliders, interactCollisionMask, QueryTriggerInteraction.Collide);
            if (count == colliders.Length)
            {
                colliders = Physics.OverlapSphere(transform.position, maximumInSightDistance, interactCollisionMask, QueryTriggerInteraction.Collide);
                count = colliders.Length;
                // We increase the size of the array to prevent an allocation on the next update.
                Array.Resize(ref colliders, count + 1);
            }

#if UNITY_EDITOR
            gizmos.Clear();
#endif

            for (int i = 0; i < count; i++)
            {
                Collider collider = colliders[i];
                GetComponentsInHierarchy(collider.transform, tmp);

                for (int j = 0; j < tmp.Count; j++)
                {
                    IInteractableFeedback feedback = tmp[j];
                    if (!(interactables.Contains(feedback) || lastInteractables.Contains(feedback)))
                    {
                        if (!CheckRaycast(tmp, collider.transform.position))
                            CheckCollider(tmp, collider);
                        break;
                    }
                }
            }

            tmp.Clear();
            tmp2.Clear();

            foreach (IInteractableFeedback feedback in lastInteractables)
                if (!interactables.Contains(feedback))
                    feedback.OutOfSight();
            lastInteractables.Clear();

            HashSet<IInteractableFeedback> set = lastInteractables;
            lastInteractables = interactables;
            interactables = lastInteractables;
        }

        private bool CheckCollider(List<IInteractableFeedback> feedbacks, Collider collider)
        {
            switch (collider)
            {
                case BoxCollider boxCollider:
                    return CheckCube<Exact>(boxCollider.size, boxCollider);
                case SphereCollider sphereCollider:
                {
                    Vector3 radius = sphereCollider.radius * sphereCollider.transform.lossyScale;
                    Vector3 center = sphereCollider.bounds.center;

                    int n = Mathf.CeilToInt(Mathf.Max(8 * radius.magnitude, 8));
                    float offset = 2f / n;
                    float increment = Mathf.PI * (3 - Mathf.Sqrt(5)); // Golden angle in radains
                    for (int j = 0; j < n; j++)
                    {
                        float y = (j * offset) - 1 + (offset / 2);
                        float r = Mathf.Sqrt(1 - (y * y));
                        float phi = j * increment;
                        float x = Mathf.Cos(phi) * r;
                        float z = Mathf.Sin(phi) * r;

                        if (CheckRaycast(feedbacks, new Vector3(x * radius.x, y * radius.y, z * radius.z) + center))
                            return true;
                    }
                    break;
                }
                case CapsuleCollider capsuleCollider:
                {
                    Vector3 size = Vector3.one * capsuleCollider.radius;

                    switch (capsuleCollider.direction)
                    {
                        case 0:
                            size.x = capsuleCollider.height;
                            break;
                        case 1:
                            size.y = capsuleCollider.height;
                            break;
                        case 2:
                            size.z = capsuleCollider.height;
                            break;
                    }

                    // TODO: This doesn't not check property the curved part of capsules.

                    return CheckCube<Closest>(size, capsuleCollider);
                }
                case MeshCollider meshCollider:
                {
                    foreach (Vector3 vertice in meshCollider.sharedMesh.vertices)
                        if (CheckRaycast(feedbacks, vertice))
                            return true;
                    return false;
                }
            }

            return false;

            bool CheckCube<T>(Vector3 size, Collider collider)
            {
                Vector3 halfSize = size * .5f;
                Transform boxTransform = collider.transform;
                Vector3 lossyScale = boxTransform.lossyScale;
                Matrix4x4 matrix = Matrix4x4.TRS(collider.bounds.center, boxTransform.localRotation, lossyScale);

                int xSteps = Mathf.Max(Mathf.CeilToInt(2 * size.x * lossyScale.x), 2);
                int ySteps = Mathf.Max(Mathf.CeilToInt(2 * size.y * lossyScale.y), 2);
                int zSteps = Mathf.Max(Mathf.CeilToInt(2 * size.z * lossyScale.z), 2);

                if (xSteps == 2 && ySteps == 2 && zSteps == 2)
                {
                    if (CheckRaycast(feedbacks, GetPoint(new Vector3(-halfSize.x, -halfSize.y, -halfSize.z))))
                        return true;
                    if (CheckRaycast(feedbacks, GetPoint(new Vector3(-halfSize.x, -halfSize.y, +halfSize.z))))
                        return true;
                    if (CheckRaycast(feedbacks, GetPoint(new Vector3(-halfSize.x, +halfSize.y, +halfSize.z))))
                        return true;
                    if (CheckRaycast(feedbacks, GetPoint(new Vector3(+halfSize.x, +halfSize.y, +halfSize.z))))
                        return true;
                    if (CheckRaycast(feedbacks, GetPoint(new Vector3(+halfSize.x, +halfSize.y, -halfSize.z))))
                        return true;
                    if (CheckRaycast(feedbacks, GetPoint(new Vector3(+halfSize.x, -halfSize.y, -halfSize.z))))
                        return true;
                    if (CheckRaycast(feedbacks, GetPoint(new Vector3(+halfSize.x, -halfSize.y, +halfSize.z))))
                        return true;
                    if (CheckRaycast(feedbacks, GetPoint(new Vector3(-halfSize.x, +halfSize.y, -halfSize.z))))
                        return true;
                }
                else
                {
                    for (float x = 0; x <= xSteps; x++)
                    {
                        for (float y = 0; y <= ySteps; y++)
                        {
                            for (float z = 0; z <= zSteps; z++)
                            {
                                if (CheckRaycast(feedbacks, GetPoint(new Vector3(
                                    halfSize.x * Mathf.Lerp(-1, 1, x / xSteps),
                                    halfSize.y * Mathf.Lerp(-1, 1, y / ySteps),
                                    halfSize.z * Mathf.Lerp(-1, 1, z / zSteps)
                                ))))
                                    return true;
                            }
                        }
                    }
                }

                return false;

                Vector3 GetPoint(Vector3 position)
                {
                    if (typeof(T) == typeof(Exact))
                        return matrix.MultiplyPoint3x4(position);
                    if (typeof(T) == typeof(Closest))
                        return collider.ClosestPoint(matrix.MultiplyPoint3x4(position));
                    Debug.Assert(false, "Invalid generic type.");
                    return default;
                }
            }
        }

        private bool CheckRaycast(List<IInteractableFeedback> feedbacks, Vector3 end)
        {
#if UNITY_EDITOR
            gizmos.Add(end);
#endif

            if (Physics.Linecast(interationSource.transform.position, end, out RaycastHit hitInfo, blockCollisionMask))
            {
                GetComponentsInHierarchy(hitInfo.collider.transform, tmp2);
                if (tmp2.Count == 0)
                    return false;

                for (int i = feedbacks.Count - 1; i >= 0; i--)
                {
                    IInteractableFeedback feedback = feedbacks[i];
                    if (tmp2.Contains(feedback) && !lastInteractables.Contains(feedback))
                    {
                        feedback.InSight();
                        lastInteractables.Add(feedback);
                        feedbacks.RemoveAt(i);
                    }
                }
                return feedbacks.Count == 0;
            }

            for (int i = 0; i < feedbacks.Count; i++)
            {
                IInteractableFeedback feedback = feedbacks[i];
                if (!lastInteractables.Contains(feedback))
                {
                    feedback.InSight();
                    lastInteractables.Add(feedback);
                }
            }

            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = lastInteractions.Count > 0 ? Color.yellow : Color.green;
            Ray ray = interationSource.ViewportPointToRay(new Vector3(.5f, .5f));
            Gizmos.DrawLine(ray.origin, ray.GetPoint(maximumInteractionDistance));

            Gizmos.color = Color.magenta;
            foreach (Vector3 point in gizmos)
                Gizmos.DrawSphere(point, .02f);
        }
#endif

        private void OnCollisionEnter(Collision collision)
        {
            if (autoPickupItems && collision.transform.TryGetComponent(out IPickup pickup))
                pickup.Pickup();
        }

        public void GetComponentsInHierarchy(Transform transform, List<IInteractableFeedback> feedbacks)
        {
            feedbacks.Clear();

            transform.GetComponentsInParent(false, feedbacks);

            if (transform.TryGetComponent(out IInteractableFeedback inObject) && !feedbacks.Contains(inObject))
                feedbacks.Add(inObject);

            tmp3.Clear();
            transform.GetComponentsInChildren(tmp3);
            for (int i = 0; i < tmp3.Count; i++)
            {
                IInteractableFeedback element = tmp3[i];
                if (element != inObject)
                    feedbacks.Add(element);
            }

            tmp3.Clear();
        }

        private struct Closest { }
        private struct Exact { }
    }
}