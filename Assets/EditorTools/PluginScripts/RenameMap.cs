using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(RenameMap))]
public class RenameMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RenameMap renameMap = (RenameMap)target;
        base.OnInspectorGUI();
        GUILayout.Space(10);
        if (GUILayout.Button("RENAME"))
        {
            renameMap.Execute();
        }
        EditorUtility.SetDirty(target);
    }
}
#endif
public class RenameMap : MonoBehaviour
{
    public ERenameMap renameType;
    public string fileType = "prefab";
    public string nameFile;
    public string pathName;
    public Vector2 range;
    public int step;

    public void Execute()
    {
        if (renameType == ERenameMap.Increase)
        {
            for (int index = (int)range.y; index >= (int)range.x; index--)
            {
                string oldPath = pathName + "/" + nameFile + index + "." + fileType;
                int resIndex = index + step;
                string newPath = pathName + "/" + nameFile + resIndex + "." + fileType;
                Debug.LogError(pathName + "/" + nameFile + resIndex + "." + fileType);
                System.IO.File.Move(oldPath, newPath);
            }
        }
        else
        {
            for (int index = (int)range.x; index <= (int)range.y; index++)
            {
                string oldPath = pathName + "/" + nameFile + index + "." + fileType;
                int resIndex = index - step;
                string newPath = pathName + "/" + nameFile + resIndex + "." + fileType;
                Debug.LogError(pathName + "/" + nameFile + resIndex + "." + fileType);
                System.IO.File.Move(oldPath, newPath);
            }
        }

    }
}

public enum ERenameMap
{
    Increase,
    Reduce
}