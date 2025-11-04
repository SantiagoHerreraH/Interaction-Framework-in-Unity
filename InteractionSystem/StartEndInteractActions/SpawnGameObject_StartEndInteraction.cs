using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnGameObject_StartEndInteraction : IStartEndInteract
{
    public enum SpawnRotationType
    {
        CopyRotationReference,
        LookAtRotationReference
    }

    public enum DespawnActionOnEnd
    {
        Deactivate,
        Destroy,
        None
    }

    [Header("Spawn Params")]
    [SerializeField]
    GameObject _prefab;

    [Header("SpawnPosition")]
    [SerializeField]
    private TargetType _spawnWhere;
    [SerializeField]
    private bool _parentToTarget;

    [Header("SpawnRotation")]
    [SerializeField]
    private SpawnRotationType _spawnRotationType;

    [SerializeField]
    private TargetType _rotationReference;
    [SerializeField]
    private bool _referenceXPosForLookAt = true;
    [SerializeField]
    private bool _referenceYPosForLookAt = true;
    [SerializeField]
    private bool _referenceZPosForLookAt = true;

    [Header("DespawnAction")]
    [SerializeField]
    private DespawnActionOnEnd _despawnActionOnEndInteraction;

    private List<GameObject> _spawnedGameObjects = new List<GameObject>();
    int _activeCount = 0;

    public void StartInteract(GameObject caller, GameObject target)
    {
        GameObject newSpawned = null;

        if (_activeCount >= _spawnedGameObjects.Count)
        {
            newSpawned = GameObject.Instantiate(_prefab);
            _spawnedGameObjects.Add(newSpawned);
            ++_activeCount;
        }
        else
        {
            newSpawned = _spawnedGameObjects[_activeCount];
            ++_activeCount;
            newSpawned.SetActive(true);
        }


        switch (_spawnWhere)
        {
            case TargetType.Caller:
                newSpawned.transform.position = caller.transform.position;
                if (_parentToTarget)
                {
                    newSpawned.transform.SetParent(caller.transform, true);
                }
                break;
            case TargetType.Target:
                newSpawned.transform.position = target.transform.position; 
                
                if (_parentToTarget)
                {
                    newSpawned.transform.SetParent(target.transform, true);
                }
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

        Vector3 targetLookAtPos = Vector3.zero;
        Vector3 calculatedVector = Vector3.zero;

        switch (_spawnRotationType)
        {
            case SpawnRotationType.CopyRotationReference:
                newSpawned.transform.rotation = rotationReference.transform.rotation;
                break;
            case SpawnRotationType.LookAtRotationReference:

                calculatedVector = rotationReference.transform.position - newSpawned.transform.position;

                if (_referenceXPosForLookAt)
                {
                    targetLookAtPos.x = calculatedVector.x;
                }
                if (_referenceYPosForLookAt)
                {
                    targetLookAtPos.y = calculatedVector.y;
                }
                if (_referenceZPosForLookAt)
                {
                    targetLookAtPos.z = calculatedVector.z;
                }

                newSpawned.transform.rotation = Quaternion.LookRotation(
                    targetLookAtPos.normalized,
                    Vector3.up);
                break;
            default:
                break;
        }

    }

    public void EndInteract(GameObject caller, GameObject target)
    {

        switch (_despawnActionOnEndInteraction)
        {
            case DespawnActionOnEnd.Deactivate:

                foreach (var item in _spawnedGameObjects)
                {
                    item.SetActive(false);
                }

                _activeCount = 0;

                break;
            case DespawnActionOnEnd.Destroy:
                
                foreach (var item in _spawnedGameObjects)
                {
                    GameObject.Destroy(item);
                }

                _activeCount = 0;
                break;
            case DespawnActionOnEnd.None:
                break;
            default:
                break;
        }
        
    }
}
