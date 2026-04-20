using System.Collections;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class DynamicCartObstacle : MonoBehaviour
    {
        public Transform movingRoot;
        public Renderer telegraphRenderer;
        public float activationDistance = 38f;
        public float telegraphDuration = 0.75f;
        public float moveDuration = 0.35f;

        private bool triggered;
        private Color originalColor;
        private Vector3 initialLocalPosition;

        private void Awake()
        {
            if (movingRoot == null)
            {
                movingRoot = transform;
            }

            if (telegraphRenderer != null)
            {
                originalColor = telegraphRenderer.sharedMaterial.color;
            }

            initialLocalPosition = movingRoot.localPosition;
        }

        private void OnEnable()
        {
            triggered = false;
            if (movingRoot != null)
            {
                movingRoot.localPosition = initialLocalPosition;
            }

            if (telegraphRenderer != null)
            {
                telegraphRenderer.material.color = originalColor;
            }
        }

        private void Update()
        {
            if (triggered || GameCoordinator.Instance == null || GameCoordinator.Instance.Player == null)
            {
                return;
            }

            float zDistance = transform.position.z - GameCoordinator.Instance.Player.transform.position.z;
            if (zDistance <= activationDistance && zDistance > 0f)
            {
                triggered = true;
                StartCoroutine(AttackRoutine());
            }
        }

        private IEnumerator AttackRoutine()
        {
            if (telegraphRenderer != null)
            {
                telegraphRenderer.material.color = Color.red;
            }

            yield return new WaitForSeconds(telegraphDuration);

            RunnerConfig config = GameCoordinator.Instance.Config;
            Lane targetLane = GameCoordinator.Instance.Player.CurrentLane;
            float startX = movingRoot.localPosition.x;
            float targetX = config != null ? config.LaneToX(targetLane) : (int)targetLane * 3f;

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
        }
    }
}
