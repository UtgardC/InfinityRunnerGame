using UnityEngine;

namespace InfinityRunner
{
    public abstract class DistanceTriggeredObstacle : MonoBehaviour
    {
        [Header("Timing")]
        [Min(0.05f)] public float triggerLeadTime = 1f;
        [Min(0f)] public float minTriggerDistance = 2f;
        [Min(0f)] public float maxTriggerDistance = 60f;

        private bool triggered;

        protected virtual void OnEnable()
        {
            triggered = false;
        }

        protected virtual void Update()
        {
            if (triggered)
            {
                return;
            }

            GameCoordinator coordinator = GameCoordinator.Instance;
            if (coordinator == null || !coordinator.IsRunning || coordinator.Player == null)
            {
                return;
            }

            float distanceToPlayer = transform.position.z - coordinator.Player.transform.position.z;
            if (distanceToPlayer <= 0f)
            {
                return;
            }

            float currentSpeed = Mathf.Max(0.01f, coordinator.WorldSpeed);
            float triggerDistance = Mathf.Clamp(currentSpeed * triggerLeadTime, minTriggerDistance, maxTriggerDistance);
            if (distanceToPlayer > triggerDistance)
            {
                return;
            }

            triggered = true;
            HandleTriggered(distanceToPlayer, currentSpeed);
        }

        protected abstract void ResetObstacle();
        protected abstract void HandleTriggered(float distanceToPlayer, float currentSpeed);
    }
}
