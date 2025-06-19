using UnityEngine;

// Class to manage the trigger to drop the item in the claw
public class ClawDropTrigger : MonoBehaviour
{
    [SerializeField] private ClawController clawController;
    [SerializeField] private LayerMask layerMask;

    private void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & layerMask) != 0) // Check if layer matches
        {
            if (BoundsUtility.IsWithinBounds(GetComponent<Collider>(), other))
            {
                clawController.onDropTriggerReached();
            }
        }
    }
}
