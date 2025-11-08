#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public class ResetTransform
{
    [MenuItem("Tools/Reset Transform &r")]
    static void ClearConsole()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection.Length < 1) return;
        Undo.RegisterCompleteObjectUndo(selection, "Zero Position");
        foreach (GameObject go in selection)
        {
            InternalZeroPosition(go);
            InternalZeroRotation(go);
            InternalZeroScale(go);
        }
    }
    private static void InternalZeroPosition(GameObject go)
    {
        go.transform.localPosition = Vector3.zero;
    }
    private static void InternalZeroRotation(GameObject go)
    {
        go.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    private static void InternalZeroScale(GameObject go)
    {
        go.transform.localScale = Vector3.one;
    }
}
#endif