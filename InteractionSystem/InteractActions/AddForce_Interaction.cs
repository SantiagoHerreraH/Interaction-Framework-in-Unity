using System;
using UnityEngine;

[Serializable]
public class AddForce_Interaction : IInteract
{
    [SerializeField]
    TargetType _targetToAddForceOn;
    [SerializeField]
    ForceMode2D _forceMode2D;
    [SerializeField]
    Vector2 _vector;

    //I know this is not ideal, if need be it'll be optimized
    public void Interact(GameObject caller, GameObject target)
    {
        Rigidbody2D rb = null;

        switch (_targetToAddForceOn)
        {
            case TargetType.Caller:
                rb = caller.GetComponent<Rigidbody2D>();
                break;
            case TargetType.Target:
                rb = target.GetComponent<Rigidbody2D>();
                break;
            default:
                break;
        }

        if (rb != null)
        {
            rb.AddForce(_vector, _forceMode2D);
        }
        
    }
}
