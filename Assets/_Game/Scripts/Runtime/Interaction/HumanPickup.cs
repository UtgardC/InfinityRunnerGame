using UnityEngine;

namespace InfinityRunner
{
    public sealed class HumanPickup : MonoBehaviour
    {
        [Min(0f)] public float moveSpeed = 2.4f;
        [Min(0.25f)] public float obstacleCheckDistance = 8f;
        [Min(0f)] public float rayOriginHeight = 0.5f;
        public LayerMask deathLayers;
        public string deathTag = "Death";

        private Vector3 initialLocalPosition;
        private bool hasInitialPosition;

        private void Reset()
        {
            RunnerInteractable interactable = GetComponent<RunnerInteractable>();
            if (interactable != null)
            {
                interactable.interactionType = RunnerInteractableType.Person;
            }
        }

        private void Awake()
        {
            initialLocalPosition = transform.localPosition;
            hasInitialPosition = true;
        }

        private void OnEnable()
        {
            if (!hasInitialPosition)
            {
                initialLocalPosition = transform.localPosition;
                hasInitialPosition = true;
            }

            transform.localPosition = initialLocalPosition;
        }

        private void Update()
        {
            if (ShouldStop())
            {
                return;
            }

            transform.localPosition += Vector3.forward * (moveSpeed * Time.deltaTime);
        }

        private bool ShouldStop()
        {
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight;
            RaycastHit[] hits = Physics.RaycastAll(origin, transform.forward, obstacleCheckDistance, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider collider = hits[i].collider;
                if (collider == null || collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (RunnerCollisionUtility.IsDeathObject(collider, deathLayers, deathTag))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
