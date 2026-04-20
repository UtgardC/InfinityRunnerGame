using UnityEngine;

namespace InfinityRunner
{
    public sealed class PowerUpSpawnPoint : MonoBehaviour
    {
        public Lane lane = Lane.Center;
        public bool allowDestroyAll = true;
        public bool allowDivineRamp = true;

        public bool Allows(PowerUpType type)
        {
            if (type == PowerUpType.DestroyAll)
            {
                return allowDestroyAll;
            }

            if (type == PowerUpType.DivineRamp)
            {
                return allowDivineRamp;
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.35f);
        }
    }
}
