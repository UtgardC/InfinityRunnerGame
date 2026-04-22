using UnityEngine;

namespace InfinityRunner
{
    public static class RunnerCollisionUtility
    {
        public static RunnerInteractable FindInteractable(Collider collider)
        {
            return collider != null ? collider.GetComponentInParent<RunnerInteractable>() : null;
        }

        public static bool IsDeathObject(Collider collider, LayerMask deathLayers, string deathTag)
        {
            if (collider == null)
            {
                return false;
            }

            RunnerInteractable interactable = FindInteractable(collider);
            if (interactable != null && interactable.interactionType == RunnerInteractableType.Death)
            {
                return true;
            }

            Transform current = collider.transform;
            while (current != null)
            {
                GameObject currentObject = current.gameObject;
                if ((deathLayers.value & (1 << currentObject.layer)) != 0)
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(deathTag) && currentObject.tag == deathTag)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }
    }
}
