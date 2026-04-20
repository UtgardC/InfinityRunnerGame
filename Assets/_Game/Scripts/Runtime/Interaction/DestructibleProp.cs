using UnityEngine;

namespace InfinityRunner
{
    public sealed class DestructibleProp : MonoBehaviour
    {
        public DestructionMode mode = DestructionMode.InstantVfx;
        public GameObject prebakedBreakPrefab;
        public ParticleSystem breakVfx;
        public AudioSource breakAudio;

        private Renderer[] renderers;
        private Collider[] colliders;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            CacheComponents();
            SetVisible(true);
        }

        public void Break()
        {
            if (breakVfx != null)
            {
                breakVfx.Play(true);
            }

            if (breakAudio != null)
            {
                breakAudio.Play();
            }

            if (mode == DestructionMode.PrebakedBreak && prebakedBreakPrefab != null)
            {
                Instantiate(prebakedBreakPrefab, transform.position, transform.rotation, transform.parent);
            }

            SetVisible(false);
        }

        private void CacheComponents()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }

            if (colliders == null || colliders.Length == 0)
            {
                colliders = GetComponentsInChildren<Collider>(true);
            }
        }

        private void SetVisible(bool visible)
        {
            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].enabled = visible;
                    }
                }
            }

            if (colliders != null)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                    {
                        colliders[i].enabled = visible;
                    }
                }
            }
        }
    }
}
