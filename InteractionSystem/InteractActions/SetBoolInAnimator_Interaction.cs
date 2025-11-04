using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SetBoolInAnimator_Interaction : IInteract
{

    [SerializeField]
    private TargetType _whoToSetBoolOn;
    [SerializeField]
    private string _boolName;
    [SerializeField]
    private bool _value;

    public void Interact(GameObject caller, GameObject target)
    {
        Animator[] animators = null;
        switch (_whoToSetBoolOn)
        {
            case TargetType.Caller:

                animators = caller.GetComponents<Animator>();
                if (animators != null && animators.Length != 0)
                {
                    foreach (var item in animators)
                    {

                        item.SetBool(_boolName, _value);
                    }
                }
                else
                {
                    animators = caller.GetComponentsInChildren<Animator>(true);

                    if (animators != null)
                    {
                        foreach (var item in animators)
                        {
                            item.SetBool(_boolName, _value);
                        }
                    }
                }

                break;
            case TargetType.Target:

                animators = target.GetComponents<Animator>();
                if (animators != null && animators.Length != 0)
                {
                    foreach (var item in animators)
                    {
                        item.SetBool(_boolName, _value);
                    }
                }
                else 
                {
                    animators = target.GetComponentsInChildren<Animator>(true);

                    if (animators != null)
                    {
                        foreach (var item in animators)
                        {
                            item.SetBool(_boolName, _value);
                        }
                    }

                    Debug.Log("Set Animation bool to " + _value + " in animation " +_boolName);
                }

                break;
            default:

                animators = caller.GetComponents<Animator>();
                if (animators != null && animators.Length != 0)
                {
                    foreach (var item in animators)
                    {

                        item.SetBool(_boolName, _value);
                    }
                }
                else 
                {
                    animators = caller.GetComponentsInChildren<Animator>(true);

                    if (animators != null)
                    {
                        foreach (var item in animators)
                        {
                            item.SetBool(_boolName, _value);
                        }
                    }
                }

                break;
        }

    }

}
