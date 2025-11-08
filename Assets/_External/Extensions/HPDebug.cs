#define ENABLE_HP_LOG
using System.Collections.Generic;
using UnityEngine;

public static class HPDebug
{
    public static void Log(object _log, Object _object = null)
    {
#if UNITY_EDITOR && ENABLE_HP_LOG
        Debug.Log($"<color=green>HPLog_</color>" + _log, _object);
#endif
    }
    public static void LogCheck(object _log, Object _object = null)
    {
        Debug.Log($"<color=teal>HPLogCheck_</color>" + _log, _object);
    }
    public static void LogWarning(object _log)
    {
#if UNITY_EDITOR && ENABLE_HP_LOG
        Debug.Log($"<color=yellow>HPLogWarning_</color>" + _log);
#endif
    }
    public static void LogError(object _log, Object _object = null)
    {
#if UNITY_EDITOR && ENABLE_HP_LOG
        Debug.Log($"<color=red>HPLogError_</color>" + _log, _object);
#endif
    }

    //------------------------------------------------------------------------------------------------//
    public static void TempLog(object _log)
    {
#if UNITY_EDITOR && ENABLE_HP_LOG
        Debug.Log($"<color=green>HPTempLog_</color>" + _log);
#endif
    }
}
