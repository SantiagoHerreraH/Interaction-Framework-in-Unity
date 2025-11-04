using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Linq;

public enum TargetType
{
    Caller,
    Target
}

public interface IStartEndInteract
{
    public void StartInteract(GameObject caller, GameObject target);
    public void EndInteract(GameObject caller, GameObject target);
}

public interface IInteract
{
    public void Interact(GameObject caller, GameObject target);
}

[Serializable]
public class Interaction
{

    [Header("State Modifiers Settings")]
    [SerializeField]
    public float Duration;


    [Header("Start End Interactions")]
    [SerializeReference, SubclassSelector]
    public List<IStartEndInteract> StartEndInteractions = new List<IStartEndInteract>();


    [Header("Interactions")]
    [SerializeReference, SubclassSelector]
    public List<IInteract> StartInteractions = new List<IInteract>();
    [SerializeReference, SubclassSelector]
    public List<IInteract> UpdateInteractions = new List<IInteract>();
    [SerializeReference, SubclassSelector]
    public List<IInteract> FixedUpdateInteractions = new List<IInteract>();
    [SerializeReference, SubclassSelector]
    public List<IInteract> EndInteractions = new List<IInteract>();
}

[Serializable]
public class InteractionData
{
    public string Name;
    public Interaction Interaction;
}

[Serializable]
public class InteractionSequence
{
    public enum WhenToStart
    {
        OnStartInteraction,
        OnReferencedInteractionEnd,
        OnReferencedInteractionStart,
    }

    public WhenToStart WhenToStartSequence;
    public string ReferencedInteraction;
    public List<string> InteractionNamesInSequence = new List<string>();
    [HideInInspector]
    public List<InteractionData> Interactions = new List<InteractionData>();
}

public class InteractionMachine : MonoBehaviour
{
    public class SequenceData
    {
        public int CurrentInteractionIndex;
        public InteractionData CurrentInteraction;
        public float CurrentDuration;
    }

    public class TargetData
    {
        public List<SequenceData> SequenceData = new List<SequenceData>();
        public List<InteractionSequence> Sequences= new List<InteractionSequence>();

    }

    [Header("Actions"), SerializeField]
    List<InteractionData> _interactionData = new List<InteractionData>();

    [Header("InteractionType"), SerializeField]
    InteractionType _interactionType;
    UnityEvent _onAutomaticCallStart = new UnityEvent();

    [Header("When Do The Interactions Happen"), SerializeField]
    List<InteractionSequence> _interactionSequences = new List<InteractionSequence>();

    List<GameObject> _players = new List<GameObject>();
    List<GameObject> _targets = new List<GameObject>();
    List<TargetData> _targetData = new List<TargetData>();

    private void Awake()
    {
        for (int index = 0; index < _interactionData.Count; index++)
        {
            for (int otherIndex = 0; otherIndex < _interactionData.Count; otherIndex++)
            {
                if (index != otherIndex)
                {
                    if (_interactionData[index].Name == _interactionData[otherIndex].Name)
                    {
                        Debug.LogError("same names with name " + _interactionData[index].Name + " in ActionController in gameObject " + gameObject.name);
                    }
                }
            }
        }

        foreach (var interactionSequence in _interactionSequences)
        {
            foreach (var interactionName in interactionSequence.InteractionNamesInSequence)
            {
                bool hasName = false;

                foreach (var interactionData in _interactionData)
                {
                    if (interactionData.Name == interactionName)
                    {
                        interactionSequence.Interactions.Add(interactionData);
                        hasName = true;
                        break;
                    }
                }

                if (!hasName)
                {
                    Debug.LogError("interaction sequence name: " + interactionName + " is not present in interaction data in gameobject " + gameObject.name);
                }
            }
            
        }

    }

    public void RegisterPlayer(PlayerInput playerInput)
    {
        if (!_players.Contains(playerInput.gameObject))
        {
            _players.Add(playerInput.gameObject);
        }
    }

    public void AddTarget(GameObject gameObj)
    {
        if (!_targets.Contains(gameObj))
        {
            _targets.Add(gameObj);
            _targetData.Add(new TargetData());
        }
    }

    public void RemoveTarget(GameObject gameObj)
    {
        if (_targets.Contains(gameObj))
        {
            _targetData.RemoveAt(_targets.IndexOf(gameObj));
            _targets.Remove(gameObj);
        }
    }

    public void RemoveAllTargets()
    {
        _targetData.Clear();
        _targets.Clear();
    }

    public void StartInteraction(GameObject newTarget)
    {
        AddTarget(newTarget);

        StartInteraction_Implementation(newTarget);
    }

    public void StartInteractionWithMainCamera()
    {
        StartInteraction(Camera.main.gameObject);
    }

