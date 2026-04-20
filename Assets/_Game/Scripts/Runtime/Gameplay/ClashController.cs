using UnityEngine;
using UnityEngine.Playables;

namespace InfinityRunner
{
    public sealed class ClashController : MonoBehaviour
    {
        public PlayableDirector optionalTimeline;
        public Transform rockSpinProxy;
        public Transform wallRoot;
        public ParticleSystem breakVfx;
        public AudioSource breakAudio;
        public float maxSpinSpeed = 900f;
        public float wallShakeAmount = 0.22f;

        private Vector3 wallStartPosition;
        private bool active;

        private void Awake()
        {
            if (wallRoot != null)
            {
                wallStartPosition = wallRoot.localPosition;
            }
        }

        private void OnEnable()
        {
            active = false;
            if (wallRoot != null)
            {
                wallRoot.gameObject.SetActive(true);
                wallRoot.localPosition = wallStartPosition;
            }
        }

        private void Update()
        {
            if (!active || rockSpinProxy == null)
            {
                return;
            }

            float progress = 0f;
            GameCoordinator coordinator = GameCoordinator.Instance;
            if (coordinator != null && coordinator.Config != null)
            {
                // The coordinator calls SetProgress too; this keeps visual spin alive between taps.
                progress = 0.25f;
            }

            rockSpinProxy.Rotate(Vector3.right, maxSpinSpeed * progress * Time.deltaTime, Space.Self);
        }

        public void BeginClash()
        {
            active = true;
            if (optionalTimeline != null && optionalTimeline.playableAsset != null)
            {
                optionalTimeline.time = 0.0;
                optionalTimeline.Play();
            }
        }

        public void SetProgress(float progress)
        {
            if (rockSpinProxy != null)
            {
                rockSpinProxy.Rotate(Vector3.right, Mathf.Lerp(120f, maxSpinSpeed, progress) * Time.deltaTime, Space.Self);
            }

            if (wallRoot != null)
            {
                float shake = Mathf.Sin(Time.time * Mathf.Lerp(20f, 55f, progress)) * wallShakeAmount * progress;
                wallRoot.localPosition = wallStartPosition + new Vector3(shake, 0f, 0f);
            }
        }

        public void CompleteClash()
        {
            active = false;

            if (optionalTimeline != null && optionalTimeline.playableAsset != null)
            {
                optionalTimeline.Stop();
            }

            if (breakVfx != null)
            {
                breakVfx.Play(true);
            }

            if (breakAudio != null)
            {
                breakAudio.Play();
            }

            if (wallRoot != null)
            {
                wallRoot.gameObject.SetActive(false);
            }
        }
    }
}
