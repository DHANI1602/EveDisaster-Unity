using Game.Utility;

using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract partial class MovableEnemy : Enemy, IPushable
    {
        [SerializeField, Tooltip("Navigation information.")]
        protected NavMeshAgentHelper NavAgent;

        [SerializeField, Range(0, 1), Tooltip("Speed multiplier during hurt animation.")]
        private float hurtingSpeedMultiplied = 1;

        protected override void Awake()
        {
            base.Awake();
            NavAgent.Initialize(this);
        }

        public void TakeForce(Vector3 force) => NavAgent.ApplyForce(force);

        protected override void GoToIdleState()
        {
            base.GoToIdleState();
            NavAgent.Stop();
        }

        protected void LookAtPlayer() => NavAgent.LookAt(LastPlayerPosition);

        protected abstract float GetStateSpeedMultiplier();

        protected override void OnTakeDamage(float amount, bool isOnWeakspot)
        {
            base.OnTakeDamage(amount, isOnWeakspot);
            NavAgent.SetSpeedMultiplier(hurtingSpeedMultiplied * GetStateSpeedMultiplier());
        }

        protected override void FromHurt()
        {
            base.FromHurt();
            NavAgent.SetSpeedMultiplier(GetStateSpeedMultiplier());
        }

        protected override void OnDeath(bool isOnWeakspot)
        {
            base.OnDeath(isOnWeakspot);
            NavAgent.Stop();
            NavAgent.Disable();
        }

        protected override void ApplyDeath()
        {
            base.ApplyDeath();
            NavAgent.Disable();
        }

        protected override Vector3 OnDeathVelocity() => NavAgent.Velocity;

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            NavAgent.TryDrawPathInGizmos();
        }
#endif
    }
}