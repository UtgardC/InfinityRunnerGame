using UnityEngine;

namespace InfinityRunner
{
    public sealed class FallingPillarObstacle : DistanceTriggeredObstacle
    {
        public Animator pillarAnimator;
        public Transform animatedRoot;
        public string idleStateName = "Idle";
        public string triggeredStateName = "Triggered";

        private Vector3 initialLocalPosition;
        private Quaternion initialLocalRotation;
        private Vector3 initialLocalScale;
        private bool hasInitialPose;

        private void Awake()
        {
            CacheInitialPose();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetObstacle();
        }

        protected override void ResetObstacle()
        {
            if (pillarAnimator == null)
            {
                return;
            }

            CacheInitialPose();
            RestoreAnimatedRootPose();
            pillarAnimator.Rebind();
            pillarAnimator.Update(0f);
            RestoreAnimatedRootPose();

            if (!string.IsNullOrWhiteSpace(idleStateName))
            {
                pillarAnimator.Play(idleStateName, 0, 0f);
                pillarAnimator.Update(0f);
                RestoreAnimatedRootPose();
            }
        }

        protected override void HandleTriggered(float distanceToPlayer, float currentSpeed)
        {
            if (pillarAnimator == null || string.IsNullOrWhiteSpace(triggeredStateName))
            {
                return;
            }

            pillarAnimator.Play(triggeredStateName, 0, 0f);
        }

        private void CacheInitialPose()
        {
            Transform target = ResolveAnimatedRoot();
            if (target == null || hasInitialPose)
            {
                return;
            }

            initialLocalPosition = target.localPosition;
            initialLocalRotation = target.localRotation;
            initialLocalScale = target.localScale;
            hasInitialPose = true;
        }

        private void RestoreAnimatedRootPose()
        {
            Transform target = ResolveAnimatedRoot();
            if (target == null || !hasInitialPose)
            {
                return;
            }

            target.localPosition = initialLocalPosition;
            target.localRotation = initialLocalRotation;
            target.localScale = initialLocalScale;
        }

        private Transform ResolveAnimatedRoot()
        {
            if (animatedRoot != null)
            {
                return animatedRoot;
            }

            return pillarAnimator != null ? pillarAnimator.transform : null;
        }
    }
}