    public void StartInteractionWithAllTargets()
    {
        foreach (var target in _targets)
        {
            StartInteraction_Implementation(target);
        }
    }

    public void StartInteractionWithLastTarget()
    {
        if (_targets.Count != 0)
        {
            StartInteraction_Implementation(_targets.Last());
        }
    }

    public void StartInteractionWithAllButLastTarget()
    {
        for (int i = 0; i < (_targets.Count - 1); i++)
        {
            StartInteraction_Implementation(_targets[i]);
        }
    }

    private void ResetTargetData(GameObject target)
    {
        if (_targets.Contains(target))
        {
            _targetData[_targets.IndexOf(target)].SequenceData.Clear();
            _targetData[_targets.IndexOf(target)].Sequences.Clear();
        }
    }

    private void StartInteraction_Implementation(GameObject target)
    {
        TargetData currentTargetData = _targetData[_targets.IndexOf(target)];
        bool wasRecalled = currentTargetData.Sequences.Count != 0;

        if (wasRecalled)
        {
            switch (_interactionType._whatHappensToTheCallOnTargetRecall)
            {
                case InteractionType.WhatHappensToCallOnTargetRecall.IgnoreCall:
                    break;
                case InteractionType.WhatHappensToCallOnTargetRecall.RestartInteraction:
                    ResetTargetData(target);
                    RestartInteraction(target);
                    break;
                case InteractionType.WhatHappensToCallOnTargetRecall.RecallFirstInteractionsWithoutRestarting:
                    RecallFirstInteractionsWithoutRestarting(target);
                    break;
                case InteractionType.WhatHappensToCallOnTargetRecall.RecallCurrentInteractionsInSequenceWithoutRestarting:
                    RecallCurrentInteractionsInSequenceWithoutRestarting(target);
                    break;
                case InteractionType.WhatHappensToCallOnTargetRecall.RecallCurrentInteractionsInSequenceAndResetItsTime:
                    RestartCurrentInteractionsInSequence(target);
                    break;
                default:
                    break;
            }
        }
        else
        {
            RestartInteraction(target);
        }
        
    }

    private void RestartCurrentInteractionsInSequence(GameObject target)
    {
        TargetData currentTargetData = _targetData[_targets.IndexOf(target)];

        foreach (var sequenceData in currentTargetData.SequenceData)
        {
            sequenceData.CurrentDuration = 0;
            StartInteraction(sequenceData.CurrentInteraction.Interaction, target);
        }
    }

    private void RecallCurrentInteractionsInSequenceWithoutRestarting(GameObject target)
    {
        TargetData currentTargetData = _targetData[_targets.IndexOf(target)];

        foreach (var sequenceData in currentTargetData.SequenceData)
        {
            StartInteraction(sequenceData.CurrentInteraction.Interaction, target);
        }
    }

    private void RecallFirstInteractionsWithoutRestarting(GameObject target)
    {
        TargetData currentTargetData = _targetData[_targets.IndexOf(target)];

        foreach (var interactionSequence in _interactionSequences)
        {
            if (interactionSequence.WhenToStartSequence == InteractionSequence.WhenToStart.OnStartInteraction)
            {
                StartInteraction(interactionSequence.Interactions[0].Interaction, target);
            }
        }
    }

    private void RestartInteraction(GameObject target)
    {
        TargetData currentTargetData = _targetData[_targets.IndexOf(target)];

        foreach (var interactionSequence in _interactionSequences)
        {
            if (interactionSequence.WhenToStartSequence == InteractionSequence.WhenToStart.OnStartInteraction)
            {
                SequenceData newSequenceData = new SequenceData();
                newSequenceData.CurrentInteraction = interactionSequence.Interactions[0];
                newSequenceData.CurrentInteractionIndex = 0;
                newSequenceData.CurrentDuration = 0;

                currentTargetData.SequenceData.Add(newSequenceData);
                currentTargetData.Sequences.Add(interactionSequence);
                StartInteraction(newSequenceData.CurrentInteraction.Interaction, target);

                TriggerSequencesDependentOnInteractionStart(interactionSequence.InteractionNamesInSequence[0], target);
            }
        }
    }

