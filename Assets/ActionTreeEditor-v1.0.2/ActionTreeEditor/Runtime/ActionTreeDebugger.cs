#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ActionTreeEditor.Runtime
{
    public class ActionTreeDebugger : MonoBehaviour
    {
        public static Func<bool> IsDebuggingFunc;
        public static bool Debugging => IsDebuggingFunc?.Invoke() ?? false;
        
        public static bool Playing { get; set; } = false;
        
        public static ActionTreeDebugger Instance { get; private set; }
        
        private static Dictionary<GlobalObjectId, HashSet<int>> GlobalBreakpoints { get; set; } = new();

        // for use at runtime, will return 'no nodes' if not debugging
        private HashSet<int> ActiveTreeBreakpoints => Debugging ? GetBreakpoints(ActionTreeObjectId) : new HashSet<int>();

        // for use in edit mode, returns all breakpoints for the tree even if not debugging
        private static HashSet<int> GetBreakpoints(GlobalObjectId? treeId)
        {
            if (treeId == null)
                return new HashSet<int>();
            
            if (!GlobalBreakpoints.ContainsKey(treeId.Value))
                GlobalBreakpoints[treeId.Value] = new HashSet<int>();

            return GlobalBreakpoints[treeId.Value];
        }

        private ActionTree m_tree;

        private ActionTree ActionTree
        {
            get => m_tree;
            set
            {
                m_tree = value;
                ActionTreeObjectId = GlobalObjectId.GetGlobalObjectIdSlow(m_tree);
            }
        }
        
        private GlobalObjectId? ActionTreeObjectId { get; set; } = null;
        
        public event Action<int, int> TreeNodeChanged;

        public event Action<int, bool> BreakpointHit;

        public event Action Stopped;
        
        public event Action TreeEndReached;

        private int m_lastNode = int.MinValue;

        private int? m_oldStartNode = null;

        private int m_singleStepTarget = -1;

        public bool IsDebugBroken { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (ActionTree == null)
            {
                Destroy(gameObject);
                return;
            }

            if (!Debugging)
                return;

            if (ActiveTreeBreakpoints.Contains(ActionTree.startNode))
            {
                m_oldStartNode = ActionTree.startNode;
                ActionTree.startNode = -1;
            }

            PreProcessBreakpoints();
        }

        public void PreProcessBreakpoints()
        {
            foreach (var id in ActiveTreeBreakpoints)
            {
                var node = ActionTree.GetNodeByID(id);
                if (node == null)
                    continue;

                // by disabling the node, the ActionTree will halt before the execution of this node
                // we can then continue manually
                node.enabled = false;
            }
        }

        private void CleanupLastSingleStep()
        {
            if (m_singleStepTarget == -1)
                return;

            var node = ActionTree.GetNodeByID(m_singleStepTarget);
            if (node == null || node.enabled)
                return;

            node.enabled = true;
        }

        public void SingleStep()
        {
            if (!IsDebugBroken)
                return;

            CleanupLastSingleStep();

            var nodeToExecuteId = GetNextNode();
            if (nodeToExecuteId == -1)
                return;

            var nodeToExecute = ActionTree.GetNodeByID(nodeToExecuteId);

            ActionNode stopBeforeNode = null;
            
            // get the node to stop before (by setting enabled to false)
            var stopBeforeNodeId = GetNextNode(nodeToExecute);
            if (stopBeforeNodeId != -1) // this is false when nodeToExecute is the last node
            {
                stopBeforeNode = ActionTree.GetNodeByID(stopBeforeNodeId);
                stopBeforeNode.enabled = false;
                
                m_singleStepTarget = stopBeforeNodeId;
            }
            
            try
            {
                // execute the node
                ActionTree.Load(nodeToExecuteId);
            }
            catch (Exception e)
            {
                // I don't think this can throw, but TO BE SAFE we might as well try-catch it
            }
            
            if (stopBeforeNodeId != -1)
                BreakpointHit?.Invoke(stopBeforeNodeId, true);
        }

        public static void ClearBreakpoints(GlobalObjectId? tree)
        {
            GetBreakpoints(tree).Clear();
            
            SaveBreakpointsToPrefs();
        }
        
        public static bool HasBreakpoint(GlobalObjectId? tree, int id)
        {
            return GetBreakpoints(tree).Contains(id);
        }
        
        public static void AddBreakpoint(GlobalObjectId? tree, int id)
        {
            GetBreakpoints(tree).Add(id);
            
            SaveBreakpointsToPrefs();
            
            if (Instance != null)
                Instance.PreProcessBreakpoints();
        }
        
        public static void RemoveBreakpoint(GlobalObjectId? tree, int id)
        {
            if (!HasBreakpoint(tree, id))
                return;
            
            GetBreakpoints(tree).Remove(id);
            
            SaveBreakpointsToPrefs();
            
            if (Instance != null)
                Instance.Internal_RemoveBreakpoint(id);
        }
        
        private void Internal_RemoveBreakpoint(int id)
        {
            ActiveTreeBreakpoints.Remove(id);

            var node = ActionTree.GetNodeByID(id);
            if (node != null)
                node.enabled = true;
        }
        
        // prolly not needed cause its playmode, but whatever
        private void UndoBreakpoints()
        {
            CleanupLastSingleStep();
            
            foreach (var id in ActiveTreeBreakpoints)
            {
                var node = ActionTree.GetNodeByID(id);
                if (node == null)
                    continue;
                
                node.enabled = true;
            }
        }

        public void InitTree(ActionTree tree)
        {
            ActionTree = tree;
        }
        
        private void Update()
        {
            if (!Playing)
                return;
            
            if (m_lastNode == ActionTree.currentNode)
                return;

            var oldNode = m_lastNode;
            m_lastNode = ActionTree.currentNode;
            
            TreeNodeChanged?.Invoke(oldNode, m_lastNode);
            
            var nextNodeId = GetNextNode();
            if (nextNodeId == -1)
            {
                TreeEndReached?.Invoke();
                Playing = false;
                return;
            }

            if (!Debugging)
                return;
            
            if (ActiveTreeBreakpoints.Contains(nextNodeId))
            {
                // restore old start node
                if (nextNodeId == m_oldStartNode)
                    ActionTree.startNode = m_oldStartNode.Value;
                    
                var wasBrokenBefore = IsDebugBroken;
                IsDebugBroken = true;
                BreakpointHit?.Invoke(nextNodeId, wasBrokenBefore);
            }
        }

        public void Break()
        {
            IsDebugBroken = true;
            
            var nextNodeId = GetNextNode();
            if (nextNodeId == -1)
                return;

            var nextNode = ActionTree.GetNodeByID(nextNodeId);
            nextNode.enabled = false;
            
            BreakpointHit?.Invoke(nextNodeId, false);
        }
        
        public void Continue()
        {
            IsDebugBroken = false;
            EditorApplication.isPaused = false;

            var nextNodeId = GetNextNode();
            
            m_singleStepTarget = nextNodeId;
            CleanupLastSingleStep();

            // don't block
            if (nextNodeId != -1)
                StartCoroutine(ResumeTree(nextNodeId));
        }

        private int GetNextNode(ActionNode node = null)
        {
            if (node != null)
                return node.nodesOut is { Length: > 0 } ? node.nodesOut[0] : -1;
            
            if (m_lastNode == -1 && m_oldStartNode != null)
                return m_oldStartNode.Value;
            
            // this happens when it cant find an enabled out node
            if (ActionTree.node == null || ActionTree.node.type == NodeType.Default)
            {
                var currentNode = ActionTree.GetNodeByID(ActionTree.currentNode);
                ActionTree.node = currentNode;
            }

            return ActionTree?.node?.nodesOut is { Length: > 0 } ? ActionTree.node.nodesOut[0] : -1;
        }

        private IEnumerator ResumeTree(int id)
        {
            var nextNode = ActionTree.GetNodeByID(id);
            if (nextNode != null)
                nextNode.enabled = true;
            
            ActionTree.Load(id);
            yield break;
        }

        private void OnDestroy()
        {
            UndoBreakpoints();
            
            Playing = false;
            Stopped?.Invoke();
            
            SaveBreakpointsToPrefs();
        }

        private const string GLOBAL_BREAKPOINTS_KEY = "ActionTreeDBG-GlobalBreakpoints";

        public static void SaveBreakpointsToPrefs()
        {
            var dict = GlobalBreakpoints.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value
            );

            string bps = null;
            try
            {
                bps = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Global breakpoint serialization failed");
                Debug.LogException(e);
                return;
            }
            
            EditorPrefs.SetString(GLOBAL_BREAKPOINTS_KEY, bps);
        }
        
        public static void ReadBreakpointsFromPrefs()
        {
            var bps = EditorPrefs.GetString(GLOBAL_BREAKPOINTS_KEY, null);
            if (bps == null)
                return;

            Dictionary<string, HashSet<int>> dict = null;
            try
            {
                dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, HashSet<int>>>(bps);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Global breakpoint deserialization failed");
                Debug.LogException(e);
                return;
            }
            
            GlobalBreakpoints.Clear();
            
            foreach (var (treeIdString, nodeIds) in dict)
            {
                if (!GlobalObjectId.TryParse(treeIdString, out var treeId))
                    continue;
                
                GlobalBreakpoints[treeId] = nodeIds;
            }
        }
    }
}

#endif