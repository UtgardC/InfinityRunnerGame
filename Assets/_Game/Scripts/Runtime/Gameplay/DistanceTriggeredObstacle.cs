using UnityEngine;

namespace InfinityRunner
{
    public abstract class DistanceTriggeredObstacle : MonoBehaviour
    {
        [Header("Timing")]
        [Min(0.05f)] public float triggerLeadTime = 1f;
        [Min(0f)] public float minTriggerDistance = 2f;
        [Min(0f)] public float maxTriggerDistance = 60f;

        [Header("Gizmos")]
        public bool drawActivationGizmo = true;
        [Min(0.01f)] public float gizmoPreviewSpeed = 10f;
        [Min(0.1f)] public float gizmoSphereRadius = 0.35f;
        public Color gizmoColor = new Color(1f, 0.7f, 0.1f, 1f);

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
            float triggerDistance = CalculateTriggerDistance(currentSpeed);
            if (distanceToPlayer > triggerDistance)
            {
                return;
            }

            triggered = true;
            HandleTriggered(distanceToPlayer, currentSpeed);
        }

        protected float CalculateTriggerDistance(float speed)
        {
            return Mathf.Clamp(speed * triggerLeadTime, minTriggerDistance, maxTriggerDistance);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawActivationGizmo)
            {
                return;
            }

            float speed = gizmoPreviewSpeed;
            GameCoordinator coordinator = GameCoordinator.Instance;
            if (Application.isPlaying && coordinator != null)
            {
                speed = Mathf.Max(speed, coordinator.WorldSpeed);
            }

            float triggerDistance = CalculateTriggerDistance(Mathf.Max(0.01f, speed));
            Vector3 triggerPoint = transform.position - transform.forward * triggerDistance;

            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(transform.position, triggerPoint);
            Gizmos.DrawWireSphere(triggerPoint, gizmoSphereRadius);
            Gizmos.DrawWireCube(triggerPoint, new Vector3(gizmoSphereRadius * 4f, gizmoSphereRadius * 2f, gizmoSphereRadius * 0.5f));
        }

        protected abstract void ResetObstacle();
        protected abstract void HandleTriggered(float distanceToPlayer, float currentSpeed);
    }
}
