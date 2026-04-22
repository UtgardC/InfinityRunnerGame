using System.Collections;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class RunnerCameraController : MonoBehaviour
    {
        private enum CameraMode
        {
            Menu,
            Runner
        }

        public Camera targetCamera;
        public Transform player;

        [Header("Menu")]
        public Transform menuPositionPivot;
        public Transform menuLookTarget;

        [Header("Runner")]
        public Transform runnerPositionPivot;
        public Transform runnerLookTarget;
        public Vector3 runnerOffset = new Vector3(0f, 5.25f, -9f);
        public Vector3 runnerLookOffset = new Vector3(0f, 1.2f, 6f);
        public float runnerFollowSharpness = 12f;

        [Header("Transition")]
        public AnimationCurve positionTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve rotationTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private CameraMode mode = CameraMode.Menu;
        private Coroutine transitionRoutine;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null || player == null || transitionRoutine != null)
            {
                return;
            }

            if (mode == CameraMode.Menu)
            {
                ApplyMenuPose();
                return;
            }

            FollowRunnerPose();
        }

        public void SnapToMenu()
        {
            if (targetCamera == null || player == null)
            {
                return;
            }

            StopTransition();
            mode = CameraMode.Menu;
            ApplyMenuPose();
        }

        public void SnapToRunner()
        {
            if (targetCamera == null || player == null)
            {
                return;
            }

            StopTransition();
            mode = CameraMode.Runner;
            ApplyRunnerPoseImmediate();
        }

        public void TransitionToRunner(float fallbackDuration)
        {
            if (targetCamera == null || player == null)
            {
                return;
            }

            StopTransition();
            transitionRoutine = StartCoroutine(TransitionToRunnerRoutine(ResolveTransitionDuration(fallbackDuration)));
        }

        public float ResolveTransitionDuration(float fallbackDuration)
        {
            float curveDuration = Mathf.Max(
                GetCurveDuration(positionTransitionCurve),
                GetCurveDuration(rotationTransitionCurve));

            if (curveDuration > 0.01f)
            {
                return curveDuration;
            }

            return Mathf.Max(0.01f, fallbackDuration);
        }

        private IEnumerator TransitionToRunnerRoutine(float duration)
        {
            mode = CameraMode.Menu;
            duration = Mathf.Max(0.01f, duration);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float positionT = EvaluateCurve(positionTransitionCurve, elapsed, duration);
                float rotationT = EvaluateCurve(rotationTransitionCurve, elapsed, duration);

                Vector3 position = Vector3.LerpUnclamped(ResolveMenuPosition(), ResolveRunnerPosition(), positionT);
                Quaternion menuRotation = ResolveLookRotation(position, ResolveMenuLookTarget());
                Quaternion runnerRotation = ResolveLookRotation(position, ResolveRunnerLookTarget());
                targetCamera.transform.position = position;
                targetCamera.transform.rotation = Quaternion.SlerpUnclamped(menuRotation, runnerRotation, rotationT);
                yield return null;
            }

            mode = CameraMode.Runner;
            ApplyRunnerPoseImmediate();
            transitionRoutine = null;
        }

        private void StopTransition()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }
        }

        private void ApplyMenuPose()
        {
            targetCamera.transform.position = ResolveMenuPosition();
            LookAt(ResolveMenuLookTarget());
        }

        private void ApplyRunnerPoseImmediate()
        {
            targetCamera.transform.position = ResolveRunnerPosition();
            LookAt(ResolveRunnerLookTarget());
        }

        private void FollowRunnerPose()
        {
            Vector3 desiredPosition = ResolveRunnerPosition();
            targetCamera.transform.position = Vector3.Lerp(
                targetCamera.transform.position,
                desiredPosition,
                1f - Mathf.Exp(-runnerFollowSharpness * Time.deltaTime));
            LookAt(ResolveRunnerLookTarget());
        }

        private Vector3 ResolveMenuPosition()
        {
            if (menuPositionPivot != null)
            {
                return menuPositionPivot.position;
            }

            return targetCamera.transform.position;
        }

        private Vector3 ResolveMenuLookTarget()
        {
            if (menuLookTarget != null)
            {
                return menuLookTarget.position;
            }

            return player.position;
        }

        private Vector3 ResolveRunnerPosition()
        {
            if (runnerPositionPivot != null)
            {
                return runnerPositionPivot.position;
            }

            return player.TransformPoint(runnerOffset);
        }

        private Vector3 ResolveRunnerLookTarget()
        {
            if (runnerLookTarget != null)
            {
                return runnerLookTarget.position;
            }

            return player.TransformPoint(runnerLookOffset);
        }

        private void LookAt(Vector3 target)
        {
            Vector3 direction = target - targetCamera.transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                targetCamera.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        private Quaternion ResolveLookRotation(Vector3 cameraPosition, Vector3 target)
        {
            Vector3 direction = target - cameraPosition;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return targetCamera.transform.rotation;
            }

            return Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private float EvaluateCurve(AnimationCurve curve, float elapsedTime, float fallbackDuration)
        {
            if (curve == null || curve.length == 0)
            {
                return Mathf.Clamp01(elapsedTime / Mathf.Max(0.01f, fallbackDuration));
            }

            Keyframe lastKey = curve[curve.length - 1];
            float clampedTime = Mathf.Clamp(elapsedTime, curve[0].time, lastKey.time);
            return curve.Evaluate(clampedTime);
        }

        private float GetCurveDuration(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0)
            {
                return 0f;
            }

            return Mathf.Max(0f, curve[curve.length - 1].time);
        }
    }
}
