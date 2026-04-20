using UnityEngine;

namespace InfinityRunner
{
    public sealed class HumanPickup : MonoBehaviour
    {
        [Min(0f)] public float fleeSpeed = 2.4f;
        [Min(0f)] public float bobAmount = 0.15f;
        [Min(0f)] public float bobSpeed = 7f;

        private Vector3 initialLocalPosition;
        private bool hasInitialPosition;

        private void Awake()
        {
            initialLocalPosition = transform.localPosition;
            hasInitialPosition = true;
        }

        private void OnEnable()
        {
            if (!hasInitialPosition)
            {
                initialLocalPosition = transform.localPosition;
                hasInitialPosition = true;
            }

            transform.localPosition = initialLocalPosition;
        }

        private void Update()
        {
            Vector3 position = transform.localPosition;
            position.z += fleeSpeed * Time.deltaTime;
            position.y = initialLocalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.localPosition = position;
        }
    }
}
