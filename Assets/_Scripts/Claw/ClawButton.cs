using UnityEngine;
using UnityEngine.Assertions;

// Class to manage the claw drop button
public class ClawButton : MonoBehaviour
{
    private SkinnedMeshRenderer meshRenderer;
    private int pressedShapeKeyIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        Assert.IsNotNull(meshRenderer);

        pressedShapeKeyIndex = meshRenderer.sharedMesh.GetBlendShapeIndex("Pressed");
        Assert.AreNotEqual(-1, pressedShapeKeyIndex);

        if (meshRenderer != null)
        {
            Mesh mesh = meshRenderer.sharedMesh;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                Debug.Log($"Blend Shape {i}: {mesh.GetBlendShapeName(i)}");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Lerp to target shape key
    }

    public void setPressed(bool pressed)
    {
        meshRenderer.SetBlendShapeWeight(pressedShapeKeyIndex, pressed ? 100 : 0);
    }
}
