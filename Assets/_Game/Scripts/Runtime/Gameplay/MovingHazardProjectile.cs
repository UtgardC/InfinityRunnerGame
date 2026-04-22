using UnityEngine;

namespace InfinityRunner
{
    public sealed class MovingHazardProjectile : MonoBehaviour
    {
        private Vector3 direction = Vector3.back;
        private float speed;
        private float lifetime;
        private float elapsed;

        public void Launch(Vector3 worldDirection, float moveSpeed, float maxLifetime)
        {
            direction = worldDirection.normalized;
            speed = moveSpeed;
            lifetime = maxLifetime;
            elapsed = 0f;
        }

        private void Update()
        {
            transform.position += direction * (speed * Time.deltaTime);
            elapsed += Time.deltaTime;

            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
