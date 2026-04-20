using System.Collections;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class CatapultObstacle : MonoBehaviour
    {
        public Transform arm;
        public Transform firePoint;
        public RunnerInteractable projectilePrefab;
        public Renderer warningRenderer;
        public float activationDistance = 55f;
        public float telegraphDuration = 1.1f;
        public float projectileSpeed = 42f;
        public float projectileLifetime = 2.2f;

        private bool fired;
        private Quaternion armStartRotation;

        private void Awake()
        {
            if (arm != null)
            {
                armStartRotation = arm.localRotation;
            }
        }

        private void OnEnable()
        {
            fired = false;
            if (arm != null)
            {
                arm.localRotation = armStartRotation;
            }

            if (warningRenderer != null)
            {
                warningRenderer.enabled = false;
            }
        }

        private void Update()
        {
            if (fired || GameCoordinator.Instance == null || GameCoordinator.Instance.Player == null)
            {
                return;
            }

            float zDistance = transform.position.z - GameCoordinator.Instance.Player.transform.position.z;
            if (zDistance <= activationDistance && zDistance > 0f)
            {
                fired = true;
                StartCoroutine(FireRoutine());
            }
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
                RunnerInteractable projectile = Instantiate(projectilePrefab, firePoint != null ? firePoint.position : transform.position, Quaternion.identity, transform.parent);
                StartCoroutine(ProjectileRoutine(projectile.transform));
            }
        }

        private IEnumerator ProjectileRoutine(Transform projectile)
        {
            float elapsed = 0f;
            while (projectile != null && elapsed < projectileLifetime)
            {
                elapsed += Time.deltaTime;
                projectile.position += Vector3.back * projectileSpeed * Time.deltaTime;
                yield return null;
            }

            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }
    }
}
