using System.Collections;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class DynamicCartObstacle : DistanceTriggeredObstacle
    {
        public Transform movingRoot;
        public Renderer telegraphRenderer;
        public float telegraphDuration = 0.75f;
        public float moveDuration = 0.35f;
        public float laneOffset;

        private Color originalColor;
        private Vector3 initialLocalPosition;
        private Coroutine attackRoutine;

        private void Awake()
        {
            if (movingRoot == null)
            {
                movingRoot = transform;
            }

            if (telegraphRenderer != null && telegraphRenderer.sharedMaterial != null)
            {
                originalColor = telegraphRenderer.sharedMaterial.color;
            }

            initialLocalPosition = movingRoot.localPosition;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetObstacle();
        }

        protected override void ResetObstacle()
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            triggerLeadTime = telegraphDuration + moveDuration;

            if (movingRoot != null)
            {
                movingRoot.localPosition = initialLocalPosition;
            }

            if (telegraphRenderer != null && telegraphRenderer.material != null)
            {
                telegraphRenderer.material.color = originalColor;
            }
        }

        protected override void HandleTriggered(float distanceToPlayer, float currentSpeed)
        {
            attackRoutine = StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            if (telegraphRenderer != null && telegraphRenderer.material != null)
            {
                telegraphRenderer.material.color = Color.red;
            }

            yield return new WaitForSeconds(telegraphDuration);

            GameCoordinator coordinator = GameCoordinator.Instance;
            RunnerConfig runnerConfig = coordinator != null ? coordinator.Config : null;
            Lane targetLane = coordinator != null && coordinator.Player != null ? coordinator.Player.CurrentLane : Lane.Center;
            float startX = movingRoot.localPosition.x;
            float targetX = (runnerConfig != null ? runnerConfig.LaneToX(targetLane) : 0f) + laneOffset;

            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));
                Vector3 local = movingRoot.localPosition;
                local.x = Mathf.Lerp(startX, targetX, t);
                movingRoot.localPosition = local;
                yield return null;
            }

            attackRoutine = null;
        }
    }
}