    private void TriggerSequencesDependentOnInteractionStart(string name, GameObject target)
    {

        TargetData currentTargetData = _targetData[_targets.IndexOf(target)];

        foreach (var interactionSequence in _interactionSequences)
        {
            //if not already started sequence
            if (!currentTargetData.Sequences.Contains(interactionSequence) &&
                (interactionSequence.ReferencedInteraction == name &&
                interactionSequence.WhenToStartSequence == InteractionSequence.WhenToStart.OnReferencedInteractionStart))
            {
                SequenceData newSequenceData = new SequenceData();
                newSequenceData.CurrentInteraction = interactionSequence.Interactions[0];
                newSequenceData.CurrentInteractionIndex = 0;
                newSequenceData.CurrentDuration = 0;

                currentTargetData.SequenceData.Add(newSequenceData);
                currentTargetData.Sequences.Add(interactionSequence);
                StartInteraction(newSequenceData.CurrentInteraction.Interaction, target);

            }
        }
    }

    private void TriggerSequencesDependentOnInteractionEnd(string name, GameObject target)
    {

        TargetData currentTargetData = _targetData[_targets.IndexOf(target)];

        foreach (var interactionSequence in _interactionSequences)
        {
            //if not already started sequence
            if (!currentTargetData.Sequences.Contains(interactionSequence) &&
                (interactionSequence.ReferencedInteraction == name &&
                interactionSequence.WhenToStartSequence == InteractionSequence.WhenToStart.OnReferencedInteractionEnd))
            {
                SequenceData newSequenceData = new SequenceData();
                newSequenceData.CurrentInteraction = interactionSequence.Interactions[0];
                newSequenceData.CurrentInteractionIndex = 0;
                newSequenceData.CurrentDuration = 0;

                currentTargetData.SequenceData.Add(newSequenceData);
                currentTargetData.Sequences.Add(interactionSequence);
                StartInteraction(newSequenceData.CurrentInteraction.Interaction, target);

            }
        }
    }



    private void StartInteraction(Interaction interaction, GameObject target)
    {
        foreach (var item in interaction.StartEndInteractions)
        {
            item.StartInteract(gameObject, target);
        }

        foreach (var item in interaction.StartInteractions)
        {
            item.Interact(gameObject, target);
        }

    }

    private void UpdateInteraction(Interaction interaction, GameObject target)
    {
        foreach (var item in interaction.UpdateInteractions)
        {
            item.Interact(gameObject, target);
        }
    }

    private void FixedUpdateInteraction(Interaction interaction, GameObject target)
    {
        foreach (var item in interaction.FixedUpdateInteractions)
        {
            item.Interact(gameObject, target);
        }
    }

    private void EndInteraction(Interaction interaction, GameObject target)
    {
        foreach (var item in interaction.StartEndInteractions)
        {
            item.EndInteract(gameObject, target);
        }

        foreach (var item in interaction.EndInteractions)
        {
            item.Interact(gameObject, target);
        }
    }

