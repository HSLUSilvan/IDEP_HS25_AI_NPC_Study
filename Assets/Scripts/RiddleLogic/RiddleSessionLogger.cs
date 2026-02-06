using System;
using System.IO;
using System.Text;
using UnityEngine;

public class RiddleSessionLogger
{
    private readonly StringBuilder _sb = new StringBuilder();
    private readonly string _path;

    public RiddleSessionLogger(string sessionId = null)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        _path = Path.Combine(
            Application.persistentDataPath,
            $"riddle_session_{sessionId}.log"
        );

        WriteHeader();
    }

    private void WriteHeader()
    {
        _sb.AppendLine("=== RIDDLE SESSION START ===");
        _sb.AppendLine($"Time: {DateTime.Now:O}");
        _sb.AppendLine($"Unity Version: {Application.unityVersion}");
        _sb.AppendLine();
        Flush();
    }

    public void LogRiddle(RiddleDefinition riddle)
    {
        _sb.AppendLine("=== RIDDLE ===");
        _sb.AppendLine($"ID: {riddle.id}");
        _sb.AppendLine($"Question: {riddle.question}");
        _sb.AppendLine($"AcceptanceCriteria: {riddle.acceptanceCriteria}");
        _sb.AppendLine($"Hint: {riddle.hint}");
        _sb.AppendLine();
        Flush();
    }

    public void LogPlayer(string text)
    {
        _sb.AppendLine($"[PLAYER] {text}");
        Flush();
    }

    public void LogDoor(string text)
    {
        _sb.AppendLine($"[DOOR] {text}");
        Flush();
    }

    public void LogJudge(JudgeResponse r)
    {
        _sb.AppendLine("[JUDGE]");
        _sb.AppendLine($"  solved: {r.solved}");
        _sb.AppendLine($"  confidence: {r.confidence}");
        _sb.AppendLine($"  reason: {r.reason}");
        _sb.AppendLine();
        Flush();
    }

    public void LogError(string error)
    {
        _sb.AppendLine($"[ERROR] {error}");
        Flush();
    }

    private void Flush()
    {
        try
        {
            File.WriteAllText(_path, _sb.ToString());
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to write riddle session log: " + e.Message);
        }
    }

    public string FilePath => _path;
}
