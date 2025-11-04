using System;
using UnityEngine;

[Serializable]
public class TargetedAddForce_Interaction : IInteract
{

    [SerializeField]
    private TargetType _pushWho;
    [SerializeField]
    private TargetType _pushTowardsWhichDirection;
    [SerializeField]
    ForceMode2D _forceMode2D;
    [SerializeField]
    float _pushMagnitude;

    public void Interact(GameObject caller, GameObject target)
    {
        Rigidbody2D rb = null; 

        switch (_pushWho)
        {
            case TargetType.Caller:
                rb = caller.GetComponent<Rigidbody2D>();
                break;
            case TargetType.Target:
                rb = target.GetComponent<Rigidbody2D>();
                break;
            default:
                rb = target.GetComponent<Rigidbody2D>();
                break;
        }

        Vector3 pushDirection;

        switch (_pushTowardsWhichDirection)
        {
            case TargetType.Caller:
                pushDirection = caller.transform.position - target.transform.position;
                break;
            case TargetType.Target:
                pushDirection = target.transform.position - caller.transform.position;
                break;
            default:
                pushDirection = target.transform.position - caller.transform.position;
                break;
        }

        if (rb != null)
        {
            rb.AddForce(pushDirection.normalized * _pushMagnitude, _forceMode2D);
        }
    }

}
