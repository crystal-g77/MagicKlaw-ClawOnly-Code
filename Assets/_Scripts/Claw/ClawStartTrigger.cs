using UnityEngine;

// Class to manage whether the claw base is at the start location
public class ClawStartTrigger : MonoBehaviour
{
    [SerializeField] private ClawController clawController;
    [SerializeField] private LayerMask layerMask;

    private void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & layerMask) != 0) // Check if layer matches
        {
            if (BoundsUtility.IsWithinBounds(GetComponent<Collider>(), other))
            {
                clawController.onStartTriggerReached();
            }
        }
    }
}
