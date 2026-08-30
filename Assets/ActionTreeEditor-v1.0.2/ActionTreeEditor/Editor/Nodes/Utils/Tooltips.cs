using System;
using UnityEngine;

namespace ActionTreeEditor.Loca
{
    public static class Tooltips
    {
        public const string EditorVersion = "1.0.2";
        
        public const string WaitModeTooltip = 
            "Whether to wait before continuing to the next node.";
        
        public const string ObjectNameFieldTooltip = 
            "An object name saved from a previous node.";
        
        public const string QueueIdleTooltip = 
            "If a second animation should be queued to play after the current one completes.";
        
        public const string DelaySecondsTooltip = 
            "The delay in seconds before continuing to the next node.";
        
        public const string InstantiateGameObjectParentTooltip = 
            "The object will be parented to a GameObject in the scene.";
        
        public const string InstantiateObjectNameParentTooltip = 
            "The object will be parented to a previously instantiated object with the chosen Object Name.";

        public const string SaveAsObjectNameTooltip = 
            "The object name used to reference this object from other nodes.";
        
        public const string PositionNodeSetParentTooltip = 
            "Whether to set the parent of the current object to the referenced parent object.";
        
        public const string PositionRelativeToParentTransformTooltip = 
            "Set the position to the referenced transform's position + the offset.";
        
        public const string PositionInWorldSpaceTooltip = 
            "Set the global world space position to the supplied position.";
        
        public const string PositionShiftByOffsetTooltip = 
            "Shift the current object's position by the offset.";
        
        public const string PositionRelativeToParentObjectNameTooltip = 
            "Set the position to the referenced object's position + the offset.";
        
        public const string CharacterControllerPrefabTooltip = 
            "The CharacterControllerWorldMap or CharacterControllerCamp prefab.";
        
        public const string CharacterBalancingDataNameIdTooltip = 
            "The NameId in balancing of the character to instantiate. This NameId must be present in Bird, Pig, or Boss BalancingData.";
        
        public const string MoveAnimationTooltip = 
            "The animation to play while the character walks down the path. Default: Move_Loop";
        
        public const string MoveAlongPathWaitForCompletionTooltip =
            "Whether to wait for the character to reach the end hotspot before continuing to the next node.";
        
        public const string BirdGameObjectNameTooltip = 
            "The name of the GameObject of the bird, e.g. RedBird, YellowBird, etc.";
        
        public const string EnableStorySequenceActiveTooltip = 
            "Whether the story sequence is starting or ending. ON == starting, OFF == ending.";
        
        public const string PropAssetNameIdTooltip = 
            "The NameId of the prop to instantiate. This NameId must be present in PropLiteAssetProvider in Root.";
        
        public const string ZoomCameraOrthoSizeDelta = 
            "The amount to increase the Camera's orthographic size by. finalSize = startSize + <value>";
        
        public const string ObjectNameCameraTooltip = 
            "A previously saved object name. This object MUST have a Camera component somewhere in its children.";

        public const string NodeWaitConditionTooltip = 
            "The condition to move onto the next node. NextNode == continue instantly, WaitUntilCompleted == wait for the zoom to complete";
        
        public const string FindGameObjectNameTooltip = 
            "The name of the GameObject thats being searched for.";
        
        public const string ResetLocalPositionTooltip = 
            "Whether to set the target's localPosition to Vector3.zero (0, 0, 0).";
        
        public const string PlaySoundSourceGameObjectTooltip = 
            "The GameObject to play the sound from (this can be null). This GameObject must have an AudioSource component.";
        
        public const string PlaySoundStartTimeTooltip = 
            "The position in the sound to start from.";
        
        public const string SoundNameIdTooltip = 
            "The sound NameId. This is the NameId found in the 4_Sounds GameObject in Root.";

        
        public const string PreviewerSingleStepTooltip = 
            "Move forward ONE node. Will not reset your camera position.";
        

        public const string DefaultNodeTypeDesc =
            "";
        
