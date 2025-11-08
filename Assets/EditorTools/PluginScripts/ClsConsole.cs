#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;

public class ClsConsole
{
    [MenuItem("Tools/Clear Console &s")]
    static void ClearConsole()
    {
        var logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
}
#endif
