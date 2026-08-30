using System;
using UnityEngine;

internal static class DebugLog
{
    private static string m_timestamp
    {
        get
        {
            return DateTime.Now.ToString("HH:mm:ss.ffffff");
        }
    }
    
    public static void ForceLog(Type tag, string msg)
    {
        var message = m_timestamp + ": [" + tag + "] " + msg;
        Debug.Log(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Log(Type tag, string msg)
    {
        var message = m_timestamp + ": [" + tag.Name + "] " + msg;
        Debug.Log(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Log(object msg)
    {
        var message = m_timestamp + ": " + msg;
        Debug.Log(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Log(string tag, string msg)
    {
        var message = m_timestamp + ": [" + tag + "] " + msg;
        Debug.Log(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Log(string tag, string msg, string hexColor)
    {
        var message = m_timestamp + ": [" + tag + "] " + msg;
        Debug.Log(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Log(string tag, string msg, LogPlatform platform)
    {
        var message = m_timestamp + ": [" + tag + "] " + msg;
        Debug.Log(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Log(Type tag, string msg, LogPlatform platform)
    {
        var message = m_timestamp + ": [" + tag.Name + "] " + msg;
        Debug.Log(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Error(object msg)
    {
        var message = m_timestamp + ": " + msg;
        Debug.LogError(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Error(Type tag, string msg)
    {
        var message = m_timestamp + ": [" + tag.Name + "] " + msg;
        Debug.LogError(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Error(string tag, string msg)
    {
        var message = m_timestamp + ": [" + tag + "] " + msg;
        Debug.LogError(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Error(Type tag, string msg, LogPlatform platform)
    {
        var message = m_timestamp + ": [" + tag.Name + "] " + msg;
        Debug.LogError(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Error(string tag, string msg, LogPlatform platform)
    {
        var message = m_timestamp + ": [" + tag + "] " + msg;
        Debug.LogError(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void ForceWarn(Type tag, string msg)
    {
        var message = m_timestamp + ": [" + tag + "] " + msg;
        Debug.LogWarning(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Warn(Type tag, string msg, LogPlatform platform)
    {
        var message = m_timestamp + ": [" + tag.Name + "] " + msg;
        Debug.LogWarning(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Warn(string tag, string msg, LogPlatform platform)
    {
        var message = m_timestamp + ": [" + tag + "] " + msg;
        Debug.LogWarning(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Warn(Type tag, string msg)
    {
        var message = m_timestamp + ": [" + tag.Name + "] " + msg;
        Debug.LogWarning(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Warn(string tag, string msg)
    {
        var message = m_timestamp + ": [" + tag + "] " + msg;
        Debug.LogWarning(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }

    public static void Warn(object msg)
    {
        var message = m_timestamp + ": " + msg;
        Debug.LogWarning(message);
#if UNITY_IOS
        PrintToSystemService.PrintToSystem.Print(message);
#endif
    }
}
