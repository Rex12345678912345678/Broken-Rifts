/*
 * MADE BY LERPMCGERK
 * Simple editor script to replace prop meshes from CombinedScene:whatever to their corresponding mesh file if found.
 * This works by searching finding mesh with the same name as the prop, and setting the mesh filter to use that one instead: note that sometimes incorrect meshes will be found do to there being multiple with the same name, or the one in the prop has never been marked non-static
 * Available Options:
 * 1. Propify Scene: Every single game object with the word "Prop" and has a mesh filter will attempt to update the mesh to the corresponding mesh. Found under Propify/Propify Scene
 * 2. Propify Selection: Only attempts to find and set the mesh of the selection that has "Prop" in its name. Found under Propify/Propify Selection AND GameObject/Propify/Propify Selection
 * 3. Propify Entire Scene: Attempts to find and set the mesh of every single mesh filter whether the name contains "Prop" or not.  This is kind of risky so save your scene and make a backup first! Found under Propify/Propify Entire Scene
 * 4. Propify Entire Selection: Attempts to find and set the mesh of every single mesh filter under the selected game objects whether the name contains "Prop" or not.  This is kind of risky so save your scene and make a backup 
 * Found under GameObjects/Propify Entire Selection AND Propify/Propify Entire Selection
 * 5. Propify Entire Selected: Finds and sets the mesh of the active selected game object. Found under Propify/Propify Selected AND GameObject/Propify/Propify Selected
 * 6. Log Actions: Whether to log which meshes successfully and which meshes failed. Found under Propify/Log Actions
 * 7. Create Backup: Puts all changed game objects into a GameObject called _Backup_PropifyTime if enabled. This is recommended and is enabled by default, found under Propify/Create Backup
 * 8. Blacklist: Blacklists objects from getting propified, adds/removes the current selection. Ignored by Propfiy/Propify Selected, Found under Propify/Blacklist 
 * If there are any bugs, issues, suggestions, or any questions feel free to ask me in the Angry Birds Epic Central Discord server
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class Propify
{
    static Propify()
    {
        LogProgress = EditorPrefs.GetBool(ToggleLogProgressPath);
        Backups = !EditorPrefs.HasKey(ToggleBackupsPath) || EditorPrefs.GetBool(ToggleBackupsPath);
        Blacklist = JsonConvert.DeserializeObject<List<string>>(EditorPrefs.GetString("Propify/Blacklist"));
        Blacklist ??= new List<string>();
    }
    
    private const string ToggleLogProgressPath = "Propify/Log Actions";
    
    public static bool LogProgress;
    
    private const string ToggleBackupsPath = "Propify/Create Backup";
    
    public static readonly List<string> Blacklist;
    
    public static bool Backups = true;

    private static Transform currentParent;
    
    public static bool PropifyFilter(MeshFilter filter, List<Object> changedList = null, bool ignoreBlacklist = false)
    {
        if (Blacklist.Contains(filter.name) && !ignoreBlacklist) return false;
        if (filter.GetComponent<CHMeshSprite>()) return false;
        
        if (Backups && currentParent != null)
        {
            EditorUtility.SetDirty(Object.Instantiate(filter.gameObject, filter.transform.position, filter.transform.rotation, currentParent));
        }
        
        var meshGuids = AssetDatabase.FindAssets(filter.name); // FirstOrDefault to avoid null references
        var oldMeshId = filter.sharedMesh.GetInstanceID();
        Mesh mesh = null;
        foreach (var guid in meshGuids)
        {
            mesh =  AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(guid));
            if (mesh)
            {
                break;
            }
        }
        if (mesh == null)
        {
            throw new Exception($"Mesh {filter.name} doesn't exist!");
        }
        
        
        if (oldMeshId != mesh.GetInstanceID())
        {
            if (changedList != null)
            {
                changedList.Add(filter);
            }
            else
            {
                Undo.RecordObject(filter, "Propify");
            }
            filter.sharedMesh = mesh;
            EditorUtility.SetDirty(filter);
            return true;
        }
        return false;
    }

    [MenuItem("Propify/Print Blacklist")]
    public static void PrintBlacklist()
    {
        foreach (var item in Blacklist)
        {
            Debug.Log($"[Propify] {item}");
        }
    }
    
    [MenuItem("GameObject/Propify/Blacklist")]
    [MenuItem("Propify/Blacklist")]
    public static void AddRemoveBlacklist()
    {
        foreach (string propName in Selection.gameObjects.Select(go => go.name))
        {
            if (Blacklist.Contains(propName))
            {
                Blacklist.Remove(propName);
                
                Debug.Log($"[Propify] Removed {propName} from the blacklist!");
            }
            else
            {
                Blacklist.Add(propName);
                Debug.Log($"[Propify] Added {propName} to the blacklist!");
            }
        }
        
        EditorPrefs.SetString("Propify/Blacklist", JsonConvert.SerializeObject(Blacklist));
    }

    [MenuItem("Propify/Propify Scene")]
    public static void PropifyScene()
    {
        Debug.Log($"[Propify] Propifying...");

        if (Backups)
        {
            currentParent =  new GameObject("_Backup_" + DateTimeOffset.Now.ToUnixTimeSeconds()).transform;
            currentParent.gameObject.SetActive(false);
        }
        
        List<Object> changed = new List<Object>();
        
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            try
            {
                MeshFilter filter = go.GetComponent<MeshFilter>();
                if (filter && filter.name.Contains("Prop"))
                {
                    PropifyFilter(filter, changed);
                }

                if (LogProgress)
                {
                    Debug.Log($"[Propify] Successfully propified {go.name}!");
                }
            }
            catch (Exception e)
            {
                if (LogProgress)
                {
                    Debug.LogError($"[Propify] Failed to propify {go.name}!");
                    Debug.LogException(e);
                }
            }
        }

        if (changed.Count > 0)
        {
            Undo.RegisterCompleteObjectUndo(changed.ToArray(), "Propified");
        }
    }
    [MenuItem("Propify/Propify Entire Scene")]
    public static void PropifyEntireScene()
    {
        Debug.Log($"[Propify] Propifying...");
        
        if (Backups)
        {
            currentParent =  new GameObject("_Backup_" + DateTimeOffset.Now.ToUnixTimeSeconds()).transform;
            currentParent.gameObject.SetActive(false);
        }
        
        List<Object> changed = new List<Object>();
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            try
            {
                MeshFilter filter = go.GetComponent<MeshFilter>();
                if (filter)
                {
                    PropifyFilter(filter, changed);
                }

                if (LogProgress)
                {
                    Debug.Log($"[Propify] Successfully propified {go.name}!");
                }
            }
            catch (Exception e)
            {
                if (LogProgress)
                {
                    Debug.LogError($"[Propify] Failed to propify {go.name}!");
                    Debug.LogException(e);
                }
            }
        }
        if (changed.Count > 0)
        {
            Undo.RegisterCompleteObjectUndo(changed.ToArray(), "Propified");
        }
    }
    [MenuItem("Propify/Propify Selection")]
    [MenuItem("GameObject/Propify/Propify Selection")]
    public static void PropifySelection()
    {
        if (Backups)
        {
            currentParent =  new GameObject("_Backup_" + DateTimeOffset.Now.ToUnixTimeSeconds()).transform;
            currentParent.gameObject.SetActive(false);
        }
        
        Debug.Log($"[Propify] Propifying...");
        List<Object> changed = new List<Object>();
        foreach (GameObject go in Selection.gameObjects)
        {
            try
            {
                MeshFilter filter = go.GetComponent<MeshFilter>();
                if (filter && filter.name.Contains("Prop"))
                {
                    PropifyFilter(filter, changed);
                }

                if (LogProgress)
                {
                    Debug.Log($"[Propify] Successfully propified {go.name}!");
                }
            }
            catch (Exception e)
            {
                if (LogProgress)
                {
                    Debug.LogError($"[Propify] Failed to propify {go.name}!");
                    Debug.LogException(e);
                }
            }
        }
        if (changed.Count > 0)
        {
            Undo.RegisterCompleteObjectUndo(changed.ToArray(), "Propified");
        }
    }
    [MenuItem("Propify/Propify Entire Selection")]
    [MenuItem("GameObject/Propify/Propify Entire Selection")]
    public static void PropifyEntireSelection()
    {
        if (Backups)
        {
            currentParent =  new GameObject("_Backup_" + DateTimeOffset.Now.ToUnixTimeSeconds()).transform;
            currentParent.gameObject.SetActive(false);
        }
        
        Debug.Log($"[Propify] Propifying...");
        List<Object> changed = new List<Object>();
        foreach (GameObject go in Selection.gameObjects)
        {
            try
            {
                MeshFilter filter = go.GetComponent<MeshFilter>();
                if (filter)
                {
                    PropifyFilter(filter, changed);
                }

                if (LogProgress)
                {
                    Debug.Log($"[Propify] Successfully propified {go.name}!");
                }
            }
            catch (Exception e)
            {
                if (LogProgress)
                {
                    Debug.LogError($"[Propify] Failed to propify {go.name}!");
                    Debug.LogException(e);
                }
            }
        }
        if (changed.Count > 0)
        {
            Undo.RegisterCompleteObjectUndo(changed.ToArray(), "Propified");
        }
    }
    [MenuItem("Propify/Propify Selected")]
    [MenuItem("GameObject/Propify/Propify Selected")]
    public static void PropifySelected()
    {
        Debug.Log($"[Propify] Propifying...");
        
        GameObject go = Selection.activeGameObject;
        if (!go)
        {
            return;
        }
        if (Backups)
        {
            currentParent =  new GameObject("_Backup_" + DateTimeOffset.Now.ToUnixTimeSeconds()).transform;
            currentParent.gameObject.SetActive(false);
        }
        try
        {
            MeshFilter filter = go.GetComponent<MeshFilter>();
            if (filter)
            {
                PropifyFilter(filter, null, true);
            }

            if (LogProgress)
            {
                Debug.Log($"[Propify] Successfully propified {go.name}!");
            }
        }
        catch (Exception e)
        {
            if (LogProgress)
            {
                Debug.LogError($"[Propify] Failed to propify {go.name}!");
                Debug.LogException(e);
            }
        }
    }
    
    [MenuItem(ToggleLogProgressPath)]
    private static void ToggleLogProgress()
    {
        LogProgress = !LogProgress;
        EditorPrefs.SetBool(ToggleLogProgressPath, LogProgress);
    }
    [MenuItem(ToggleLogProgressPath, true)]
    private static bool ValidateToggleLogProgress()
    {
        Menu.SetChecked(ToggleLogProgressPath, LogProgress);
        return true;
    }
    [MenuItem(ToggleBackupsPath)]
    private static void ToggleBackups()
    {
        Backups = !Backups;
        EditorPrefs.SetBool(ToggleBackupsPath, Backups);
    }
    [MenuItem(ToggleBackupsPath, true)]
    private static bool ValidateToggleBackups()
    {
        Menu.SetChecked(ToggleBackupsPath, Backups);
        return true;
    }
}
