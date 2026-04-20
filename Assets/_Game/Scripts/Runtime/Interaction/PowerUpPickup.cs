using UnityEngine;

namespace InfinityRunner
{
    [RequireComponent(typeof(RunnerInteractable))]
    public sealed class PowerUpPickup : MonoBehaviour
    {
        public PowerUpDefinition definition;
        public float rotateSpeed = 120f;

        private void Reset()
        {
            RunnerInteractable interactable = GetComponent<RunnerInteractable>();
            interactable.type = RunnerInteractableType.PowerUp;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        }
    }
}
