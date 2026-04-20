using System.Collections;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class FallingPillarObstacle : MonoBehaviour
    {
        public Transform pillarVisual;
        public Collider verticalHazard;
        public Collider fallenHazard;
        public float activationDistance = 42f;
        public float telegraphDuration = 0.85f;
        public float fallDuration = 0.45f;
        public Vector3 fallenEulerAngles = new Vector3(0f, 0f, 90f);

        private bool triggered;
        private Quaternion startRotation;

        private void Awake()
        {
            if (pillarVisual == null)
            {
                pillarVisual = transform;
            }

            startRotation = pillarVisual.localRotation;
        }

        private void OnEnable()
        {
            triggered = false;
            if (pillarVisual != null)
            {
                pillarVisual.localRotation = startRotation;
            }

            if (verticalHazard != null)
            {
                verticalHazard.enabled = true;
            }

            if (fallenHazard != null)
            {
                fallenHazard.enabled = false;
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
                StartCoroutine(FallRoutine());
            }
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

            if (verticalHazard != null)
            {
                verticalHazard.enabled = false;
            }

            if (fallenHazard != null)
            {
                fallenHazard.enabled = true;
            }
        }
    }
}
