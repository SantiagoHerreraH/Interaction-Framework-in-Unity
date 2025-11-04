using System;
using UnityEngine;

[Serializable]
public class PlaySound_Interaction : IInteract
{

    [SerializeField]
    private TargetType _whoToPlaySoundOn;
    [SerializeField]
    string _soundName;

    public void Interact(GameObject caller, GameObject target)
    {
        GameObject chosen = null;
        switch (_whoToPlaySoundOn)
        {
            case TargetType.Caller:
                chosen = caller;
                break;
            case TargetType.Target:
                chosen = target;
                break;
            default:
                chosen = target;
                break;
        }

        AudioManager.Instance.PlaySound(chosen, _soundName);
    }

}
