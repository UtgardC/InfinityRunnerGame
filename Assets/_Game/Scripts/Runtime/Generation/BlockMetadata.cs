using UnityEngine;

namespace InfinityRunner
{
    public sealed class BlockMetadata : MonoBehaviour
    {
        [Min(1f)] public float length = 30f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 center = transform.position + transform.forward * (length * 0.5f);
            Gizmos.DrawWireCube(center, new Vector3(10f, 0.25f, length));
        }
    }
}
