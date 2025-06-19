using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using static ClawController;

// Class to manage the claw base and its movement
public class Claw : MonoBehaviour
{
    [SerializeField] private ClawHead clawHead;
    [SerializeField] private LayerMask clawBaseCollisionLayerMask;
    [SerializeField] private float clawBaseMoveSpeed = 2f;

    private ClawBaseState clawBaseState = ClawBaseState.None;
    private CharacterController clawBaseRigidbody;
    private BoxCollider clawBaseCollider;
    private AudioSource moveAudio;

    void Awake()
    {
        clawBaseRigidbody = GetComponent<CharacterController>();
        Assert.IsNotNull(clawBaseRigidbody);

        Transform baseTransform = clawBaseRigidbody.transform.Find("ClawBase");
        clawBaseCollider = baseTransform.GetComponent<BoxCollider>();
        Assert.IsNotNull(clawBaseCollider);

        moveAudio = GetComponent<AudioSource>();
        Assert.IsNotNull(moveAudio);
    }

    void FixedUpdate()
    {
        moveClawBase(Time.fixedDeltaTime);
    }

    public void setClawBaseMovement(ClawBaseState state)
    {
        clawBaseState = state;
    }

    private void moveClawBase(float deltaTime)
    {
        if (clawBaseState == ClawBaseState.None)
        {
            if (moveAudio.isPlaying)
            {
                moveAudio.Stop();
            }
            return;
        }

        float moveDistance = clawBaseMoveSpeed * deltaTime;
        Vector3 position = Vector3.zero;

        switch (clawBaseState)
        {
            case ClawBaseState.MoveForward:
                position.z -= moveDistance;
                break;

            case ClawBaseState.MoveBack:
                position.z += moveDistance;
                break;

            case ClawBaseState.MoveLeft:
                position.x -= moveDistance;
                break;

            case ClawBaseState.MoveRight:
                position.x += moveDistance;
                break;

            case ClawBaseState.MoveForwardRight:
                moveDistance /= Mathf.Sqrt(2f);
                position.z -= moveDistance;
                position.x += moveDistance;
                break;

            case ClawBaseState.MoveForwardLeft:
                moveDistance /= Mathf.Sqrt(2f);
                position.z -= moveDistance;
                position.x -= moveDistance;
                break;

            case ClawBaseState.MoveBackRight:
                moveDistance /= Mathf.Sqrt(2f);
                position.z += moveDistance;
                position.x += moveDistance;
                break;

            case ClawBaseState.MoveBackLeft:
                moveDistance /= Mathf.Sqrt(2f);
                position.z += moveDistance;
                position.x -= moveDistance;
                break;
        }

        clawBaseRigidbody.Move(position);

        if (!moveAudio.isPlaying)
        {
            moveAudio.Play();
        }
    }
}
