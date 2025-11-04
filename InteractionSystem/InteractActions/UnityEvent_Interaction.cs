using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnityEvent_Interaction : IInteract
{
    [SerializeField]
    private UnityEvent _event;

    public void Interact(GameObject caller, GameObject target)
    {
        _event.Invoke();
    }
}
