using UnityEngine;

namespace InfinityRunner
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(RunnerInteractable))]
    public sealed class PowerUpPickup : MonoBehaviour
    {
        public PowerUpDefinition definition;

        private PowerUpSpawnPoint owningSpawnPoint;

        public PowerUpDefinition Definition
        {
            get { return definition; }
        }

        public void Initialize(PowerUpDefinition assignedDefinition, PowerUpSpawnPoint spawnPoint)
        {
            definition = assignedDefinition;
            owningSpawnPoint = spawnPoint;
            EnsureInteractable();
            gameObject.SetActive(true);
        }

        public void Consume()
        {
            if (owningSpawnPoint != null)
            {
                owningSpawnPoint.NotifyPickupConsumed(this);
                owningSpawnPoint = null;
            }

            Destroy(gameObject);
        }

        public void DetachFromSpawnPoint(PowerUpSpawnPoint spawnPoint)
        {
            if (owningSpawnPoint == spawnPoint)
            {
                owningSpawnPoint = null;
            }
        }

        private void Reset()
        {
            EnsureInteractable();
            Collider targetCollider = GetComponent<Collider>();
            targetCollider.isTrigger = true;
        }

        private void Awake()
        {
            EnsureInteractable();
        }

        private void EnsureInteractable()
        {
            RunnerInteractable interactable = GetComponent<RunnerInteractable>();
            if (interactable != null)
            {
                interactable.interactionType = RunnerInteractableType.PowerUp;
            }
        }
    }
}
