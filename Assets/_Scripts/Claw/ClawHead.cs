using System;
using UnityEngine;
using UnityEngine.Assertions;
using static ClawController;

// Class to manage the claw head
public class ClawHead : MonoBehaviour
{
    [SerializeField] private LayerMask clawHeadCollisionLayerMask;
    public LayerMask ClawHeadCollisionLayerMask { get => clawHeadCollisionLayerMask; }
    [SerializeField] private float clawHeadMoveSpeed = 3f;
    [SerializeField] private AudioClip liftAudio;
    [SerializeField] private AudioClip dropAudio;

    public Action<ClawHeadState> OnClawHeadStateChangeCallback;

    private ClawManager clawManager;
    private ClawHeadState clawHeadState = ClawHeadState.None;
    private Rigidbody clawHeadRigidbody;
    public Vector3 ClawHeadRigidbodyPosition { get => clawHeadRigidbody.position; set { clawHeadRigidbody.position = value; } }
    private SphereCollider clawBodyCollider;
    private Animator clawAnimator;
    private AudioSource moveAudio;
    private ClawHeadMoveTrigger[] clawHeadMoveTriggers;

    void Awake()
    {
        clawHeadRigidbody = GetComponent<Rigidbody>();
        Assert.IsNotNull(clawHeadRigidbody);

        clawBodyCollider = clawHeadRigidbody.transform.GetComponentInChildren<SphereCollider>();
        Assert.IsNotNull(clawBodyCollider);

        clawAnimator = GetComponent<Animator>();
        Assert.IsNotNull(clawAnimator);

        moveAudio = GetComponent<AudioSource>();
        Assert.IsNotNull(moveAudio);

        clawHeadMoveTriggers = GetComponentsInChildren<ClawHeadMoveTrigger>();
        Assert.IsTrue(clawHeadMoveTriggers.Length > 0);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clawManager = ClawManager.Instance;

        foreach (ClawHeadMoveTrigger trigger in clawHeadMoveTriggers)
        {
            trigger.disableTrigger();
        }
    }

    void FixedUpdate()
    {
        moveClawHead(Time.fixedDeltaTime);
    }

    public void setClawHeadMovement(ClawHeadState state)
    {
        clawHeadState = state;

        Vector3 pushDirection = Vector3.zero;
        switch (clawHeadState)
        {
            case ClawHeadState.MoveDown:
                pushDirection = Vector3.up;
                break;

            case ClawHeadState.MoveUp:
                pushDirection = Vector3.down;
                break;
        }

        if (pushDirection != Vector3.zero)
        {
            foreach (ClawHeadMoveTrigger trigger in clawHeadMoveTriggers)
            {
                if (trigger.PushDirection == pushDirection)
                {
                    trigger.enableTrigger();
                }
                else
                {
                    trigger.disableTrigger();
                }
            }
        }
        else
        {
            foreach (ClawHeadMoveTrigger trigger in clawHeadMoveTriggers)
            {
                trigger.disableTrigger();
            }
        }
    }

    public void onGrapFinished()
    {
        setClawHeadMovement(ClawHeadState.MoveUp);
        OnClawHeadStateChangeCallback?.Invoke(clawHeadState);
    }

    public void onDropFinished()
    {
        setClawHeadMovement(ClawHeadState.Reset);
        OnClawHeadStateChangeCallback?.Invoke(clawHeadState);
    }

    public void onStartReached()
    {
        if (clawHeadState == ClawHeadState.Reset)
        {
            setClawHeadMovement(ClawHeadState.None);
            OnClawHeadStateChangeCallback?.Invoke(clawHeadState);
        }
    }

    public void drop()
    {
        if (clawHeadState == ClawHeadState.Hold)
        {
            setClawHeadMovement(ClawHeadState.Drop);
            ClawItemData[] items = GetComponentsInChildren<ClawItemData>();
            foreach (ClawItemData data in items)
            {
                Rigidbody rb = data.gameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }

                data.transform.SetParent(data.oldParent, true);
            }

            clawAnimator.SetTrigger("Drop");
            OnClawHeadStateChangeCallback?.Invoke(clawHeadState);
        }
    }

    public void edgeOfMoveTriggered(Vector3 moveOffset)
    {
        Vector3 newPosition = clawHeadRigidbody.position + moveOffset;

        switch (clawHeadState)
        {
            case ClawHeadState.MoveDown:
            case ClawHeadState.MoveUp:
                clawHeadRigidbody.MovePosition(newPosition);
                break;
        }

        switch (clawHeadState)
        {
            case ClawHeadState.MoveDown:
                setClawHeadMovement(ClawHeadState.Grab);
                clawAnimator.SetTrigger("Grab");
                OnClawHeadStateChangeCallback?.Invoke(clawHeadState);
                break;

            case ClawHeadState.MoveUp:
                setClawHeadMovement(ClawHeadState.Hold);
                if (clawManager.ClawItemsHeld.Count == 0)
                {
                    drop();
                }
                else
                {
                    foreach (ClawItemData data in clawManager.ClawItemsHeld)
                    {
                        Rigidbody rb = data.gameObject.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = true;
                        }

                        data.oldParent = data.transform.parent;
                        data.transform.SetParent(transform, true);
                    }

                    OnClawHeadStateChangeCallback?.Invoke(clawHeadState);
                }
                break;
        }
    }

    private void moveClawHead(float deltaTime)
    {
        if (clawHeadState != ClawHeadState.MoveDown
            && clawHeadState != ClawHeadState.MoveUp)
        {
            if (moveAudio.isPlaying)
            {
                moveAudio.Stop();
            }
            return;
        }

        float moveDistance = clawHeadMoveSpeed * deltaTime;
        Vector3 position = clawHeadRigidbody.position;

        switch (clawHeadState)
        {
            case ClawHeadState.MoveDown:
                position.y -= moveDistance;
                break;

            case ClawHeadState.MoveUp:
                position.y += moveDistance;
                break;
        }

        clawHeadRigidbody.MovePosition(position);
        if (!moveAudio.isPlaying)
        {
            if (clawHeadState == ClawHeadState.MoveDown)
            {
                moveAudio.clip = dropAudio;
            }
            else
            {
                moveAudio.clip = liftAudio;
            }
            moveAudio.Play();
        }
    }
}
