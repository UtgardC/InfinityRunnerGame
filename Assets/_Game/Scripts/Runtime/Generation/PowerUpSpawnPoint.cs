using UnityEngine;

namespace InfinityRunner
{
    public sealed class PowerUpSpawnPoint : MonoBehaviour
    {
        public Transform spawnAnchor;
        public PowerUpTypeMask allowedTypes = PowerUpTypeMask.All;

        private PowerUpPickup spawnedPickup;

        public bool HasSpawnedPickup
        {
            get { return spawnedPickup != null; }
        }

        public bool CanSpawn(PowerUpDefinition definition)
        {
            if (definition == null || definition.pickupPrefab == null || HasSpawnedPickup)
            {
                return false;
            }

            return (allowedTypes & ToMask(definition.type)) != 0;
        }

        public PowerUpPickup Spawn(PowerUpDefinition definition)
        {
            if (!CanSpawn(definition))
            {
                return null;
            }

            Transform parent = spawnAnchor != null ? spawnAnchor : transform;
            PowerUpPickup pickupInstance = Instantiate(definition.pickupPrefab, parent);
            pickupInstance.transform.localPosition = Vector3.zero;
            pickupInstance.transform.localRotation = Quaternion.identity;
            pickupInstance.Initialize(definition, this);
            spawnedPickup = pickupInstance;
            return pickupInstance;
        }

        public void NotifyPickupConsumed(PowerUpPickup pickup)
        {
            if (spawnedPickup == pickup)
            {
                spawnedPickup = null;
            }
        }

        public void ClearSpawnedPickup()
        {
            if (spawnedPickup == null)
            {
                return;
            }

            PowerUpPickup pickup = spawnedPickup;
            spawnedPickup = null;
            pickup.DetachFromSpawnPoint(this);

            if (pickup != null && pickup.gameObject != null)
            {
                Destroy(pickup.gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Transform anchor = spawnAnchor != null ? spawnAnchor : transform;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(anchor.position, 0.35f);
            Gizmos.DrawLine(transform.position, anchor.position);
        }

        private static PowerUpTypeMask ToMask(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.InvincibleRock:
                    return PowerUpTypeMask.InvincibleRock;
                case PowerUpType.ScoreMultiplier:
                    return PowerUpTypeMask.ScoreMultiplier;
                default:
                    return PowerUpTypeMask.None;
            }
        }
    }
}
