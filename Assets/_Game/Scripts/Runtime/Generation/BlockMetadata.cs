using System.Collections.Generic;
using UnityEngine;

namespace InfinityRunner
{
    public sealed class BlockMetadata : MonoBehaviour
    {
        [Min(5f)] public float length = 30f;
        public BlockKind kind = BlockKind.Safe;
        public Transform startAnchor;
        public Transform endAnchor;
        public Transform[] cameraAnchors;

        private readonly List<GameObject> spawnedPooledObjects = new List<GameObject>();

        public IReadOnlyList<GameObject> SpawnedPooledObjects
        {
            get { return spawnedPooledObjects; }
        }

        public void RegisterSpawnedObject(GameObject spawnedObject)
        {
            if (spawnedObject != null && !spawnedPooledObjects.Contains(spawnedObject))
            {
                spawnedPooledObjects.Add(spawnedObject);
            }
        }

        public void ClearSpawnedObjects()
        {
            spawnedPooledObjects.Clear();
        }

        public PowerUpSpawnPoint[] GetPowerUpSpawnPoints()
        {
            return GetComponentsInChildren<PowerUpSpawnPoint>(true);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 center = transform.position + transform.forward * (length * 0.5f);
            Gizmos.DrawWireCube(center, new Vector3(10f, 0.25f, length));
        }
    }
}
