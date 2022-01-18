using Game.Player;

using UnityEngine;

namespace Game.Effects
{
    [RequireComponent(typeof(Renderer))]
    public sealed class EmissionByDistance : MonoBehaviour
    {
        private Material material;

        private void Awake() => material = GetComponent<Renderer>().material;

        private void Update() => material.SetVector("_PlayerPosition", PlayerBody.Instance.transform.position);
    }
}