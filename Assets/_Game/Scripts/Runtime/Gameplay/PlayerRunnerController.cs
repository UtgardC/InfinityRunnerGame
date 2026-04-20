using System.Collections;
using UnityEngine;

namespace InfinityRunner
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerRunnerController : MonoBehaviour
    {
        public Transform visualRoot;
        public RunnerConfig config;

        private Lane currentLane = Lane.Center;
        private Lane targetLane = Lane.Center;
        private float laneChangeTime;
        private float laneStartX;
        private float verticalVelocity;
        private bool grounded = true;
        private bool controlsLocked;
        private bool hazardInvulnerable;
        private Coroutine rampFlightRoutine;

        public Lane CurrentLane
        {
            get { return targetLane; }
        }

        public bool IsGrounded
        {
            get { return grounded; }
        }

        public bool IsHazardInvulnerable
        {
            get { return hazardInvulnerable; }
        }

        public float HeightAboveGround
        {
            get
            {
                if (config == null)
                {
                    return transform.position.y;
                }

                return transform.position.y - config.groundHeight;
            }
        }

        private void Awake()
        {
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

        public void ResetPlayer()
        {
            StopRampFlight();
            currentLane = Lane.Center;
            targetLane = Lane.Center;
            laneChangeTime = 1f;
            verticalVelocity = 0f;
            grounded = true;
            controlsLocked = false;
            hazardInvulnerable = false;
            transform.position = new Vector3(config.LaneToX(Lane.Center), config.groundHeight, 0f);
            transform.rotation = Quaternion.identity;
        }

        public void SetControlsLocked(bool locked)
        {
            controlsLocked = locked;
        }

        public void SetHazardInvulnerable(bool invulnerable)
        {
            hazardInvulnerable = invulnerable;
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
            if (controlsLocked || !grounded || rampFlightRoutine != null)
            {
                return;
            }

            grounded = false;
            verticalVelocity = config.jumpVelocity;
        }

        public bool ClearsHeight(float clearHeight)
        {
            return HeightAboveGround >= clearHeight;
        }

        public void BeginRampFlight(float duration, float height, float invulnerabilityPadding)
        {
            StopRampFlight();
            rampFlightRoutine = StartCoroutine(RampFlightRoutine(duration, height, invulnerabilityPadding));
        }

        private void StopRampFlight()
        {
            if (rampFlightRoutine != null)
            {
                StopCoroutine(rampFlightRoutine);
                rampFlightRoutine = null;
            }
        }

        private IEnumerator RampFlightRoutine(float duration, float height, float invulnerabilityPadding)
        {
            controlsLocked = true;
            hazardInvulnerable = true;
            grounded = false;
            verticalVelocity = 0f;

            float elapsed = 0f;
            float baseY = config.groundHeight;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float y = baseY + Mathf.Sin(t * Mathf.PI) * height;
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
                yield return null;
            }

            transform.position = new Vector3(transform.position.x, baseY, transform.position.z);
            grounded = true;
            controlsLocked = false;

            if (invulnerabilityPadding > 0f)
            {
                yield return new WaitForSeconds(invulnerabilityPadding);
            }

            hazardInvulnerable = false;
            rampFlightRoutine = null;
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
            if (grounded || rampFlightRoutine != null)
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
            if (visualRoot == null || GameCoordinator.Instance == null)
            {
                return;
            }

            float speed = GameCoordinator.Instance.CurrentWorldSpeed;
            visualRoot.Rotate(Vector3.right, speed * 75f * Time.deltaTime, Space.Self);
        }
    }
}
