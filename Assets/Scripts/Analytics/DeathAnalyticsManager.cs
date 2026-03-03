using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DeathCause
{
    Timeout,       // 1. Timer ran out
    Spikes,       // 2. Death by touching the red ball
    Trap          // 3. Death by touching the red wall
}

[Serializable]
public class DeathRecord
{
    public int level;
    public string sceneName;
    public string deathCause;
    public float timeRemaining;
    public float hpRemaining;
    public string timestamp;
}

public class DeathAnalyticsManager : MonoBehaviour
{
    public static DeathAnalyticsManager Instance;

    [Header("File Settings")]
    public string jsonFileName = "death_log.jsonl";
    public string csvFileName = "death_log.csv";

    private string jsonPath;
    private string csvPath;
    private readonly List<DeathRecord> records = new List<DeathRecord>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        jsonPath = Path.Combine(Application.persistentDataPath, jsonFileName);
        csvPath = Path.Combine(Application.persistentDataPath, csvFileName);

        try
        {
            if (!File.Exists(csvPath))
                File.WriteAllText(csvPath, "level,sceneName,deathCause,timeRemaining,hpRemaining,timestamp\n");
        }
        catch (Exception e) { Debug.LogWarning("[DeathAnalytics] File init: " + e.Message); }
    }

    public static void LogDeath(DeathCause cause, float timeRemaining, float hpRemaining)
    {
        if (Instance == null)
        {
            var go = new GameObject("DeathAnalyticsManager");
            Instance = go.AddComponent<DeathAnalyticsManager>();
            go.AddComponent<DeathAnalyticsGraphUI>();
        }

        Instance.InternalLogDeath(cause, timeRemaining, hpRemaining);
    }

    private void InternalLogDeath(DeathCause cause, float timeRemaining, float hpRemaining)
    {
        var scene = SceneManager.GetActiveScene();

        var record = new DeathRecord
        {
            level = scene.buildIndex,
            sceneName = scene.name,
            deathCause = cause.ToString(),
            timeRemaining = timeRemaining,
            hpRemaining = hpRemaining,
            timestamp = DateTime.UtcNow.ToString("o")
        };

        records.Add(record);

        try
        {
            var jsonLine = JsonUtility.ToJson(record);
            File.AppendAllText(jsonPath, jsonLine + Environment.NewLine);
            var csvLine =
                $"{record.level},{EscapeForCsv(record.sceneName)},{record.deathCause},{record.timeRemaining:F2},{record.hpRemaining:F2},{record.timestamp}";
            File.AppendAllText(csvPath, csvLine + Environment.NewLine);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[DeathAnalytics] Write failed (OK in WebGL): " + e.Message);
        }

        Debug.Log("[DeathAnalytics] Logged: " + record.deathCause + " (total " + records.Count + ")");
    }

    private string EscapeForCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";

        if (value.Contains(",") || value.Contains("\""))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    public int TotalDeathCount => records.Count;

    public void GetDeathCauseCounts(out int timeout, out int spikes, out int trap)
    {
        timeout = spikes = trap = 0;
        foreach (var r in records)
        {
            switch (r.deathCause)
            {
                case "Timeout": timeout++; break;
                case "Spikes": spikes++; break;
                case "Trap": trap++; break;
            }
        }
    }
}

