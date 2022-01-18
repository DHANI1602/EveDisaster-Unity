using Game.Utility;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Player.Weapons
{
    public sealed class KnockbackModifier : WeaponModifier
    {
        [SerializeField, Min(0), Tooltip("Amount of force applied as knockback on impact.")]
        private float force;

        private Action<IPushable, Vector3> action;

        public override void Modify(List<ShootInformation> information)
        {
            if (action is null)
                action = (pushable, direction) =>
                {
                    direction.y = 0;
                    pushable.TakeForce(direction * force);
                };

            for (int i = 0; i < information.Count; i++)
            {
                ShootInformation info = information[i];
                info.OnShootPushable += action;
                information[i] = info;
            }
        }
    }
}