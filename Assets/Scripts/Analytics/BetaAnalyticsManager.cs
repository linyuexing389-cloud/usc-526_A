using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Session outcomes: level completion (win/loss per attempt) and time per level (seconds until outcome).
/// Logged to session_outcomes.csv; mirrored in memory for end-screen stats.
/// </summary>
public class BetaAnalyticsManager : MonoBehaviour
{
    public static BetaAnalyticsManager Instance { get; private set; }

    [Header("Files (under persistentDataPath)")]
    public string sessionCsvFileName = "session_outcomes.csv";

    private string _sessionPath;
    private float _levelStartRealtime;

    private readonly List<SessionRecord> _sessions = new List<SessionRecord>();

    private struct SessionRecord
    {
        public string SceneName;
        public bool Won;
        public float DurationSec;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WebGLDownloadTextFile(string filename, string text);
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        _sessionPath = Path.Combine(Application.persistentDataPath, sessionCsvFileName);

        try
        {
            if (!File.Exists(_sessionPath))
                File.WriteAllText(_sessionPath, "level,sceneName,outcome,durationSec,timestamp\n");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[BetaAnalytics] File init: " + e.Message);
        }
    }

    public void BeginLevelSession()
    {
        _levelStartRealtime = Time.realtimeSinceStartup;
    }

    public void LogSessionEnd(bool won)
    {
        var scene = SceneManager.GetActiveScene();
        float duration = Mathf.Max(0f, Time.realtimeSinceStartup - _levelStartRealtime);
        string outcome = won ? "Win" : "Loss";
        string ts = DateTime.UtcNow.ToString("o");
        string line = $"{scene.buildIndex},{EscapeCsv(scene.name)},{outcome},{duration:F2},{ts}\n";

        _sessions.Add(new SessionRecord
        {
            SceneName = scene.name,
            Won = won,
            DurationSec = duration
        });

        try
        {
            File.AppendAllText(_sessionPath, line);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[BetaAnalytics] Session write: " + e.Message);
        }
    }

    /// <summary>Duration of the attempt that just ended (seconds).</summary>
    public float GetLastAttemptDurationSeconds()
    {
        if (_sessions.Count == 0) return 0f;
        return _sessions[_sessions.Count - 1].DurationSec;
    }

    /// <summary>Completion rate for this scene: wins / (wins + losses) in the current run.</summary>
    public void GetCompletionStatsForScene(string sceneName, out int wins, out int losses, out float ratePercent)
    {
        wins = losses = 0;
        foreach (var s in _sessions)
        {
            if (s.SceneName != sceneName) continue;
            if (s.Won) wins++;
            else losses++;
        }

        int n = wins + losses;
        ratePercent = n > 0 ? (100f * wins / n) : 0f;
    }

    public void ExportAllAnalytics()
    {
        StartCoroutine(ExportCoroutine());
    }

    private IEnumerator ExportCoroutine()
    {
        TryExportFile("death_log.csv", SafeRead(DeathAnalyticsManager.Instance != null
            ? DeathAnalyticsManager.Instance.CsvAbsolutePath
            : null));
        yield return new WaitForSecondsRealtime(0.35f);
        TryExportFile(sessionCsvFileName, SafeRead(_sessionPath));
    }

    private static string SafeRead(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return "";
        try
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[BetaAnalytics] Read: " + e.Message);
            return "";
        }
    }

    private void TryExportFile(string filename, string contents)
    {
        if (string.IsNullOrEmpty(contents))
            contents = "";

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            WebGLDownloadTextFile(filename, contents);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[BetaAnalytics] WebGL download: " + e.Message);
        }
#else
        try
        {
            string outPath = Path.Combine(Application.persistentDataPath, "export_" + filename);
            File.WriteAllText(outPath, contents, Encoding.UTF8);
            Debug.Log("[BetaAnalytics] Exported: " + outPath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[BetaAnalytics] Export: " + e.Message);
        }
#endif
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(",") || value.Contains("\""))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
