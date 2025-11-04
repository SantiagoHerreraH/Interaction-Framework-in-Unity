using System;
using UnityEngine;

[Serializable]
public class TrailRenderer_Interaction : IInteract
{
    [SerializeField]
    private TargetType _who;
    [SerializeField]
    private bool _isEnabled;

    public void Interact(GameObject caller, GameObject target)
    {
        TrailRenderer trailRenderer = null;
        switch (_who)
        {
            case TargetType.Caller:
                trailRenderer = caller.GetComponent<TrailRenderer>();
                if (trailRenderer != null)
                {
                    trailRenderer.enabled = _isEnabled;
                }
                else
                {
                    trailRenderer = caller.GetComponentInChildren<TrailRenderer>(true);
                    if (trailRenderer != null)
                    {
                        trailRenderer.enabled = _isEnabled;
                    }
                }
                break;
            case TargetType.Target:
                trailRenderer = target.GetComponent<TrailRenderer>();
                if (trailRenderer != null)
                {
                    trailRenderer.enabled = _isEnabled;
                }
                else
                {
                    trailRenderer = caller.GetComponentInChildren<TrailRenderer>(true);
                    if (trailRenderer != null)
                    {
                        trailRenderer.enabled = _isEnabled;
                    }
                }
                break;
            default:
                trailRenderer = target.GetComponent<TrailRenderer>();
                if (trailRenderer != null)
                {
                    trailRenderer.enabled = _isEnabled;
                }
                else
                {
                    trailRenderer = caller.GetComponentInChildren<TrailRenderer>(true);
                    if (trailRenderer != null)
                    {
                        trailRenderer.enabled = _isEnabled;
                    }
                }
                break;
        }
    }
}
