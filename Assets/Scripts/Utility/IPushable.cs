using UnityEngine;

namespace Game.Utility
{
    public interface IPushable
    {
        void TakeForce(Vector3 force);
    }
}