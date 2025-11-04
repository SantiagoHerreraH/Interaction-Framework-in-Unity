using System;
using UnityEngine;

[Serializable]
public class SpawnGameObject_Interaction : IInteract
{
    public enum SpawnRotationType
    {
        CopyRotationReference,
        LookAtRotationReference
    }

    [SerializeField]
    GameObject _prefab;

    [SerializeField]
    private TargetType _spawnWhere;

    [SerializeField]
    private SpawnRotationType _spawnRotationType;

    [SerializeField]
    private TargetType _rotationReference;

    public void Interact(GameObject caller, GameObject target)
    {
        GameObject newGameObject = GameObject.Instantiate(_prefab);

        switch (_spawnWhere)
        {
            case TargetType.Caller:
                newGameObject.transform.position = caller.transform.position;
                break;
            case TargetType.Target:
                newGameObject.transform.position = target.transform.position;
                break;
            default:
                break;
        }

        GameObject rotationReference = null;
        switch (_rotationReference)
        {
            case TargetType.Caller:
                rotationReference = caller;
                break;
            case TargetType.Target:
                rotationReference = target;
                break;
            default:
                break;
        }

        switch (_spawnRotationType)
        {
            case SpawnRotationType.CopyRotationReference:
                newGameObject.transform.rotation = rotationReference.transform.rotation;
                break;
            case SpawnRotationType.LookAtRotationReference:
                newGameObject.transform.rotation = Quaternion.LookRotation(
                    (rotationReference.transform.position - newGameObject.transform.position).normalized,
                    Vector3.up);
                break;
            default:
                break;
        }
    }
}