        public const string PlayAnimationNodeTypeDesc =
            "";
        
        public const string InstantiateNodeTypeDesc =
            "";
        
        public const string SetPositionNodeTypeDesc =
            "";
        
        public const string MoveToNodeTypeDesc =
            "";
        
        public const string MoveAlongPathNodeTypeDesc =
            "";
        
        public const string DelayNodeTypeDesc =
            "";
        
        public const string SetActiveNodeTypeDesc =
            "";
        
        public const string DestroyNodeTypeDesc =
            "";
        
        public const string SetScaleNodeTypeDesc =
            "";
        
        public const string EnableStorySequenceNodeTypeDesc =
            "";
        
        public const string GetWorldMapCharacterNodeTypeDesc =
            "";
        
        public const string InstantiateCharacterNodeTypeDesc =
            "";
        
        public const string PlayBoneAnimationNodeTypeDesc =
            "";
        
        public const string ZoomCameraNodeTypeDesc =
            "";
        
        public const string KillBattlePigsNodeTypeDesc =
            "";
        
        public const string FindSceneObjectNodeTypeDesc =
            "";
        
        public const string TimeScaleNodeTypeDesc =
            "";
        
        public const string SetParentNodeTypeDesc =
            "";
        
        public const string SetRotationNodeTypeDesc =
            "";
        
        public const string PlaySoundNodeTypeDesc =
            "";
        
        public const string FindObjectNodeTypeDesc =
            "";
        
        public const string InstatiatePropNodeTypeDesc =
            "";
        
        public const string SetBirdsToHotspotNodeTypeDesc =
            "";
        
        public const string FindObjectByTagNodeTypeDesc =
            "";
        
        public const string FinishCurrentChronicleCaveNodeTypeDesc =
            "";
        
        public static string GetDescriptionForType(NodeType type)
        {
            return type switch
            {
               NodeType.Default              => DefaultNodeTypeDesc,
               NodeType.PlayAnimation        => PlayAnimationNodeTypeDesc,
               NodeType.Instantiate          => InstantiateNodeTypeDesc,
               NodeType.SetPosition          => SetPositionNodeTypeDesc,
               NodeType.MoveTo               => MoveToNodeTypeDesc,
               NodeType.MoveAlongPath        => MoveAlongPathNodeTypeDesc,
               NodeType.Delay                => DelayNodeTypeDesc,
               NodeType.SetActive            => SetActiveNodeTypeDesc,
               NodeType.Destroy              => DestroyNodeTypeDesc,
               NodeType.SetScale             => SetScaleNodeTypeDesc,
               NodeType.EnableStorySequence  => EnableStorySequenceNodeTypeDesc,
               NodeType.GetWorldMapCharacter => GetWorldMapCharacterNodeTypeDesc,
               NodeType.InstantiateCharacter => InstantiateCharacterNodeTypeDesc,
               NodeType.PlayBoneAnimation    => PlayBoneAnimationNodeTypeDesc,
               NodeType.ZoomCamera           => ZoomCameraNodeTypeDesc,
               NodeType.KillBattlePigs       => KillBattlePigsNodeTypeDesc,
               NodeType.FindSceneObject      => FindSceneObjectNodeTypeDesc,
               NodeType.TimeScale            => TimeScaleNodeTypeDesc,
               NodeType.SetParent            => SetParentNodeTypeDesc,
               NodeType.SetRotation          => SetRotationNodeTypeDesc,
               NodeType.PlaySound            => PlaySoundNodeTypeDesc,
               NodeType.FindObject           => FindObjectNodeTypeDesc,
               NodeType.InstatiateProp       => InstatiatePropNodeTypeDesc,
               NodeType.SetBirdsToHotspot    => SetBirdsToHotspotNodeTypeDesc,
               NodeType.FindObjectByTag      => FindObjectByTagNodeTypeDesc,
               
               NodeType.FinishCurrentChronicleCave => FinishCurrentChronicleCaveNodeTypeDesc,
               _ => ""
            };
        }
    }
}