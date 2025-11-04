using UnityEngine;

[CreateAssetMenu(fileName = "InteractionType", menuName = "Scriptable Objects/InteractionType")]
public class InteractionType : ScriptableObject
{
    public enum WhoToInteractWith
    {
        AllTargets,
        OnlyNewOrLastAddedTarget,
        AllTargetsExceptNewOrLastAdded,
        OnlyWithSelfAsTarget
    }
    public enum WhenToCallStartAction
    {
        NoAutomaticCall,
        OnTriggerOrCollisionEnter,
        OnTargetNumberEqualRegisteredPlayerNumber
    }

    public enum WhenToRemoveTargets
    {
        AfterDuration,
        OnCollisionAndTriggerExit
    }

    public enum WhatHappensToCallOnTargetRecall
    {
        IgnoreCall,
        RestartInteraction,
        RecallFirstInteractionsWithoutRestarting, 
        RecallCurrentInteractionsInSequenceWithoutRestarting,
        RecallCurrentInteractionsInSequenceAndResetItsTime,
    }

    [Header("Target Settings")]
    public bool _getTargetsOnTriggerOrCollisionEnter;

    [Header("Automatic Start Call Settings")]
    [Tooltip("If you choose OnReachNumberOfRegisteredPlayers, you have to call RegisterPlayer from somewhere")]
    public WhenToCallStartAction _whenToCallStartAction;
    public WhoToInteractWith _whoToCallStartActionOn;

    [Header("End Call Settings"), Tooltip("EndInteraction is called when target is removed")]
    public WhenToRemoveTargets _whenToRemoveTargets;

    [Header("What Happens If You Call Start Interaction On A Target That Already Started")]
    public WhatHappensToCallOnTargetRecall _whatHappensToTheCallOnTargetRecall;
}
