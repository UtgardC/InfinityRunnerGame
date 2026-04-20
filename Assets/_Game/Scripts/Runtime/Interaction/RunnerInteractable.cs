using UnityEngine;

namespace InfinityRunner
{
    [RequireComponent(typeof(Collider))]
    public sealed class RunnerInteractable : MonoBehaviour
    {
        public RunnerInteractableType type = RunnerInteractableType.Destructible;
        public int scoreOverride;
        public float jumpClearHeight = 1.25f;
        public bool singleUse = true;

        private bool consumed;

        public bool IsConsumed
        {
            get { return consumed; }
        }

        public void ResetInteraction()
        {
            consumed = false;
        }

        public void Consume()
        {
            if (!singleUse)
            {
                return;
            }

            consumed = true;
        }

        private void Reset()
        {
            Collider targetCollider = GetComponent<Collider>();
            targetCollider.isTrigger = true;
        }

        private void OnEnable()
        {
            consumed = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (consumed)
            {
                return;
            }

            PlayerRunnerController player = other.GetComponentInParent<PlayerRunnerController>();
            if (player == null)
            {
                return;
            }

            GameCoordinator coordinator = GameCoordinator.Instance;
            if (coordinator != null)
            {
                coordinator.HandleInteractable(this, player);
            }
        }
    }
}
