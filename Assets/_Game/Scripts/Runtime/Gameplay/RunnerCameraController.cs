using System.Collections;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class RunnerCameraController : MonoBehaviour
    {
        public Camera targetCamera;
        public Transform player;

        [Header("Menu")]
        public Vector3 menuPosition = new Vector3(5f, 4f, -7f);
        public Vector3 menuLookOffset = new Vector3(0f, 1f, 0f);

        [Header("Runner")]
        public Vector3 runnerOffset = new Vector3(0f, 5.25f, -9f);
        public Vector3 runnerLookOffset = new Vector3(0f, 1.2f, 6f);
        public float runnerFollowSharpness = 12f;

        [Header("Clash")]
        public Vector3 clashSideOffset = new Vector3(7f, 2.7f, -1.6f);
        public Vector3 clashCloseOffset = new Vector3(3.5f, 2.1f, -0.7f);
        public Vector3 clashLookOffset = new Vector3(0f, 1.1f, 0.8f);

        private Coroutine transitionRoutine;
        private bool runnerFollowEnabled;
        private bool clashMode;
        private float clashProgress;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null || player == null || transitionRoutine != null)
            {
                return;
            }

            if (clashMode)
            {
                UpdateClashCamera();
                return;
            }

            if (runnerFollowEnabled)
            {
                Vector3 desiredPosition = player.position + runnerOffset;
                targetCamera.transform.position = Vector3.Lerp(
                    targetCamera.transform.position,
                    desiredPosition,
                    1f - Mathf.Exp(-runnerFollowSharpness * Time.deltaTime));
                LookAt(player.position + runnerLookOffset);
            }
        }

        public void SnapToMenu()
        {
            runnerFollowEnabled = false;
            clashMode = false;

            if (targetCamera == null || player == null)
            {
                return;
            }

            targetCamera.transform.position = player.position + menuPosition;
            LookAt(player.position + menuLookOffset);
        }

        public void TransitionToRunner(float duration)
        {
            StartTransition(player.position + runnerOffset, player.position + runnerLookOffset, duration, true);
        }

        public void BeginClashCamera()
        {
            if (targetCamera == null || player == null)
            {
                return;
            }

            StopTransition();
            runnerFollowEnabled = false;
            clashMode = true;
            clashProgress = 0f;
        }

        public void SetClashProgress(float progress)
        {
            clashProgress = Mathf.Clamp01(progress);
        }

        public void EndClashCamera(float duration)
        {
            clashMode = false;
            StartTransition(player.position + runnerOffset, player.position + runnerLookOffset, duration, true);
        }

        private void UpdateClashCamera()
        {
            Vector3 side = player.position + clashSideOffset;
            Vector3 close = player.position + clashCloseOffset;
            Vector3 desiredPosition = Vector3.Lerp(side, close, clashProgress);
            targetCamera.transform.position = Vector3.Lerp(targetCamera.transform.position, desiredPosition, 1f - Mathf.Exp(-10f * Time.deltaTime));
            LookAt(player.position + clashLookOffset);
        }

        private void StartTransition(Vector3 targetPosition, Vector3 lookTarget, float duration, bool enableRunnerAfter)
        {
            StopTransition();
            transitionRoutine = StartCoroutine(TransitionRoutine(targetPosition, lookTarget, duration, enableRunnerAfter));
        }

        private IEnumerator TransitionRoutine(Vector3 targetPosition, Vector3 lookTarget, float duration, bool enableRunnerAfter)
        {
            runnerFollowEnabled = false;
            clashMode = false;

            Vector3 startPosition = targetCamera.transform.position;
            Quaternion startRotation = targetCamera.transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation((lookTarget - targetPosition).normalized, Vector3.up);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration)));
                targetCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                targetCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            targetCamera.transform.position = targetPosition;
            targetCamera.transform.rotation = targetRotation;
            transitionRoutine = null;
            runnerFollowEnabled = enableRunnerAfter;
        }

        private void StopTransition()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }
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