    private void Update()
    {
        InteractionData currentInteractionData = null;
        Interaction currentInteraction = null;
        InteractionSequence currentInteractionSequence = null;
        GameObject currentTarget = null;

        if (_interactionType._whenToRemoveTargets == InteractionType.WhenToRemoveTargets.AfterDuration)
        {
            for (int targetIdx = 0; targetIdx < _targets.Count; targetIdx++)
            {
                for (int sequenceDataIdx = 0; sequenceDataIdx < _targetData[targetIdx].SequenceData.Count; ++sequenceDataIdx)
                {
                    currentInteractionSequence = _targetData[targetIdx].Sequences[sequenceDataIdx];
                    currentInteractionData = _targetData[targetIdx].SequenceData[sequenceDataIdx].CurrentInteraction;
                    currentInteraction = currentInteractionData.Interaction;
                    currentTarget = _targets[targetIdx];

                    ref int currentInteractionIndex = ref _targetData[targetIdx].SequenceData[sequenceDataIdx].CurrentInteractionIndex;
                    ref float currentDuration = ref _targetData[targetIdx].SequenceData[sequenceDataIdx].CurrentDuration;

                    UpdateInteraction(currentInteraction, currentTarget);

                    currentDuration += Time.deltaTime;

                    if (currentDuration >= currentInteraction.Duration)
                    {
                        EndInteraction(currentInteraction, currentTarget);
                        TriggerSequencesDependentOnInteractionEnd(currentInteractionData.Name, currentTarget);
                        currentDuration = 0;

                        //Go to next interaction since this one ended
                        if (currentInteractionIndex < (currentInteractionSequence.Interactions.Count - 1))
                        {
                            ++currentInteractionIndex;
                            currentInteractionData =
                                currentInteractionSequence.Interactions[currentInteractionIndex];
                            _targetData[targetIdx].SequenceData[sequenceDataIdx].CurrentInteraction = currentInteractionData;

                            TriggerSequencesDependentOnInteractionStart(currentInteractionData.Name, currentTarget);
                        }
                        else //reach end of sequence
                        {
                            _targetData[targetIdx].SequenceData.RemoveAt(sequenceDataIdx);
                            _targetData[targetIdx].Sequences.RemoveAt(sequenceDataIdx);
                            --sequenceDataIdx;

                            if (_targetData[targetIdx].Sequences.Count == 0) // end of interaction
                            {
                                --targetIdx;
                                RemoveTarget(currentTarget);
                                break;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            for (int targetIdx = 0; targetIdx < _targets.Count; targetIdx++)
            {
                for (int sequenceDataIdx = 0; sequenceDataIdx < _targetData[targetIdx].SequenceData.Count; ++sequenceDataIdx)
                {
                    currentInteraction = _targetData[targetIdx].SequenceData[sequenceDataIdx].CurrentInteraction.Interaction;
                    currentTarget = _targets[targetIdx];

                    UpdateInteraction(currentInteraction, currentTarget);
                   
                }
            }
        }
    }

    private void FixedUpdate()
    {

        Interaction currentInteraction = null;
        GameObject currentTarget = null;

        for (int targetIdx = 0; targetIdx < _targets.Count; targetIdx++)
        {
            for (int sequenceDataIdx = 0; sequenceDataIdx < _targetData[targetIdx].SequenceData.Count; ++sequenceDataIdx)
            {
                currentInteraction = _targetData[targetIdx].SequenceData[sequenceDataIdx].CurrentInteraction.Interaction;
                currentTarget = _targets[targetIdx];

                FixedUpdateInteraction(currentInteraction, currentTarget);

            }
        }

    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_interactionType._getTargetsOnTriggerOrCollisionEnter)
        {
            AddTarget(collision.gameObject);
        }

        switch (_interactionType._whenToCallStartAction)
        {
            case InteractionType.WhenToCallStartAction.NoAutomaticCall:
                break;
            case InteractionType.WhenToCallStartAction.OnTriggerOrCollisionEnter:

                StartInteractionAutomaticCall();

                break;
            case InteractionType.WhenToCallStartAction.OnTargetNumberEqualRegisteredPlayerNumber:

                if (_players.Count == _targets.Count)
                {
                    StartInteractionAutomaticCall();
                }

                break;
            default:
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_interactionType._getTargetsOnTriggerOrCollisionEnter)
        {
            AddTarget(collision.gameObject);
        }

        switch (_interactionType._whenToCallStartAction)
        {
            case InteractionType.WhenToCallStartAction.NoAutomaticCall:
                break;
            case InteractionType.WhenToCallStartAction.OnTriggerOrCollisionEnter:

                StartInteractionAutomaticCall();

                break;
            case InteractionType.WhenToCallStartAction.OnTargetNumberEqualRegisteredPlayerNumber:

                if (_players.Count == _targets.Count)
                {
                    StartInteractionAutomaticCall();
                }

                break;
            default:
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_interactionType._whenToRemoveTargets == InteractionType.WhenToRemoveTargets.OnCollisionAndTriggerExit)
        {
            RemoveTarget(collision.gameObject);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (_interactionType._whenToRemoveTargets == InteractionType.WhenToRemoveTargets.OnCollisionAndTriggerExit)
        {
            RemoveTarget(collision.gameObject);
        }
    }

    private void StartInteractionAutomaticCall()
    {
        _onAutomaticCallStart.Invoke();

        switch (_interactionType._whoToCallStartActionOn)
        {
            case InteractionType.WhoToInteractWith.AllTargets:
                StartInteractionWithAllTargets();
                break;
            case InteractionType.WhoToInteractWith.OnlyNewOrLastAddedTarget:
                StartInteractionWithLastTarget();
                break;
            case InteractionType.WhoToInteractWith.AllTargetsExceptNewOrLastAdded:
                StartInteractionWithAllButLastTarget();
                break;
            case InteractionType.WhoToInteractWith.OnlyWithSelfAsTarget:
                StartInteraction(gameObject);
                break;
            default:
                break;
        }
    }

    public void EndAllState()
    {

        InteractionData currentInteractionData = null;
        Interaction currentInteraction = null;
        GameObject currentTarget = null;

        for (int targetIdx = 0; targetIdx < _targets.Count; targetIdx++)
        {
            currentTarget = _targets[targetIdx];
            for (int sequenceDataIdx = 0; sequenceDataIdx < _targetData[targetIdx].SequenceData.Count; ++sequenceDataIdx)
            {
                currentInteractionData = _targetData[targetIdx].SequenceData[sequenceDataIdx].CurrentInteraction;
                currentInteraction = currentInteractionData.Interaction;

                EndInteraction(currentInteraction, currentTarget);
                
            }
        }

        RemoveAllTargets();
    }

    private void OnDisable()
    {
        EndAllState();
    }

    private void OnDestroy()
    {
        EndAllState();
    }

}
