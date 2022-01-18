using Enderlook.Enumerables;
using Enderlook.Unity.Toolset.Attributes;

using Game.Level;
using Game.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Player.Weapons
{
    public sealed class WeaponManager : MonoBehaviourSinglenton<WeaponManager>
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("A list of all the weapon that exists.")]
        private WeaponPack[] allWeapons;

        [SerializeField, Tooltip("Types of ammunitions.")]
        private AmmunitionType[] ammunitions;

        [Header("Setup")]
        [SerializeField, Tooltip("Key used to reload ammunition.")]
        private KeyCode reloadKey;

        [SerializeField, Tooltip("Key used to melee hit.")]
        private KeyCode hitKey;

        [SerializeField, Tooltip("Key used to toggle light.")]
        private KeyCode lightKey;

        [SerializeField, Tooltip("Key used to change to previous weapon.")]
        private KeyCode previousKey;

        [SerializeField, Tooltip("Key used to change to next weapon.")]
        private KeyCode nextKey;

        [field: SerializeField, IsProperty, Tooltip("Camera where target point is generated.")]
        public Camera ShootCamera { get; private set; }

        [SerializeField, Tooltip("Animator of the camera used for player sight.")]
        private Animator eyeCameraAnimator;

        private int currentWeaponIndex;

        private Lantern[] lanterns;

        private int weaponScroll;

        private Weapon[] weapons;
        private Weapon[] newWeapons;

        private Queue<string> weaponsToUnlock = new Queue<string>();

        public Weapon CurrentWeapon => weapons[currentWeaponIndex];

        private Lantern CurrentLantern {
            get {
                Lantern lantern = lanterns[currentWeaponIndex];
                if (lantern == null)
                    return null;
                return lantern;
            }
        }

        protected override void Awake_()
        {
            OnValidate();

            weapons = allWeapons.Where(e => e.CanUse).Select(e => e.Weapon).ToArray();

            lanterns = new Lantern[weapons.Length];
            for (int i = 0; i < weapons.Length; i++)
            {
                Weapon weapon = weapons[i];
                weapon.Initialize(this);
                weapon.gameObject.SetActive(false);
                Lantern lantern = weapon.gameObject.GetComponentInChildren<Lantern>();
                if (lantern != null)
                {
                    lantern.Initialize(this);
                    lanterns[i] = lantern;
                }
            }
            CurrentWeapon.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!GameManager.IsGameRunning)
                return;

            if (weapons.Length > 1)
            {
                if (weaponScroll == 0)
                {
                    float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
                    if (scrollWheel == 0 && !Input.anyKeyDown)
                        goto outside;

                    if (scrollWheel > 0 || Input.GetKeyDown(nextKey))
                        weaponScroll = int.MaxValue;
                    else if (scrollWheel < 0 || Input.GetKeyDown(previousKey))
                        weaponScroll = int.MinValue;
                    else if (Input.GetKeyDown(KeyCode.Alpha1))
                        weaponScroll = 1;
                    else if (Input.GetKeyDown(KeyCode.Alpha2) && weapons.Length >= 2)
                        weaponScroll = 2;
                    else if (Input.GetKeyDown(KeyCode.Alpha3) && weapons.Length >= 3)
                        weaponScroll = 3;
                    else if (Input.GetKeyDown(KeyCode.Alpha4) && weapons.Length >= 4)
                        weaponScroll = 4;
                    else if (Input.GetKeyDown(KeyCode.Alpha5) && weapons.Length >= 5)
                        weaponScroll = 5;
                    else if (Input.GetKeyDown(KeyCode.Alpha6) && weapons.Length >= 6)
                        weaponScroll = 6;
                    else if (Input.GetKeyDown(KeyCode.Alpha7) && weapons.Length >= 7)
                        weaponScroll = 7;
                    else if (Input.GetKeyDown(KeyCode.Alpha8) && weapons.Length >= 8)
                        weaponScroll = 8;
                    else if (Input.GetKeyDown(KeyCode.Alpha9) && weapons.Length >= 9)
                        weaponScroll = 9;
                    else
                        goto outside;

                    CurrentWeapon.TriggerOutAnimation();

                    outside:;
                }
                else
                    return;
            }

            Weapon weapon = CurrentWeapon;
            weapon.SetAim(Input.GetMouseButton(1));
            if (weapon.PrimaryCanBeHeldDown ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0))
                weapon.TryPrimaryShoot();
            else if (weapon.SecondaryCanBeHeldDown ? Input.GetMouseButton(2) : Input.GetMouseButtonDown(2))
                weapon.TrySecondaryShoot();
            else if (Input.GetKeyDown(reloadKey))
                weapon.TryReload();
            else if (Input.GetKeyDown(hitKey))
                weapon.TryMeleeHit();

            Lantern lantern = CurrentLantern;
            if (lantern is null)
                return;
            if (Input.GetKeyDown(lightKey))
            {
                if (Lantern.ActiveLight == null)
                    lantern.SetOn();
                else
                    lantern.SetOff();
            }
        }

        public void FinalizeTriggerOutAnimation()
        {
            bool hasLight = Lantern.ActiveLight != null;

            CurrentWeapon.gameObject.SetActive(false);
            CurrentLantern?.SetOffImmediately();

            if (!(newWeapons is null))
            {
                weapons = newWeapons;
                newWeapons = null;
                currentWeaponIndex = weaponScroll;

                lanterns = new Lantern[weapons.Length];
                for (int i = 0; i < weapons.Length; i++)
                {
                    Lantern lantern = weapons[i].GetComponentInChildren<Lantern>();
                    if (lantern != null)
                        lanterns[i] = lantern;
                }

                Work();
            }
            else if (weaponScroll == int.MaxValue)
            {
                currentWeaponIndex++;
                if (currentWeaponIndex == weapons.Length)
                    currentWeaponIndex = 0;
                Work();
            }
            else if (weaponScroll == int.MinValue)
            {
                currentWeaponIndex--;
                if (currentWeaponIndex == -1)
                    currentWeaponIndex = weapons.Length - 1;
                Work();
            }
            else if (weaponScroll > 0 && weaponScroll <= 9)
            {
                currentWeaponIndex = weaponScroll - 1; // -1 because arrays are zero based.
                Work();
            }
            else
            {
                Work();
                CurrentWeapon.TriggerPickedUpAnimation();
            }

            weaponScroll = 0;

            while (weaponScroll == 0 && weaponsToUnlock.TryDequeue(out string weaponToUnlock))
                UnlockWeapon(weaponToUnlock);

            void Work()
            {
                CurrentWeapon.gameObject.SetActive(true);
                if (hasLight)
                    CurrentLantern?.SetOnImmediately();
                else
                    CurrentLantern?.SetOffImmediately();
            }
        }

        public AmmunitionType GetAmmunitionType(string name)
        {
            for (int i = 0; i < ammunitions.Length; i++)
            {
                AmmunitionType ammunition = ammunitions[i];
                if (ammunition.Name == name)
                    return ammunition;
            }
            throw new KeyNotFoundException($"Not found ammunition type with name {name}.");
        }

        public void ForceTotalAmmunitionUIUpdate() => CurrentWeapon.ForceTotalAmmunitionUIUpdate();

        public void TrySetAnimationTriggerOnCamera(string triggerName, string metaTriggerName)
            => Try.SetAnimationTrigger(eyeCameraAnimator, triggerName, metaTriggerName, "eye camera");

        public void UnlockWeapon(string weaponName)
        {
            if (weaponScroll != 0)
            {
                weaponsToUnlock.Enqueue(weaponName);
                return;
            }

            Weapon newWeapon = null;
            newWeapons = new Weapon[weapons.Length + 1];
            int j = 0;
            for (int i = 0; i < allWeapons.Length; i++)
            {
                ref WeaponPack weapon = ref allWeapons[i];
                if (weapon.CanUse)
                    newWeapons[j++] = weapon.Weapon;
                else if (weapon.Name == weaponName)
                {
                    weapon.CanUse = true;
                    newWeapon = weapon.Weapon;
                    weaponScroll = j;
                    newWeapons[j++] = newWeapon;
                    Lantern lantern = newWeapon.GetComponentInChildren<Lantern>();
                    newWeapon.Initialize(this);
                    if (lantern != null)
                        lantern.Initialize(this);
                }
            }

            if (j == weapons.Length)
                Debug.LogError($"Weapon with name {weaponName} was not found or it was already unlocked");
            else
                CurrentWeapon.TriggerOutAnimation();
        }

        private void OnValidate()
        {
            if (allWeapons.Select(e => e.Name).HasDuplicates())
                Debug.LogError("Has weapon with duplicated name.");
            if (allWeapons.Select(e => e.Weapon).HasDuplicates())
                Debug.LogError("Has duplicated weapons.");
        }

        [Serializable]
        private struct WeaponPack
        {
            [field: SerializeField, Tooltip("Name of the weapon. Used to unlock it.")]
            public string Name { get; private set; }

            [field: SerializeField, Tooltip("Associated weapon.")]
            public Weapon Weapon { get; private set; }

            [SerializeField, Tooltip("Whenever the player can use it.")]
            public bool CanUse;
        }
    }
}