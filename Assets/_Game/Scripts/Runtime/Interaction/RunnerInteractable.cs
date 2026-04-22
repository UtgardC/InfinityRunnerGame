using UnityEngine;

namespace InfinityRunner
{
    [RequireComponent(typeof(Collider))]
    public sealed class RunnerInteractable : MonoBehaviour
    {
        public RunnerInteractableType interactionType = RunnerInteractableType.Death;
        public int score;
        public bool singleUse = true;

        private bool consumed;

        public bool IsConsumed
        {
            get { return consumed; }
        }

        public int ResolveScore(int defaultScore)
        {
            return score != 0 ? score : defaultScore;
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
    }
}
