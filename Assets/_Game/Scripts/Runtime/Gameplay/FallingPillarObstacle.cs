using UnityEngine;

namespace InfinityRunner
{
    public sealed class FallingPillarObstacle : DistanceTriggeredObstacle
    {
        public Animator pillarAnimator;
        public string idleStateName = "Idle";
        public string triggeredStateName = "Triggered";

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

            pillarAnimator.Rebind();
            pillarAnimator.Update(0f);

            if (!string.IsNullOrWhiteSpace(idleStateName))
            {
                pillarAnimator.Play(idleStateName, 0, 0f);
                pillarAnimator.Update(0f);
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
    }
}
