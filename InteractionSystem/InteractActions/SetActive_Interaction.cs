using System;
using UnityEngine;

[Serializable]
public class SetActive_Interaction : IInteract
{
    [SerializeField]
    private TargetType _who;
    [SerializeField]
    private bool _setActive;

    public void Interact(GameObject caller, GameObject target)
    {
        switch (_who)
        {
            case TargetType.Caller:
                caller.SetActive(_setActive);
                break;
            case TargetType.Target:
                target.SetActive(_setActive);
                break;
            default:
                target.SetActive(_setActive);
                break;
        }
        throw new NotImplementedException();
    }
}
