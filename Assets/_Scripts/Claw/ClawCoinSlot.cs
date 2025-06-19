using UnityEngine;
using UnityEngine.Assertions;

// Class to manage the claw coin slot
public class ClawCoinSlot : MonoBehaviour
{
    private Animator coinAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        coinAnimator = GetComponentInChildren<Animator>();
        Assert.IsNotNull(coinAnimator);
    }

    public void depositCoin()
    {
        coinAnimator.SetTrigger("DepositTrigger");
    }
}
