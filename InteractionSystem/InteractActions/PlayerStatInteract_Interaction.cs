using System;
using UnityEngine;

[Serializable]
public class PlayerStatInteract_Interaction: IInteract
{
    [SerializeField]
    private TargetType _whoWillInteract;
    [SerializeField]
    private string _statName;
    [SerializeField]
    private int _offSetAmount;

    public void Interact(GameObject caller, GameObject target)
    {
        PlayerStatCounter playerStatCounter = null;

        switch (_whoWillInteract)
        {
            case TargetType.Caller:
                caller.TryGetComponent(out playerStatCounter);
                break;
            case TargetType.Target:
                target.TryGetComponent(out playerStatCounter);
                break;
            default:
                target.TryGetComponent(out playerStatCounter);
                break;
        }


        if (playerStatCounter)
        {
            playerStatCounter.OffsetExternalStat(_statName, _offSetAmount);
        }
    }
}
