using System;
using UnityEngine;

[Serializable]
public class SessionActivitySelection
{
    public string type;
    public int order;
    public string status;
}

[Serializable]
public class SessionContextData
{
    public string patientId;
    public string sessionId;
    public string realtimePath;
    public SessionActivitySelection[] activities;
}

public static class SessionContext
{
    private const string PlayerPrefsKey = "TriHealSessionContext";

    public static SessionContextData Current { get; private set; }

    public static bool HasSession =>
        Current != null && !string.IsNullOrEmpty(Current.sessionId);
    
    public static event Action Changed;

    public static void Save(
        string patientId,
        string sessionId,
        string realtimePath,
        SessionActivitySelection[] activities
    )
    {
        Current = new SessionContextData
        {
            patientId = patientId,
            sessionId = sessionId,
            realtimePath = realtimePath,
            activities = activities ?? new SessionActivitySelection[0]
        };
        

        PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(Current));
        PlayerPrefs.Save();

        Debug.Log(
            $"[SessionContext] Saved session={sessionId}, activities={Current.activities.Length}"
        );

        Changed?.Invoke();
    }

    public static bool Load()
    {
        if (HasSession)
            return true;

        string json = PlayerPrefs.GetString(PlayerPrefsKey, "");

        if (string.IsNullOrEmpty(json))
            return false;

        try
        {
            Current = JsonUtility.FromJson<SessionContextData>(json);
            return HasSession;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[SessionContext] Failed to restore context: {exception.Message}"
            );

            Clear();
            return false;
        }
    }

    public static bool HasActivity(string activityType)
    {
        if (!HasSession || Current.activities == null)
            return false;

        foreach (SessionActivitySelection activity in Current.activities)
        {
            if (activity != null && activity.type == activityType)
                return true;
        }

        return false;
    }

    public static void Clear()
    {
        Current = null;
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}