using UnityEngine;

namespace InfinityRunner
{
    public sealed class HumanPickup : MonoBehaviour
    {
        [Header("Movement")]
        [Min(0f)] public float moveSpeed = 2.4f;
        [Min(0.25f)] public float obstacleCheckDistance = 8f;
        [Min(0f)] public float rayOriginHeight = 0.5f;
        public LayerMask deathLayers;
        public string deathTag = "Death";

        [Header("Visual Variety")]
        public Renderer[] materialTargets;
        public Material[] materialVariants;

        [Header("Animation")]
        public Animator animator;
        public string[] runStateNames;
        public string blockedStateName;
        [Min(0f)] public float animationCrossFadeDuration = 0.1f;

        private Vector3 initialLocalPosition;
        private string selectedRunStateName;
        private bool hasInitialPosition;
        private bool stoppedByObstacle;

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
            stoppedByObstacle = false;
            ApplyRandomMaterial();
            SelectRandomRunAnimation();
            PlayCurrentAnimationState(true);
        }

        private void Update()
        {
            bool shouldStop = ShouldStop();
            if (shouldStop != stoppedByObstacle)
            {
                stoppedByObstacle = shouldStop;
                PlayCurrentAnimationState(false);
            }

            if (shouldStop)
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

        private void ApplyRandomMaterial()
        {
            if (materialTargets == null || materialTargets.Length == 0 || materialVariants == null || materialVariants.Length == 0)
            {
                return;
            }

            Material selectedMaterial = materialVariants[Random.Range(0, materialVariants.Length)];
            if (selectedMaterial == null)
            {
                return;
            }

            for (int i = 0; i < materialTargets.Length; i++)
            {
                Renderer rendererTarget = materialTargets[i];
                if (rendererTarget == null)
                {
                    continue;
                }

                Material[] sharedMaterials = rendererTarget.sharedMaterials;
                if (sharedMaterials == null || sharedMaterials.Length == 0)
                {
                    rendererTarget.sharedMaterial = selectedMaterial;
                    continue;
                }

                for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
                {
                    sharedMaterials[materialIndex] = selectedMaterial;
                }

                rendererTarget.sharedMaterials = sharedMaterials;
            }
        }

        private void SelectRandomRunAnimation()
        {
            selectedRunStateName = string.Empty;
            if (runStateNames == null || runStateNames.Length == 0)
            {
                return;
            }

            int validCount = 0;
            for (int i = 0; i < runStateNames.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(runStateNames[i]))
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return;
            }

            int selectedIndex = Random.Range(0, validCount);
            int currentIndex = 0;
            for (int i = 0; i < runStateNames.Length; i++)
            {
                string stateName = runStateNames[i];
                if (string.IsNullOrWhiteSpace(stateName))
                {
                    continue;
                }

                if (currentIndex == selectedIndex)
                {
                    selectedRunStateName = stateName;
                    return;
                }

                currentIndex++;
            }
        }

        private void PlayCurrentAnimationState(bool instant)
        {
            if (animator == null)
            {
                return;
            }

            string stateName = stoppedByObstacle ? blockedStateName : selectedRunStateName;
            if (string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            if (instant || animationCrossFadeDuration <= 0f)
            {
                animator.Play(stateName, 0, 0f);
                return;
            }

            animator.CrossFadeInFixedTime(stateName, animationCrossFadeDuration);
        }
    }
}
