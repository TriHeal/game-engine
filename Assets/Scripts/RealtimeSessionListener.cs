using System;
using System.Collections;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

[Serializable]
public class RocksBreakFlowDetails
{
    public string eventTitle;
    public string[] thoughts;
    public string[] facts;
}

[Serializable]
public class LiveActivity
{
    public string id;
    public string activityType;
    public string status;
    public string sessionId;
    public string patientId;
    public RocksBreakFlowDetails details;
}

public class RealtimeSessionListener : MonoBehaviour
{
    private static RealtimeSessionListener instance;

    public static event Action ActivityChanged;

    public static string ActiveActivityId { get; private set; }
    public static string ActiveActivityType { get; private set; }

    public static LiveActivity CurrentActivity { get; private set; }

    public static RocksBreakFlowDetails CurrentRocksDetails =>
        CurrentActivity?.details;

    private DatabaseReference currentActivityReference;

    public static bool IsActivityActive(string activityType)
    {
        return !string.IsNullOrEmpty(activityType) &&
               ActiveActivityType == activityType;
    }

    private static void SetActiveActivity(LiveActivity activity)
    {
        bool isActive =
            activity != null &&
            activity.status == "active";

        CurrentActivity = isActive ? activity : null;
        ActiveActivityId = isActive ? activity.id : null;
        ActiveActivityType = isActive ? activity.activityType : null;

        Debug.Log(
            $"[RealtimeSession] Active activity: " +
            $"id={ActiveActivityId ?? "none"}, " +
            $"type={ActiveActivityType ?? "none"}, " +
            $"title={CurrentActivity?.details?.eventTitle ?? "none"}, " +
            $"thoughts={CurrentActivity?.details?.thoughts?.Length ?? 0}, " +
            $"facts={CurrentActivity?.details?.facts?.Length ?? 0}"
        );

        ActivityChanged?.Invoke();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(WaitForSessionAndStartListening());
    }

    private IEnumerator WaitForSessionAndStartListening()
    {
        Debug.Log("[RealtimeSession] Waiting for session context.");

        while (!SessionContext.Load())
        {
            yield return new WaitForSeconds(0.25f);
        }

        Debug.Log(
            $"[RealtimeSession] Session context found: {SessionContext.Current.sessionId}"
        );

        FirebaseAuth firebaseAuth;

        try
        {
            firebaseAuth = FirebaseAuth.DefaultInstance;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[RealtimeSession] Firebase Auth initialization failed: {exception.Message}"
            );

            yield break;
        }

        while (firebaseAuth.CurrentUser == null)
        {
            yield return new WaitForSeconds(0.25f);
        }

        StartListening();
    }

    private void StartListening()
    {
        string realtimePath = SessionContext.Current.realtimePath;

        if (string.IsNullOrEmpty(realtimePath))
        {
            Debug.LogWarning(
                "[RealtimeSession] Session context has no realtimePath."
            );

            return;
        }

        currentActivityReference = FirebaseDatabase.DefaultInstance
            .GetReference($"{realtimePath}/currentActivity");

        currentActivityReference.ValueChanged += OnCurrentActivityChanged;

        Debug.Log(
            $"[RealtimeSession] Listening to {realtimePath}/currentActivity"
        );
    }

    private void OnCurrentActivityChanged(
        object sender,
        ValueChangedEventArgs eventArgs
    )
    {
        if (eventArgs.DatabaseError != null)
        {
            Debug.LogError(
                $"[RealtimeSession] Database error: {eventArgs.DatabaseError.Message}"
            );

            return;
        }

        if (!eventArgs.Snapshot.Exists)
        {
            Debug.Log("[RealtimeSession] No active activity.");

            SetActiveActivity(null);
            return;
        }

        string json = eventArgs.Snapshot.GetRawJsonValue();

        if (string.IsNullOrEmpty(json))
        {
            SetActiveActivity(null);
            return;
        }

        LiveActivity activity;

        try
        {
            activity = JsonUtility.FromJson<LiveActivity>(json);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[RealtimeSession] Failed to parse activity: {exception.Message}"
            );

            return;
        }

        Debug.Log(
            $"[RealtimeSession] Activity changed: id={activity.id}, type={activity.activityType}, status={activity.status}"
        );

        SetActiveActivity(activity);
    }

    private void OnDestroy()
    {
        if (currentActivityReference != null)
        {
            currentActivityReference.ValueChanged -=
                OnCurrentActivityChanged;
        }

        if (instance == this)
        {
            instance = null;
            SetActiveActivity(null);
        }
    }
}