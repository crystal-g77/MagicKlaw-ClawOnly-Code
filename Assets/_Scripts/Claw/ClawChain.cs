using UnityEngine;
using UnityEngine.Assertions;

public class ClawChain : MonoBehaviour
{
    [SerializeField] private Transform clawHeadTransform;
    [SerializeField] private Transform clawBaseTransform;
    [SerializeField] private float chainLinkLength = .5f;
    [SerializeField] private float chainLinkWidth = .2f;
    [SerializeField] private bool updateChainEndPoints = true;
    private LineRenderer chainRenderer;
    private Transform clawHeadTop;
    private Transform clawBaseBottom;

    void Start()
    {
        SphereCollider clawBodyCollider = clawHeadTransform.GetComponentInChildren<SphereCollider>();
        Assert.IsNotNull(clawBodyCollider);

        GameObject clawHead = new GameObject("ClawHeadTop");
        clawHead.transform.SetParent(clawHeadTransform);
        Vector3 position = new Vector3(clawBodyCollider.bounds.center.x, clawBodyCollider.bounds.max.y, clawBodyCollider.bounds.center.z);
        clawHead.transform.position = position;
        clawHeadTop = clawHead.transform;

        BoxCollider clawBaseCollider = clawBaseTransform.GetComponent<BoxCollider>();
        Assert.IsNotNull(clawBaseCollider);

        GameObject clawbase = new GameObject("ClawBaseBottom");
        clawbase.transform.SetParent(clawBaseTransform);
        position = new Vector3(clawBaseCollider.bounds.center.x, clawBaseCollider.bounds.min.y, clawBaseCollider.bounds.center.z);
        clawbase.transform.position = position;
        clawBaseBottom = clawbase.transform;

        chainRenderer = GetComponent<LineRenderer>();
        Assert.IsNotNull(chainRenderer);
        chainRenderer.positionCount = 2;
        // Set chain width
        chainRenderer.startWidth = chainLinkWidth;
        chainRenderer.endWidth = chainLinkWidth;
        // Set chain height
        Vector2 currentScale = chainRenderer.material.GetTextureScale("_BaseMap");
        currentScale.x = chainLinkLength / transform.lossyScale.y;
        chainRenderer.material.SetTextureScale("_BaseMap", currentScale);
        // Set initial chain positions
        chainRenderer.SetPosition(0, clawHeadTop.position);
        chainRenderer.SetPosition(1, clawBaseBottom.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (updateChainEndPoints)
        {
            // Update chain positions
            chainRenderer.SetPosition(0, clawHeadTop.position);
            chainRenderer.SetPosition(1, clawBaseBottom.position);
        }
    }
}
