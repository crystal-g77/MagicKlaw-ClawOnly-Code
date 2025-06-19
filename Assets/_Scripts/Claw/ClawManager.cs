
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// Class to manage the claw minigame
public class ClawManager : MonoBehaviour
{
    [SerializeField] private int numItemsToSpawn = 5;
    [SerializeField]
    private float creationDelayTime = .5f;
    [SerializeField] private float creationForce = 1.0f;
    [SerializeField] private List<Transform> creationPoints = new List<Transform>();

    [SerializeField] private int coinCost = 1;

    [SerializeField] private float collectionDelayTime = 2.0f;

    [SerializeField] private BoxCollider collectionBlockCollider;

    private GameManager gameManager;

    private InventoryManager inventoryManager;
    private List<Queue<ItemType>> itemTypesToCreate = new List<Queue<ItemType>>();

    private CameraManager cameraManager;
    private enum CameraIndex
    {
        Front,
        Side,
    }

    private HashSet<ClawItemData> clawItemsHeld = new HashSet<ClawItemData>();
    public HashSet<ClawItemData> ClawItemsHeld { get => clawItemsHeld; }

    private bool newItemUIActive = false;
    public bool NewItemUIActive { get => newItemUIActive; set { newItemUIActive = value; } }

    private bool coinDeposited = false;
    private bool controlRequired = false;

    public Action<bool> OnCoinRequired;
    public Action<bool> OnControlRequired;
    public Action<bool> OnCoinDeposited;
    public Action<bool> OnNoCoins;

    public static ClawManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(collectionBlockCollider);
        Assert.IsTrue(creationPoints.Count > 0);

        collectionBlockCollider.enabled = true;

        gameManager = GameManager.Instance;

        inventoryManager = InventoryManager.Instance;

        // Create a list of weighted item types by placing each ItemType in the list
        // the number of times equal to its weighting
        List<ItemType> weightedItemTypes = new List<ItemType>();
        Array values = Enum.GetValues(typeof(ItemType)); // Get all enum values
        foreach (ItemType type in values)
        {
            if (type.isDefaultItem())
            {
                continue;
            }

            int weighting = inventoryManager.GetItemSO(type).Rarity.getWeighting();
            for (int j = 0; j < weighting; ++j)
            {
                weightedItemTypes.Add(type);
            }
        }

        // Create a spawn queue for each spawn point
        for (int i = 0; i < creationPoints.Count; ++i)
        {
            itemTypesToCreate.Add(new Queue<ItemType>());
        }

        // Add random items to each spawn point
        for (int i = 0; i < numItemsToSpawn; ++i)
        {
            int queueIndex = i % creationPoints.Count;
            int randomIndex = UnityEngine.Random.Range(0, weightedItemTypes.Count); // Pick a random index
            itemTypesToCreate[queueIndex].Enqueue(weightedItemTypes[randomIndex]);
        }

        // Start spawning items in the queue for each spawn point
        for (int i = 0; i < creationPoints.Count; ++i)
        {
            if (itemTypesToCreate[i].Count > 0)
            {
                StartCoroutine(spawnClawItems(creationPoints[i], itemTypesToCreate[i]));
            }
        }
    }

    void OnEnable()
    {
        cameraManager = CameraManager.Instance;

        cameraManager.AssertNumCameras(Enum.GetNames(typeof(CameraIndex)).Length);
    }

    void OnDisable()
    {
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (shouldAllowInteraction() && !isCoinDeposited() && gameManager.CoinCount >= coinCost)
#else
        if (shouldAllowInteraction() && !isCoinDeposited() && canDepositCoin())
#endif
        {
            OnCoinRequired?.Invoke(true);
        }
        else
        {
            OnCoinRequired?.Invoke(false);
        }

        if (shouldAllowInteraction() && controlRequired)
        {
            OnControlRequired?.Invoke(true);
        }
        else
        {
            OnControlRequired?.Invoke(false);
        }

#if UNITY_EDITOR
        if (gameManager.CoinCount < coinCost && !coinDeposited)
#else
        if (!canDepositCoin() && !coinDeposited)
#endif
        {
            OnNoCoins?.Invoke(true);
        }
        else
        {
            OnNoCoins?.Invoke(false);
        }
    }

    // Check if player interaction is allowed
    public bool shouldAllowInteraction()
    {
        if (itemTypesToCreate.Count > 0)
        {
            for (int i = 0; i < creationPoints.Count; ++i)
            {
                if (itemTypesToCreate[i].Count > 0)
                {
                    return false;
                }
            }
        }

        return !TutorialManager.Instance.isTutorialShowing() && !newItemUIActive
            && cameraManager.IsCameraActive((int)CameraIndex.Front);
    }

    // Check if a coin can be deposited (do we have enough)
    public bool canDepositCoin()
    {
#if UNITY_EDITOR
        if (gameManager.CoinCount < coinCost)
        {
            gameManager.AddCoins(coinCost);
        }
#endif

        return gameManager.CoinCount >= coinCost;
    }

    public bool isCoinDeposited()
    {
        return coinDeposited;
    }

    public void depositCoin()
    {
        if (gameManager.CoinCount < coinCost)
        {
            throw new Exception("Cannot use the claw with no coins!");
        }

        gameManager.RemoveCoins(coinCost);
        coinDeposited = true;
        controlRequired = true;
        OnCoinDeposited?.Invoke(coinDeposited);
    }

    public void controlUsed()
    {
        controlRequired = false;
    }

    public void resetDepositeCoin()
    {
        coinDeposited = false;
        OnCoinDeposited?.Invoke(coinDeposited);
    }

    // Called when an item enters the hold trigger
    public void onItemHeld(ClawItemData data)
    {
        clawItemsHeld.Add(data);
    }

    // Called when an item leaves the hold trigger
    public void onItemReleased(ClawItemData data)
    {
        clawItemsHeld.Remove(data);
    }

    public void onItemCollected(ClawItemData data)
    {
        inventoryManager.AddItemToInventory(data.ItemSO.ItemType);

        StartCoroutine(destroyCollectedItem(data.gameObject));
    }

    // Spawn items from a specific creation point
    private IEnumerator spawnClawItems(Transform creationPoint, Queue<ItemType> queue)
    {
        spawnClawItem(creationPoint, queue.Dequeue());

        while (queue.Count > 0)
        {
            float timer = 0;
            while (timer < creationDelayTime)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            spawnClawItem(creationPoint, queue.Dequeue());
        }

        yield return new WaitForSecondsRealtime(1.0f);

        collectionBlockCollider.enabled = false;
    }

    // Spawn a single item
    private void spawnClawItem(Transform creationPoint, ItemType type)
    {
        ClawItemData data = inventoryManager.CreateClawItem(type);
        data.transform.SetParent(transform);
        data.transform.position = creationPoint.position;
        LayerUtility.SetLayerRecursively(data.gameObject, LayerMask.NameToLayer("Claw Machine Item"));
        Rigidbody rb = data.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Give the item a random impulse force so that not all items follow the same path
            Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDirection * creationForce, ForceMode.Impulse);
        }
    }

    private IEnumerator destroyCollectedItem(GameObject obj)
    {
        float timer = 0;
        while (timer < collectionDelayTime)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        Destroy(obj);
    }
}
