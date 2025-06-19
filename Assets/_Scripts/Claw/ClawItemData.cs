using UnityEngine;

public class ClawItemData : MonoBehaviour
{
    public Transform oldParent; // used to save original parent when object gets parented to claw head

    /// Attached Scriptable Item
    public ItemSO ItemSO;
    public GameObject LegendaryEffect;

    // ! This must match the order of materials in the Capsule prefab renderer
    private enum CapsuleMaterialIndex
    {
        Bottom,
        Top
    }

    private enum MusicRendererMaterialsIndex
    {
        Theme,
        SongImage,
        Rarity,
    }

    public void Initialize()
    {
        // Use prefab + ItemSO props to instantiate a new copy of the item in the scene
        GameObject item = Instantiate(ItemSO.Prefab, Vector3.zero, Quaternion.identity, this.transform);
        item.transform.localScale = ItemSO.PrefabScale;
        item.transform.localPosition = ItemSO.PrefabPosition;
        item.transform.localRotation = ItemSO.PrefabRotation;

        // Disable all child colliders
        foreach (Collider collider in item.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        switch (ItemSO.ItemCategory)
        {
            case ItemCategory.Wallpaper:
            case ItemCategory.Flooring:
                Material overrideMaterial = Instantiate(ItemSO.Material);
                // Set the triplanar tiling setting for preview within the claw machine
                overrideMaterial.SetFloat("_Tiling", ItemSO.ClawMaterialTiling);
                item.GetComponent<Renderer>().material = overrideMaterial;
                break;
            case ItemCategory.Music:
                // TODO: Set theme and rarity materials on cassette tape
                Material songMaterial = ItemSO.Material;
                Renderer renderer = item.GetComponent<Renderer>();
                Material[] materials = renderer.materials;
                materials[(int)MusicRendererMaterialsIndex.SongImage] = songMaterial;
                renderer.materials = materials;
                break;
        }

        // Set capsule rarity colour based on SO rarity
        Renderer capsuleRenderer = gameObject.GetComponentInChildren<Renderer>();
        Material capsuleTopMaterial = capsuleRenderer.materials[(int)CapsuleMaterialIndex.Top];
        Color rarityColor = InventoryManager.rarityToColor(ItemSO.Rarity);
        rarityColor.a = 0.78f; // Set alpha value
        capsuleTopMaterial.color = rarityColor;

        // Add Legendary Effect
        if (ItemSO.Rarity == Rarity.Legendary)
        {
            Instantiate(LegendaryEffect, gameObject.transform);
        }

        // Add RigidBody to parent object for physics-based movement
        gameObject.AddComponent<Rigidbody>();
    }
}
