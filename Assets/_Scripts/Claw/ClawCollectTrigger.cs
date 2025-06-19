using UnityEngine;

// Class to manager the claw item collected trigger
public class ClawCollectTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem particles;

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
                clawManager.onItemCollected(data);
                audioSource.Play();
                particles.Stop();
                particles.Play();
            }
        }
    }
}
