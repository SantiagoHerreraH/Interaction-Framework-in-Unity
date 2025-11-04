using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class GamepadRumble_StartEndInteraction : IStartEndInteract
{
    public enum SpawnRotationType
    {
        CopyRotationReference,
        LookAtRotationReference
    }

    public enum DespawnActionOnEnd
    {
        Deactivate,
        Destroy,
        None
    }

    [Header("Rumble Params")]
    [SerializeField]
    float _lowFrequency;
    [SerializeField]
    float _highFrequency;

    [Header("TargetType")]
    [SerializeField]
    private TargetType _whereIsPlayerInput;


    public void StartInteract(GameObject caller, GameObject target)
    {
        PlayerInput playerInput = null;

        switch (_whereIsPlayerInput)
        {
            case TargetType.Caller:
                if (caller.TryGetComponent(out playerInput))
                {
                    var gamepad = playerInput.devices.FirstOrDefault(device => device is Gamepad) as Gamepad;

                    if (gamepad != null)
                    {
                        gamepad.SetMotorSpeeds(_lowFrequency, _highFrequency);
                    }
                }
                
                break;
            case TargetType.Target:
                if (target.TryGetComponent(out playerInput))
                {
                    var gamepad = playerInput.devices.FirstOrDefault(device => device is Gamepad) as Gamepad;

                    if (gamepad != null)
                    {
                        gamepad.SetMotorSpeeds(_lowFrequency, _highFrequency);
                    }
                }


                break;
            default:
                break;
        }
    }

    public void EndInteract(GameObject caller, GameObject target)
    {

        PlayerInput playerInput = null;

        switch (_whereIsPlayerInput)
        {
            case TargetType.Caller:
                if (caller.TryGetComponent(out playerInput))
                {
                    var gamepad = playerInput.devices.FirstOrDefault(device => device is Gamepad) as Gamepad;

                    if (gamepad != null)
                    {
                        gamepad.SetMotorSpeeds(0, 0);
                    }
                }

                break;
            case TargetType.Target:
                if (target.TryGetComponent(out playerInput))
                {
                    var gamepad = playerInput.devices.FirstOrDefault(device => device is Gamepad) as Gamepad;

                    if (gamepad != null)
                    {
                        gamepad.SetMotorSpeeds(0, 0);
                    }
                }


                break;
            default:
                break;
        }

    }
}
