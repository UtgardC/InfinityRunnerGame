using UnityEngine;

namespace InfinityRunner
{
    public sealed class RunnerCameraController : MonoBehaviour
    {
        public Camera targetCamera;
        public Transform player;
        public Vector3 runnerOffset = new Vector3(0f, 5.25f, -9f);
        public Vector3 runnerLookOffset = new Vector3(0f, 1.2f, 6f);
        public float runnerFollowSharpness = 12f;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null || player == null)
            {
                return;
            }

            Vector3 desiredPosition = player.position + runnerOffset;
            targetCamera.transform.position = Vector3.Lerp(
                targetCamera.transform.position,
                desiredPosition,
                1f - Mathf.Exp(-runnerFollowSharpness * Time.deltaTime));
            LookAt(player.position + runnerLookOffset);
        }

        public void SnapToRunner()
        {
            if (targetCamera == null || player == null)
            {
                return;
            }

            targetCamera.transform.position = player.position + runnerOffset;
            LookAt(player.position + runnerLookOffset);
        }

        private void LookAt(Vector3 target)
        {
            Vector3 direction = target - targetCamera.transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                targetCamera.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }
    }
}
