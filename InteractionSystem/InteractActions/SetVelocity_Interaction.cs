using System;
using UnityEngine;

[Serializable]
public class SetVelocity_Interaction : IInteract
{
    [SerializeField]
    private TargetType _who;
    [SerializeField]
    private Vector2 _velocity;

    public void Interact(GameObject caller, GameObject target)
    {
        Rigidbody2D rb = null;

        switch (_who)
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

        if (rb != null)
        {
            rb.linearVelocity = _velocity;
        }
    }
}
