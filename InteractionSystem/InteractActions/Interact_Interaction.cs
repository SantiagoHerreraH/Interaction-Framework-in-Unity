using System;
using UnityEngine;

[Serializable]
public class Interact_Interaction: IInteract
{
    [SerializeField]
    private TargetType _whoWillInteract;

    public void Interact(GameObject caller, GameObject target)
    {
        IInteract[] interacts;

        switch (_whoWillInteract)
        {
            case TargetType.Caller:
                interacts = caller.GetComponents<IInteract>();
                break;
            case TargetType.Target:
                interacts = target.GetComponents<IInteract>();
                break;
            default:
                interacts = target.GetComponents<IInteract>();
                break;
        }

        foreach (var item in interacts)
        {
            item.Interact(caller, target);
        }
    }
}
