using System.Collections;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class FallingPillarObstacle : DistanceTriggeredObstacle
    {
        public Transform pillarVisual;
        public Collider uprightDeathCollider;
        public Collider fallenDeathCollider;
        public float telegraphDuration = 0.85f;
        public float fallDuration = 0.45f;
        public Vector3 fallenEulerAngles = new Vector3(0f, 0f, 90f);

        private Quaternion startRotation;
        private Coroutine fallRoutine;

        private void Awake()
        {
            if (pillarVisual == null)
            {
                pillarVisual = transform;
            }

            startRotation = pillarVisual.localRotation;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetObstacle();
        }

        protected override void ResetObstacle()
        {
            if (fallRoutine != null)
            {
                StopCoroutine(fallRoutine);
                fallRoutine = null;
            }

            triggerLeadTime = telegraphDuration + fallDuration;

            if (pillarVisual != null)
            {
                pillarVisual.localRotation = startRotation;
            }

            if (uprightDeathCollider != null)
            {
                uprightDeathCollider.enabled = true;
            }

            if (fallenDeathCollider != null)
            {
                fallenDeathCollider.enabled = false;
            }
        }

        protected override void HandleTriggered(float distanceToPlayer, float currentSpeed)
        {
            fallRoutine = StartCoroutine(FallRoutine());
        }

        private IEnumerator FallRoutine()
        {
            yield return new WaitForSeconds(telegraphDuration);

            Quaternion endRotation = Quaternion.Euler(fallenEulerAngles);
            float elapsed = 0f;
            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fallDuration));
                pillarVisual.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                yield return null;
            }

            if (uprightDeathCollider != null)
            {
                uprightDeathCollider.enabled = false;
            }

            if (fallenDeathCollider != null)
            {
                fallenDeathCollider.enabled = true;
            }

            fallRoutine = null;
        }
    }
}
