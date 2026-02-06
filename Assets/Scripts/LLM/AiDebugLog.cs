using UnityEngine;

public static class AiDebugLog
{
    public static bool Enabled = true;

    public static void Info(string msg)
    {
        if (!Enabled) return;
        Debug.Log($"[AI][INFO] {msg}");
    }

    public static void Warn(string msg)
    {
        if (!Enabled) return;
        Debug.LogWarning($"[AI][WARN] {msg}");
    }

    public static void Error(string msg)
    {
        Debug.LogError($"[AI][ERROR] {msg}");
    }
}
