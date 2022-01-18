using Game.Enemies;
using Game.Utility;

using System;

using UnityEngine;

namespace Game.Player.Weapons
{
    public struct ShootInformation
    {
        public Vector3 Direction;
        public Action<IDamagable> OnShootDamagable;
        public Action<IPushable, Vector3> OnShootPushable;
        public bool RequiresAmmunition;

        public ShootInformation(Vector3 direction)
        {
            Direction = direction;
            OnShootDamagable = null;
            OnShootPushable = null;
            RequiresAmmunition = true;
        }

        public int ProcessShoot(
            float maximumDistance,
            ref ParticlesPerSurface particlesPerSurface,
            GameObject decalPrefab,
            ProjectileMovement projectileEffectPrefab,
            int currentMagazineAmmo,
            Transform shootPoint,
            Transform projectileSpawnPoint,
            LayerMask colliderLayer,
            LayerMask decalObjectiveLayer
#if UNITY_EDITOR
            , GizmosShootLines gizmos
#endif
            )
        {
            if (RequiresAmmunition)
            {
                if (currentMagazineAmmo == 0)
                    return currentMagazineAmmo;
                else
                    currentMagazineAmmo--;
            }

#if UNITY_EDITOR
            gizmos.Add(shootPoint, Direction, maximumDistance, Color.magenta);
#endif

            ProjectileMovement projectileTrail;
            if (projectileEffectPrefab != null)
            {
                if (projectileSpawnPoint != null)
                    projectileTrail = ParticleSystemPool.GetOrInstantiate(projectileEffectPrefab, projectileSpawnPoint.position, Quaternion.identity);
                else
                {
                    Debug.LogWarning("Missing projectile spawn point.");
                    projectileTrail = ParticleSystemPool.GetOrInstantiate(projectileEffectPrefab, shootPoint.position, Quaternion.identity);
                }
            }
            else
            {
                Debug.LogWarning("Missing projectile effect prefab.");
                projectileTrail = null;
            }

            if (Physics.Raycast(shootPoint.position, Direction, out RaycastHit hitInfo, maximumDistance, colliderLayer, QueryTriggerInteraction.Collide))
            {
                projectileTrail?.SetDestination(hitInfo.point);

                IDamagable damagable = hitInfo.transform.GetComponentInParent<IDamagable>();

                if (damagable == null)
                {
                    particlesPerSurface.OnOther(hitInfo.point, hitInfo.normal);
#if UNITY_EDITOR
                    gizmos.Add(shootPoint, hitInfo.point, Color.yellow);
#endif
                    if (Physics.Raycast(shootPoint.position, Direction, out RaycastHit hitInfoDecal, maximumDistance, decalObjectiveLayer))
                    {
                        if (decalPrefab == null)
                            Debug.LogWarning("Missing decal prefab.");
                        else
                        {
                            if (hitInfoDecal.collider is MeshCollider)
                            {
                                GameObject decal = UnityEngine.Object.Instantiate(decalPrefab);
                                decal.transform.position = hitInfoDecal.point;
                                decal.transform.forward = -hitInfoDecal.normal;
                                projectileTrail?.SetDestination(hitInfoDecal.point);

#if UNITY_EDITOR
                                gizmos.Add(shootPoint, hitInfoDecal.point, Color.white);
#endif
                            }
                        }
                    }
                }
                else
                {
                    if (damagable is WeakSpot)
                        particlesPerSurface.OnWeakspot(hitInfo.point, hitInfo.normal);
                    else
                        particlesPerSurface.OnBody(hitInfo.point, hitInfo.normal);

                    OnShootDamagable?.Invoke(damagable);

#if UNITY_EDITOR
                    gizmos.Add(shootPoint, hitInfo.point, Color.red);
#endif
                }

                IPushable pushable = hitInfo.transform.GetComponentInParent<IPushable>();
                if (pushable != null)
                    OnShootPushable?.Invoke(pushable, Direction);
            }
            else
                projectileTrail.SetDirection(Direction, maximumDistance);

            return currentMagazineAmmo;
        }
    }
}