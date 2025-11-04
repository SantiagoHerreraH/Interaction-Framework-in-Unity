using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnityTargetEvent_Interaction : IInteract
{

    [SerializeField]
    private TargetType _whoToForwardInEventCall;
    [SerializeField]
    private UnityEvent<GameObject> _event;

    public void Interact(GameObject caller, GameObject target)
    {
        switch (_whoToForwardInEventCall)
        {
            case TargetType.Caller:
                _event.Invoke(caller);
                break;
            case TargetType.Target:
                _event.Invoke(target);
                break;
            default:
                _event.Invoke(target);
                break;
        }

    }

}
