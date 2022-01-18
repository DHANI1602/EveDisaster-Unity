using Enderlook.Unity.AudioManager;
using Enderlook.Unity.Toolset.Attributes;

using Game.Enemies;
using Game.Level.Events;
using Game.Utility;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Player.Weapons
{
    public abstract partial class Weapon : MonoBehaviour
    {
        [field: Header("Ammunition")]
        [field: SerializeField, IsProperty, Min(1), Tooltip("Amount of ammunition per magazine.")]
        protected int MaximumMagazineAmmo { get; private set; }

        [SerializeField, Tooltip("Name of the ammunition type used.")]
        private string ammunitionName;

        [SerializeField, Tooltip("The amount of ammo in magazine when picked up.")]
        private int startMagazineAmmo;

        [Header("HUD")]
        [SerializeField, Tooltip("Display amount of magazine ammunition in the weapon.")]
        protected TextMesh ammunitionMagazineDisplay;

        [SerializeField, Tooltip("Display amount of total ammunition in the weapon.")]
        protected TextMesh ammunitionTotalDisplay;

        [field: IsProperty, SerializeField, Tooltip("Image show in the weapon holder of the HUD.")]
        public Sprite WeaponSprite { get; private set; }

        [Header("Reload")]
        [SerializeField, Tooltip("Name of the reload animation trigger.")]
        private string reloadAnimationTrigger;

        [SerializeField, Tooltip("Sound player when reloading with no ammunition.")]
        private AudioUnit noAmmoSound;

        [Header("Waiting")]
        [SerializeField, Tooltip("Name of the waiting animation trigger.")]
        private string waitAnimationTrigger;

        [SerializeField, ShowIf(nameof(waitAnimationTrigger), "", ComparisonMode.NotEqual), Min(0), Tooltip("Determines each how much idle time the awaiting animation is triggered.")]
        private float waitAnimationTimer = 6;

        [Header("Melee")]
        [SerializeField, Tooltip("Collider which produces the melee hit.")]
        private Collider meleeCollider;

        [SerializeField, ShowIf(nameof(meleeCollider), null, ComparisonMode.NotEqual), Tooltip("Name of the melee animation trigger. (The animation must execute Melee() at some point.)")]
        private string meleeAnimationTrigger;

        [SerializeField, ShowIf(nameof(meleeCollider), null, ComparisonMode.NotEqual), Tooltip("Amount of damage produced on a melee hit.")]
        private float meleeDamage;

        [SerializeField, ShowIf(nameof(meleeCollider), null, ComparisonMode.NotEqual), Tooltip("Amount of knockback force produced on a melee hit.")]
        private float meleeKnockback;

        [field: Header("Effects")]
        [field: IsProperty, SerializeField, Tooltip("Point where shoot particles will spawn.")]
        protected Transform ShootParticlesSpawnPoint { get; private set; }

        [SerializeField, Tooltip("Configuration of the spawed shell.")]
        private ShellConfiguration shellToSpawn;

        [Header("Primary Shoot")]
        [SerializeField, Tooltip("Configuration of the primary shoot.")]
        private ShootTypeConfiguration primaryConfiguration;

        [Header("Secondary Shoot")]
        [SerializeField, Tooltip("Configuration of the secondary shoot.")]
        private ShootTypeConfiguration secondaryConfiguration;

        [Header("Other")]
        [SerializeField, Tooltip("Animation trigger used to switch out from the weapon.")]
        private string switchOutAnimationTrigger;

        [SerializeField, Tooltip("Animation trigger used when the weapon is picked up by first time.")]
        private string pickedUpAnimationTrigger;

        [Header("Aim")]
        [SerializeField, Tooltip("Animation boolean parameter used to determine if weapon is aimmed.")]
        private string isAimmedAnimationParameter;

        [SerializeField, ShowIf(nameof(isAimmedAnimationParameter), "", ComparisonMode.NotEqual), Tooltip("Zoom value.")]
        private float zoom;

        [SerializeField, ShowIf(nameof(zoom), 0, ComparisonMode.Greater), Tooltip("Speed at which zoom is added.")]
        private float zoomInSpeed;

        [SerializeField, ShowIf(nameof(zoom), 0, ComparisonMode.Greater), Tooltip("Speed at which zoom is removed.")]
        private float zoomOutSpeed;

        public bool PrimaryCanBeHeldDown => primaryConfiguration.CanBeHeldDown;

        public bool SecondaryCanBeHeldDown => secondaryConfiguration.CanBeHeldDown;

        protected int CurrentMagazineAmmo
        {
            get => currentMagazineAmmo;
            set
            {
                currentMagazineAmmo = value;
                if (ammunitionMagazineDisplay == null)
                    Debug.LogWarning("Missing ammunition magazine display.");
                else
                    ammunitionMagazineDisplay.text = value.ToString();
            }
        }

        protected int CurrentTotalAmmo
        {
            get => ammunition.CurrentAmmunition;
            set
            {
                ammunition.CurrentAmmunition = value;
                if (ammunitionTotalDisplay == null)
                    Debug.LogWarning("Missing ammunition total display.");
                else
                    ammunitionTotalDisplay.text = value.ToString();
            }
        }

        protected Animator Animator { get; private set; }

        private Camera Camera {
            get {
                WeaponManager manager = this.manager;
                if (manager != null)
                {
                    Camera camera = manager.ShootCamera;
                    if (camera != null)
                        return camera;
                }
                return Camera.main;
            }
        }

        private float canShootAt;
        private ShootingAnimation shootingAnimation;

        private bool isInWaitingAnimation;
        private float nextWaitingAnimation;

        private bool isInReloadAnimation;

        protected bool IsReady => shootingAnimation == ShootingAnimation.No && Time.time >= canShootAt;

        private int currentMagazineAmmo;

        private AmmunitionType ammunition;

        private WeaponManager manager;

        private MeleeAttack meleeAttack;

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            Debug.Assert(Animator != null);
            ResetWaitingAnimation();
            if (meleeCollider == null)
                Debug.LogWarning("Missing melee collider.");
            else
            {
                meleeAttack = meleeCollider.gameObject.AddComponent<MeleeAttack>();
                meleeCollider.isTrigger = true;
                meleeCollider.enabled = false;
            }
        }

        public void Initialize(WeaponManager manager)
        {
            this.manager = manager;
            ammunition = manager.GetAmmunitionType(ammunitionName);
            CurrentMagazineAmmo = startMagazineAmmo;
            ForceTotalAmmunitionUIUpdate();
        }

        public void ForceTotalAmmunitionUIUpdate() => CurrentTotalAmmo = CurrentTotalAmmo; // Trigger UI update for total ammo.

        public void PlayAudioOneShoot(AudioUnit audio) => AudioController.PlayOneShoot(audio, transform.position);

        private void Update()
        {
            if (PlayerController.IsMoving)
                nextWaitingAnimation = Time.time + waitAnimationTimer;

            if (shootingAnimation != ShootingAnimation.No || isInWaitingAnimation || Time.time < nextWaitingAnimation)
                return;

            if (Try.SetAnimationTrigger(Animator, waitAnimationTrigger, "wait"))
                isInWaitingAnimation = true;
        }

        public void TryMeleeHit()
        {
            if (shootingAnimation != ShootingAnimation.No)
                return;
            if (meleeCollider == null)
                return;
            if (!Try.SetAnimationTrigger(Animator, meleeAnimationTrigger, "go to melee"))
                Melee();
        }

        private void Melee()
        {
            if (meleeCollider == null)
                return;

            StartCoroutine(Work());
            IEnumerator Work()
            {
                meleeCollider.enabled = true;
                yield return null;
                meleeCollider.enabled = false;

                foreach (IGrouping<Enemy, IDamagable> enemy in meleeAttack
                    .damagables
                    .GroupBy(e => ((Component)e).gameObject.GetComponentInParent<Enemy>()))
                {
                    float damage = meleeDamage / enemy.Count();
                    foreach (IDamagable damagable in enemy)
                        damagable.TakeDamage(damage);
                }

                foreach (IGrouping<Enemy, IPushable> enemy in meleeAttack
                    .pushables
                    .GroupBy(e => ((Component)e).gameObject.GetComponentInParent<Enemy>()))
                {
                    Vector3 direction = -(transform.position - enemy.Key.transform.position);
                    Vector3 force = direction.normalized * meleeKnockback / enemy.Count();
                    foreach (IPushable pushable in enemy)
                        pushable.TakeForce(force);
                }

                meleeAttack.damagables.Clear();
                meleeAttack.pushables.Clear();
            }
        }

        private void FromWait() => ResetWaitingAnimation();

        public void TryReload()
        {
            if (!IsReady)
                return;

            if (CurrentMagazineAmmo == MaximumMagazineAmmo)
                // We already has ammo.
                return;

            if (CurrentTotalAmmo == 0)
            {
                Try.PlayOneShoot(transform, noAmmoSound, "no ammo");
                // We don't have any ammo.
                return;
            }

            if (isInReloadAnimation)
                return;

            isInReloadAnimation = true;

            if (!Try.SetAnimationTrigger(Animator, reloadAnimationTrigger, "reload"))
                FromReload();
        }

        private void FromReload()
        {
            isInReloadAnimation = false;
            ResetWaitingAnimation();
            AfterReload();
        }

        protected virtual void AfterReload()
        {
            int need = MaximumMagazineAmmo - CurrentMagazineAmmo;
            if (need >= CurrentTotalAmmo)
            {
                CurrentMagazineAmmo += CurrentTotalAmmo;
                CurrentTotalAmmo = 0;
            }
            else
            {
                CurrentMagazineAmmo = MaximumMagazineAmmo;
                CurrentTotalAmmo -= need;
            }
        }

        public void TriggerOutAnimation()
        {
            shootingAnimation = ShootingAnimation.No;
            isInReloadAnimation = false;
            if (!Try.SetAnimationTrigger(Animator, switchOutAnimationTrigger, "switch out"))
                manager.FinalizeTriggerOutAnimation();
        }

        public void FromGetOut() => manager.FinalizeTriggerOutAnimation();

        public void TryPrimaryShoot()
        {
            if (!IsReady)
                // We are still in cooldown.
                return;

            if (CurrentMagazineAmmo > 0)
            {
                shootingAnimation = ShootingAnimation.Primary;

                if (!primaryConfiguration.PlayShootAnimation(Animator))
                {
                    FromShoot_Shoot();
                    FromShoot_End();
                }
            }
            else
                TryReload();
        }

        protected abstract void ToPrimaryShoot();

        public void TrySecondaryShoot()
        {
            if (!IsReady)
                // We are still in cooldown.
                return;

            if (CurrentMagazineAmmo > 0)
            {
                shootingAnimation = ShootingAnimation.Secondary;

                if (!secondaryConfiguration.PlayShootAnimation(Animator))
                {
                    FromShoot_Shoot();
                    FromShoot_End();
                }
            }
            else
                TryReload();
        }

        protected abstract void ToSecondaryShoot();

        private void FromShoot_Shoot()
        {
            shellToSpawn.Spawn();
            switch (shootingAnimation)
            {
                case ShootingAnimation.Primary:
                    ToPrimaryShoot();
                    primaryConfiguration.Shoot(manager, transform, ShootParticlesSpawnPoint);
                    break;
                case ShootingAnimation.Secondary:
                    ToSecondaryShoot();
                    secondaryConfiguration.Shoot(manager, transform, ShootParticlesSpawnPoint);
                    break;
            }
        }

        private void FromShoot_End()
        {
            switch (shootingAnimation)
            {
                case ShootingAnimation.Primary:
                    canShootAt = primaryConfiguration.GetNextShoot();
                    break;
                case ShootingAnimation.Secondary:
                    canShootAt = secondaryConfiguration.GetNextShoot();
                    break;
            }
            shootingAnimation = ShootingAnimation.No;
            ResetWaitingAnimation();
            isInReloadAnimation = false; // If the player shoot when it was reloading, this flag can still be set.
            if (CurrentMagazineAmmo == 0)
                TryReload();
        }

        protected Ray GetShootRay() => GetShootCamera().ViewportPointToRay(new Vector3(.5f, .5f));

        protected Transform GetShootPointTransform() => GetShootCamera().transform;

        protected Vector3 GetShootPointPosition() => GetShootCamera().transform.position;

        private Camera GetShootCamera()
        {
            Camera camera = Camera;
#if UNITY_EDITOR
            if (!Application.isPlaying && camera == null)
                camera = Camera.main;
#endif
            return camera;
        }

        public void SetAim(bool aim) => Try.SetAnimationBool(Animator, aim, isAimmedAnimationParameter, "is aimmed");

        public void AimIn() => CameraIronsight.AimIn(zoom, zoomInSpeed);

        public void AimOut() => CameraIronsight.AimOut(zoomOutSpeed);

        private void OnDisable()
        {
            shootingAnimation = ShootingAnimation.No;
            isInWaitingAnimation = false;
        }

        private void OnEnable()
        {
            if (ammunition != null)
                ForceTotalAmmunitionUIUpdate();
            EventManager.Raise(new CurrentWeaponChanged(this));
        }

        protected void ResetWaitingAnimation()
        {
            isInWaitingAnimation = false;
            nextWaitingAnimation = Time.time + waitAnimationTimer;
        }

        public void TriggerPickedUpAnimation() => Try.SetAnimationTrigger(Animator, pickedUpAnimationTrigger, "picked up");

#if UNITY_EDITOR
        private void OnValidate()
        {
            zoom = Mathf.Max(0, zoom);
            zoomInSpeed = Mathf.Max(0.1f, zoomInSpeed);
            zoomOutSpeed = Mathf.Max(0.1f, zoomOutSpeed);
        }
#endif

        private sealed class MeleeAttack : MonoBehaviour
        {
            [NonSerialized]
            public HashSet<IDamagable> damagables = new HashSet<IDamagable>();

            [NonSerialized]
            public HashSet<IPushable> pushables = new HashSet<IPushable>();

            private void OnTriggerEnter(Collider other)
            {
                IDamagable damagable = other.GetComponentInParent<IDamagable>();
                if (damagable != null)
                    damagables.Add(damagable);

                IPushable pushable = other.GetComponentInParent<IPushable>();
                if (pushable != null)
                    pushables.Add(pushable);
            }
        }

        private enum ShootingAnimation : byte
        {
            No,
            Primary,
            Secondary,
        }
    }
}
