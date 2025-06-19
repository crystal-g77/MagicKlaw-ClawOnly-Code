using System;
using UnityEngine;
using static ClawController;

// Class to manage the claw joystick
public class ClawJoystick : MonoBehaviour
{
    [SerializeField] private Transform joystickBoneTransform;

    [EnumArray(typeof(ClawBaseState))]
    [SerializeField] private Vector3[] joystickPositions = new Vector3[Enum.GetValues(typeof(ClawBaseState)).Length];

    // Update is called once per frame
    void Update()
    {
        // TODO: Lerp to target joystick position
    }

    public void setJoystickRotation(ClawBaseState state)
    {
        joystickBoneTransform.localRotation = Quaternion.Euler(joystickPositions[(int)state]);
    }
}
