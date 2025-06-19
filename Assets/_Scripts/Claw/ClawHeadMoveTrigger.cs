using System;
using UnityEngine;
using UnityEngine.Assertions;

// Class to manage the triggers associated with stopping the claw head downward and upward movement
public class ClawHeadMoveTrigger : MonoBehaviour
{
    [SerializeField] private ClawHead clawHead;
    [SerializeField] private Collider nearestCollider;
    [SerializeField] private LayerMask mask;
    [SerializeField] private Vector3 direction;
    public Vector3 PushDirection { get => direction; }

    private Collider triggerCollider;

    void Awake()
    {
        Assert.IsNotNull(clawHead);
        Assert.IsNotNull(nearestCollider);

        triggerCollider = GetComponent<Collider>();
        Assert.IsNotNull(triggerCollider);
    }

    void OnTriggerEnter(Collider other)
    {
        if (LayerUtility.IsLayerInMask(mask, other.gameObject.layer))
        {
            // Compute minimum distance required to separate the two colliders
            (float overlapDistance, Vector3 pushDirection) = estimateOverlap(other,
                new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z)));

            // Apply a small buffer to ensure it's outside
            float buffer = 0.01f;
            Vector3 moveOffset = pushDirection * (overlapDistance + buffer);

            if (moveOffset != Vector3.zero)
            {
                clawHead.edgeOfMoveTriggered(moveOffset);
            }
        }
    }

    public void enableTrigger()
    {
        triggerCollider.enabled = true;
    }

    public void disableTrigger()
    {
        triggerCollider.enabled = false;
    }

    private (float, Vector3) estimateOverlap(Collider other, Vector3 direction)
    {
        float triggerMin = 0f;
        float triggerMax = 0f;
        float colliderCenter = 0f;
        float otherMin = 0f;
        float otherMax = 0f;
        float otherCenter = 0f;

        if (direction == Vector3.up)
        {
            // Get the minimum and maximum Y values (the bounds' Y extents)
            triggerMin = triggerCollider.bounds.min.y;
            triggerMax = triggerCollider.bounds.max.y;
            colliderCenter = nearestCollider.bounds.center.y;
            otherMin = other.bounds.min.y;
            otherMax = other.bounds.max.y;
            otherCenter = other.bounds.center.y;
        }
        else if (direction == Vector3.right)
        {
            // Get the minimum and maximum Y values (the bounds' X extents)
            triggerMin = triggerCollider.bounds.min.x;
            triggerMax = triggerCollider.bounds.max.x;
            colliderCenter = nearestCollider.bounds.center.x;
            otherMin = other.bounds.min.x;
            otherMax = other.bounds.max.x;
            otherCenter = other.bounds.center.x;
        }
        else if (direction == Vector3.forward)
        {
            // Get the minimum and maximum Y values (the bounds' Z extents)
            triggerMin = triggerCollider.bounds.min.z;
            triggerMax = triggerCollider.bounds.max.z;
            colliderCenter = nearestCollider.bounds.center.z;
            otherMin = other.bounds.min.z;
            otherMax = other.bounds.max.z;
            otherCenter = other.bounds.center.z;
        }

        // Check if there's overlap in the Y direction
        if (triggerMax > otherMin && triggerMin < otherMax)
        {
            // Calculate the amount of overlap
            float overlap = Mathf.Min(triggerMax, otherMax) - Mathf.Max(triggerMin, otherMin);

            // Determine the direction of the push
            if (colliderCenter > otherCenter)
            {
                // Push in the positive if the nearest collider is in a position greater than the other object
                return (overlap, direction);
            }
            else
            {
                // Push negative if the nearest collider is in a position less than the other object
                return (overlap, -direction);
            }
        }

        // No overlap
        return (0f, Vector3.zero);
    }
}
