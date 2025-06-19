using System.Collections.Generic;
using UnityEngine;

// Class to manage the trigger associated with determining if the claw is holding an item
public class ClawHoldTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    private ClawManager clawManager;

    void Start()
    {
        clawManager = ClawManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layerMask) != 0) // Check if layer matches
        {
            ClawItemData data = other.gameObject.GetComponentInParent<ClawItemData>();
            if (data != null)
            {
                clawManager.onItemHeld(data);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & layerMask) != 0) // Check if layer matches
        {
            ClawItemData data = other.gameObject.GetComponentInParent<ClawItemData>();
            if (data != null)
            {
                clawManager.onItemReleased(data);
            }
        }
    }
}
