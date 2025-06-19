using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// Class to manager the player interaction with the claw minigame
public class ClawController : MonoBehaviour
{
    [SerializeField] private Claw claw;
    [SerializeField] private ClawHead clawHead;
    [SerializeField] private ClawCoinSlot coinSlot;
    [SerializeField] private ClawJoystick joystick;
    [SerializeField] private ClawButton button;
    [SerializeField] private LayerMask controlLayerMask;
    [SerializeField] private Collider dropArea;

    [SerializeField] private float joystickDeadZone = 10f;

    private InputManager inputManager;
    private ClawManager clawManager;

    private bool areControlsActive = true;
    private bool isJoystickHoldActive = false;
    private bool isButtonHoldActive = false;
    private bool isAutoMoveActive = false;
    private Collider coinSlotCollider;
    private Collider joystickCollider;
    private Collider buttonCollider;

    public enum ClawBaseState
    {
        None,
        MoveForward,
        MoveBack,
        MoveLeft,
        MoveRight,
        MoveForwardRight,
        MoveForwardLeft,
        MoveBackRight,
        MoveBackLeft,
    }

    public enum ClawHeadState
    {
        None,
        MoveDown,
        MoveUp,
        Grab,
        Hold,
        Drop,
        Reset
    }

    void Awake()
    {
        Assert.IsNotNull(coinSlot);
        Assert.IsNotNull(joystick);
        Assert.IsNotNull(button);
        Assert.IsNotNull(dropArea);

        coinSlotCollider = coinSlot.GetComponent<Collider>();
        joystickCollider = joystick.GetComponent<Collider>();
        buttonCollider = button.GetComponent<Collider>();
        Assert.IsNotNull(coinSlotCollider);
        Assert.IsNotNull(joystickCollider);
        Assert.IsNotNull(buttonCollider);
    }

    void OnEnable()
    {
        inputManager = InputManager.Instance;
        inputManager.OnHoldBeginCallback += onHoldBeginCallback;
        inputManager.OnHoldMovedCallback += onHoldMovedCallback;
        inputManager.OnHoldEndCallback += onHoldEndCallback;

        clawHead.OnClawHeadStateChangeCallback += onClawHeadStateChangeCallback;
    }

    void OnDisable()
    {
        clawHead.OnClawHeadStateChangeCallback -= onClawHeadStateChangeCallback;

        if (inputManager != null)
        {
            inputManager.OnHoldBeginCallback -= onHoldBeginCallback;
            inputManager.OnHoldMovedCallback -= onHoldMovedCallback;
            inputManager.OnHoldEndCallback -= onHoldEndCallback;
        }
    }

    void Start()
    {
        clawManager = ClawManager.Instance;
    }

    private void onHoldBeginCallback(Vector2 touchPoint)
    {
        if (!clawManager.shouldAllowInteraction())
        {
            return;
        }

        Vector3 worldPosition;
        if (CameraUtility.GetWorldPositionFromTouchPoint(touchPoint, controlLayerMask, out worldPosition))
        {
            if (areControlsActive && clawManager.isCoinDeposited())
            {
                if (joystickCollider.bounds.Contains(worldPosition))
                {
                    isJoystickHoldActive = true;
                    clawManager.controlUsed();
                }
                else if (buttonCollider.bounds.Contains(worldPosition))
                {
                    clawHead.setClawHeadMovement(ClawHeadState.MoveDown);
                    isButtonHoldActive = true;
                    areControlsActive = false;
                    button.setPressed(isButtonHoldActive);
                    clawManager.controlUsed();
                }
            }
            else if (!clawManager.isCoinDeposited() && clawManager.canDepositCoin())
            {
                if (coinSlotCollider.bounds.Contains(worldPosition))
                {
                    clawManager.depositCoin();
                    coinSlot.depositCoin();
                }
            }
        }
    }

    private void onHoldMovedCallback(Vector2 touchPoint, Vector2 deltatouchPoint)
    {
        if (isJoystickHoldActive)
        {
            determineClawMovement(deltatouchPoint);
        }
    }

    private void onHoldEndCallback(Vector2 touchPoint, Vector2 deltatouchPoint)
    {
        if (isJoystickHoldActive)
        {
            isJoystickHoldActive = false;
            claw.setClawBaseMovement(ClawBaseState.None);
            joystick.setJoystickRotation(ClawBaseState.None);
        }
        else if (isButtonHoldActive)
        {
            isButtonHoldActive = false;
            button.setPressed(isButtonHoldActive);
        }
    }

    private void onClawHeadStateChangeCallback(ClawHeadState newState)
    {
        switch (newState)
        {
            case ClawHeadState.None:
                areControlsActive = true;
                claw.setClawBaseMovement(ClawBaseState.None);
                break;
            case ClawHeadState.Hold:
                dropArea.enabled = false;
                isAutoMoveActive = true;
                claw.setClawBaseMovement(ClawBaseState.MoveForwardRight);
                break;
            case ClawHeadState.Drop:
                if (isAutoMoveActive)
                {
                    isAutoMoveActive = false;
                    claw.setClawBaseMovement(ClawBaseState.None);
                }
                break;
            case ClawHeadState.Reset:
                isAutoMoveActive = true;
                claw.setClawBaseMovement(ClawBaseState.MoveBackLeft);
                break;
        }
    }

    private void determineClawMovement(Vector2 deltatouchPoint)
    {
        ClawBaseState state = ClawBaseState.None;
        if (deltatouchPoint.x < -joystickDeadZone)
        {
            if (deltatouchPoint.y < -joystickDeadZone)
            {
                state = ClawBaseState.MoveForwardLeft;
            }
            else if (deltatouchPoint.y > joystickDeadZone)
            {
                state = ClawBaseState.MoveBackLeft;
            }
            else
            {
                state = ClawBaseState.MoveLeft;
            }
        }
        else if (deltatouchPoint.x > joystickDeadZone)
        {
            if (deltatouchPoint.y < -joystickDeadZone)
            {
                state = ClawBaseState.MoveForwardRight;
            }
            else if (deltatouchPoint.y > joystickDeadZone)
            {
                state = ClawBaseState.MoveBackRight;
            }
            else
            {
                state = ClawBaseState.MoveRight;
            }
        }
        else
        {
            if (deltatouchPoint.y < -joystickDeadZone)
            {
                state = ClawBaseState.MoveForward;
            }
            else if (deltatouchPoint.y > joystickDeadZone)
            {
                state = ClawBaseState.MoveBack;
            }
        }
        claw.setClawBaseMovement(state);
        joystick.setJoystickRotation(state);
    }

    public void onDropTriggerReached()
    {
        clawHead.drop();
    }

    public void onStartTriggerReached()
    {
        if (isAutoMoveActive && clawManager.ClawItemsHeld.Count == 0)
        {
            isAutoMoveActive = false;
            dropArea.enabled = true;
            clawHead.onStartReached();
            clawManager.resetDepositeCoin();
        }
    }

    void OnDrawGizmos()
    {
        RaycastHit hit;
        if (Physics.Raycast(clawHead.transform.position, -transform.up, out hit, Mathf.Infinity, clawHead.ClawHeadCollisionLayerMask))
        {
            Gizmos.color = Color.yellow;
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Claw Machine Item")) // Check if layer matches
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawLine(clawHead.transform.position, hit.point);
        }
    }
}
