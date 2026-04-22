using System.Collections;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class CatapultObstacle : DistanceTriggeredObstacle
    {
        public Transform arm;
        public Transform firePoint;
        public MovingHazardProjectile projectilePrefab;
        public Renderer warningRenderer;
        public float telegraphDuration = 1.1f;
        public float launchAnimationDuration = 0.2f;
        public float projectileSpeed = 42f;
        public float projectileLifetime = 2.2f;

        private Quaternion armStartRotation;
        private Coroutine fireRoutine;

        private void Awake()
        {
            if (arm != null)
            {
                armStartRotation = arm.localRotation;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetObstacle();
        }

        protected override void ResetObstacle()
        {
            if (fireRoutine != null)
            {
                StopCoroutine(fireRoutine);
                fireRoutine = null;
            }

            triggerLeadTime = telegraphDuration + launchAnimationDuration;

            if (arm != null)
            {
                arm.localRotation = armStartRotation;
            }

            if (warningRenderer != null)
            {
                warningRenderer.enabled = false;
            }
        }

        protected override void HandleTriggered(float distanceToPlayer, float currentSpeed)
        {
            fireRoutine = StartCoroutine(FireRoutine());
        }

        private IEnumerator FireRoutine()
        {
            if (warningRenderer != null)
            {
                warningRenderer.enabled = true;
            }

            float elapsed = 0f;
            while (elapsed < telegraphDuration)
            {
                elapsed += Time.deltaTime;
                if (arm != null)
                {
                    arm.localRotation = armStartRotation * Quaternion.Euler(-55f * Mathf.Clamp01(elapsed / telegraphDuration), 0f, 0f);
                }
                yield return null;
            }

            if (projectilePrefab != null)
            {
                Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
                Quaternion spawnRotation = firePoint != null ? firePoint.rotation : Quaternion.identity;
                MovingHazardProjectile projectile = Instantiate(projectilePrefab, spawnPosition, spawnRotation, transform.parent);
                projectile.Launch(Vector3.back, projectileSpeed, projectileLifetime);
            }

            elapsed = 0f;
            while (elapsed < launchAnimationDuration)
            {
                elapsed += Time.deltaTime;
                if (arm != null)
                {
                    float t = Mathf.Clamp01(elapsed / launchAnimationDuration);
                    arm.localRotation = armStartRotation * Quaternion.Euler(Mathf.Lerp(-55f, 20f, t), 0f, 0f);
                }
                yield return null;
            }

            if (warningRenderer != null)
            {
                warningRenderer.enabled = false;
            }

            fireRoutine = null;
        }
    }
}
