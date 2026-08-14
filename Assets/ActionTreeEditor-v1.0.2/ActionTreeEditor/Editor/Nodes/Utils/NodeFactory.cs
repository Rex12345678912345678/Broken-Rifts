namespace ActionTreeEditor.Nodes
{
    public static class NodeFactory
    {
        private const bool DO_CLONE = false;
        
        private static ActionNode Clone(ActionNode node)
        {
            return new ActionNode
            {
                editorRect = node.editorRect,
                nodeID = node.nodeID,
                nodesIn = node.nodesIn,
                nodesOut = node.nodesOut,
                nodesOutIndex = node.nodesOutIndex,
                target = node.target,
                text = node.text,
                param = node.param,
                enabled = node.enabled,
                pass = node.pass,
                type = node.type,
                refObject = node.refObject,
                refObject1 = node.refObject1,
                refObject2 = node.refObject2,
                customInt = node.customInt,
                customInt2 = node.customInt2,
                customInt3 = node.customInt3,
                customFloat = node.customFloat,
                customFloat2 = node.customFloat2,
                customBool = node.customBool,
                customBool2 = node.customBool2,
                objectType = node.objectType,
                objectName = node.objectName,
                customVec1 = node.customVec1,
                customVec2 = node.customVec2,
                QueueIdle = node.QueueIdle,
                secondaryText = node.secondaryText
            };
        }

        public static VisualNode CloneNode(VisualNode node)
        {
            var clonedActionNode = Clone(node.Node);
            clonedActionNode.nodeID = -1;
            
            clonedActionNode.nodesIn = null;
            clonedActionNode.nodesOut = null;
            clonedActionNode.nodesOutIndex = null;
            
            var clonedVisualNode = CreateNode(clonedActionNode);
            return clonedVisualNode;
        }
        
        public static VisualNode CreateNode(ActionNode node)
        {
            return node.type switch
            {
                NodeType.Default              => new DefaultNode(node),
                NodeType.PlayAnimation        => new PlayAnimationNode(node),
                NodeType.Instantiate          => new InstantiateNode(node),
                NodeType.SetPosition          => new SetPositionNode(node),
                NodeType.MoveTo               => new MoveToNode(node),
                NodeType.MoveAlongPath        => new MoveAlongPathNode(node),
                NodeType.Delay                => new DelayNode(node),
                NodeType.SetActive            => new SetActiveNode(node),
                NodeType.Destroy              => new DestroyNode(node),
                NodeType.SetScale             => new SetScaleNode(node),
                NodeType.EnableStorySequence  => new EnableStorySequenceNode(node),
                NodeType.GetWorldMapCharacter => new GetWorldMapCharacterNode(node),
                NodeType.InstantiateCharacter => new InstantiateCharacterNode(node),
                NodeType.PlayBoneAnimation    => new PlayBoneAnimationNode(node),
                NodeType.ZoomCamera           => new ZoomCameraNode(node),
                NodeType.KillBattlePigs       => new KillBattlePigsNode(node),
                NodeType.FindSceneObject      => new FindSceneObjectNode(node),
                NodeType.TimeScale            => new TimeScaleNode(node),
                NodeType.SetParent            => new SetParentNode(node),
                NodeType.SetRotation          => new SetRotationNode(node),
                NodeType.PlaySound            => new PlaySoundNode(node),
                NodeType.FindObject           => new FindObjectNode(node),
                NodeType.InstatiateProp       => new InstantiatePropNode(node),
                NodeType.SetBirdsToHotspot    => new SetBirdsToHotspotNode(node),
                NodeType.FindObjectByTag      => new FindObjectByTagNode(node),
                
                // you just HAD to ruin my nice formatting
                NodeType.FinishCurrentChronicleCave => new FinishCurrentChronicleCaveNode(node),
                
                _ => new UnImplementedNode(node)
            };
        }
    }
}