
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
public class ApplyPrefab
{
    [MenuItem("Tools/Apply changes to Prefab &a")]
    [Obsolete]
    static void SaveChangesToPrefab()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection.Length < 1) return;
        Undo.RegisterCompleteObjectUndo(selection, "Apply Prefab");
        foreach (GameObject go in selection)
        {
            if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
            {
                PrefabUtility.ReplacePrefab(go, PrefabUtility.GetPrefabParent(go));
                PrefabUtility.RevertPrefabInstance(go);
            }
        }
    }
}
#endif
