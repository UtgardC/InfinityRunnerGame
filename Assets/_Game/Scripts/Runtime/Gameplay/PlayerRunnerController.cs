using UnityEngine;

namespace InfinityRunner
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerRunnerController : MonoBehaviour
    {
        [Header("Visual")]
        public Transform visualRoot;
        public RunnerConfig config;

        [Header("Power Up Visual")]
        public Renderer invincibilityRenderer;
        public Material normalVisualMaterial;
        public Material invincibleVisualMaterial;

        private GameCoordinator coordinator;
        private Vector3 startingPosition;
        private Quaternion startingRotation;
        private Lane currentLane = Lane.Center;
        private Lane targetLane = Lane.Center;
        private float laneChangeTime;
        private float laneStartX;
        private float verticalVelocity;
        private bool grounded = true;
        private bool controlsLocked;

        public Lane CurrentLane
        {
            get { return targetLane; }
        }

        public bool IsGrounded
        {
            get { return grounded; }
        }

        private void Awake()
        {
            startingPosition = transform.position;
            startingRotation = transform.rotation;

            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            Collider playerCollider = GetComponent<Collider>();
            playerCollider.isTrigger = false;
        }

        private void Update()
        {
            if (config == null)
            {
                return;
            }

            UpdateLanePosition();
            UpdateJump();
            RotateVisual();
        }

        public void SetCoordinator(GameCoordinator owner)
        {
            coordinator = owner;
        }

        public void ResetPlayer()
        {
            currentLane = Lane.Center;
            targetLane = Lane.Center;
            laneChangeTime = 1f;
            verticalVelocity = 0f;
            grounded = true;
            controlsLocked = false;
            transform.position = new Vector3(config.LaneToX(Lane.Center), config.groundHeight, startingPosition.z);
            transform.rotation = startingRotation;
            SetInvincibilityVisual(false);
        }

        public void SetControlsLocked(bool locked)
        {
            controlsLocked = locked;
        }

        public void ChangeLane(int direction)
        {
            if (controlsLocked || direction == 0)
            {
                return;
            }

            int nextLane = Mathf.Clamp((int)targetLane + direction, -1, 1);
            if (nextLane == (int)targetLane)
            {
                return;
            }

            currentLane = targetLane;
            targetLane = (Lane)nextLane;
            laneStartX = transform.position.x;
            laneChangeTime = 0f;
        }

        public void Jump()
        {
            if (controlsLocked || !grounded)
            {
                return;
            }

            grounded = false;
            verticalVelocity = config.jumpVelocity;
        }

        public void FastFall()
        {
            if (controlsLocked || grounded || config == null)
            {
                return;
            }

            verticalVelocity = Mathf.Min(verticalVelocity, -config.fastFallVelocity);
        }

        public void SetInvincibilityVisual(bool active)
        {
            if (invincibilityRenderer == null)
            {
                return;
            }

            Material targetMaterial = active ? invincibleVisualMaterial : normalVisualMaterial;
            if (targetMaterial == null)
            {
                return;
            }

            Material[] sharedMaterials = invincibilityRenderer.sharedMaterials;
            if (sharedMaterials == null || sharedMaterials.Length == 0)
            {
                invincibilityRenderer.sharedMaterial = targetMaterial;
                return;
            }

            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                sharedMaterials[i] = targetMaterial;
            }

            invincibilityRenderer.sharedMaterials = sharedMaterials;
        }

        private void UpdateLanePosition()
        {
            float targetX = config.LaneToX(targetLane);
            float x = transform.position.x;

            if (laneChangeTime < 1f)
            {
                laneChangeTime += Time.deltaTime / Mathf.Max(0.01f, config.laneChangeDuration);
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(laneChangeTime));
                x = Mathf.Lerp(laneStartX, targetX, eased);
            }
            else
            {
                x = Mathf.MoveTowards(x, targetX, Time.deltaTime * config.laneSpacing * 10f);
            }

            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        private void UpdateJump()
        {
            if (grounded)
            {
                return;
            }

            verticalVelocity -= config.gravity * Time.deltaTime;
            float y = transform.position.y + verticalVelocity * Time.deltaTime;

            if (y <= config.groundHeight)
            {
                y = config.groundHeight;
                verticalVelocity = 0f;
                grounded = true;
            }

            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }

        private void RotateVisual()
        {
            if (visualRoot == null || coordinator == null || config == null)
            {
                return;
            }

            visualRoot.Rotate(Vector3.right, coordinator.WorldSpeed * config.visualRollSpeed * Time.deltaTime, Space.Self);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (coordinator != null)
            {
                coordinator.HandlePlayerContact(other);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (coordinator != null)
            {
                coordinator.HandlePlayerContact(collision.collider);
            }
        }
    }
}
