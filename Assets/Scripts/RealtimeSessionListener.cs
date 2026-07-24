#if UNITY_EDITOR || UNITY_STANDALONE_OSX
#define TRIHEAL_FIREBASE_REST
#endif

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

#if !TRIHEAL_FIREBASE_REST
using Firebase.Auth;
using Firebase.Database;
#endif

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

    public static string ActiveActivityId
    {
        get;
        private set;
    }

    public static string ActiveActivityType
    {
        get;
        private set;
    }

    public static LiveActivity CurrentActivity
    {
        get;
        private set;
    }

    public static RocksBreakFlowDetails
        CurrentRocksDetails =>
            CurrentActivity?.details;

#if !TRIHEAL_FIREBASE_REST
    private DatabaseReference
        currentActivityReference;
#endif

    private Coroutine restPollingCoroutine;
    private string lastActivityJson;
    private string lastRestError;

    public static bool IsActivityActive(
        string activityType
    )
    {
        return
            !string.IsNullOrEmpty(activityType) &&
            ActiveActivityType == activityType;
    }

    private static void SetActiveActivity(
        LiveActivity activity
    )
    {
        bool isActive =
            activity != null &&
            activity.status == "active";

        CurrentActivity =
            isActive ? activity : null;

        ActiveActivityId =
            isActive ? activity.id : null;

        ActiveActivityType =
            isActive
                ? activity.activityType
                : null;

        Debug.Log(
            $"[RealtimeSession] Active activity: " +
            $"id={ActiveActivityId ?? "none"}, " +
            $"type={ActiveActivityType ?? "none"}, " +
            $"title=" +
            $"{CurrentActivity?.details?.eventTitle ?? "none"}, " +
            $"thoughts=" +
            $"{CurrentActivity?.details?.thoughts?.Length ?? 0}, " +
            $"facts=" +
            $"{CurrentActivity?.details?.facts?.Length ?? 0}"
        );

        ActivityChanged?.Invoke();
    }

    private void Awake()
    {
        if (
            instance != null &&
            instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(
            WaitForSessionAndStartListening()
        );
    }

    private IEnumerator
        WaitForSessionAndStartListening()
    {
        Debug.Log(
            "[RealtimeSession] Waiting for session context."
        );

        while (!SessionContext.Load())
        {
            yield return new WaitForSecondsRealtime(
                0.25f
            );
        }

        Debug.Log(
            "[RealtimeSession] Session context found: " +
            SessionContext.Current.sessionId
        );

#if TRIHEAL_FIREBASE_REST
        while (!FirebaseRestSession.Restore())
        {
            yield return new WaitForSecondsRealtime(
                0.25f
            );
        }

        StartRestPolling();
#else
        FirebaseAuth firebaseAuth;

        try
        {
            firebaseAuth =
                FirebaseAuth.DefaultInstance;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[RealtimeSession] Firebase Auth " +
                "initialization failed: " +
                exception.Message
            );

            yield break;
        }

        while (firebaseAuth.CurrentUser == null)
        {
            yield return new WaitForSecondsRealtime(
                0.25f
            );
        }

        StartNativeListening();
#endif
    }

#if TRIHEAL_FIREBASE_REST
    private void StartRestPolling()
    {
        string realtimePath =
            SessionContext.Current.realtimePath;

        if (string.IsNullOrEmpty(realtimePath))
        {
            Debug.LogWarning(
                "[RealtimeSession] Session context " +
                "has no realtimePath."
            );

            return;
        }

        if (restPollingCoroutine != null)
        {
            StopCoroutine(restPollingCoroutine);
        }

        restPollingCoroutine =
            StartCoroutine(PollCurrentActivity());

        Debug.Log(
            "[RealtimeSession] REST polling started for " +
            $"{realtimePath}/currentActivity"
        );
    }

    private IEnumerator PollCurrentActivity()
    {
        while (true)
        {
            string realtimePath =
                SessionContext.Current?.realtimePath;

            string url =
                FirebaseRestSession
                    .BuildCurrentActivityUrl(
                        realtimePath
                    );

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError(
                    "[RealtimeSession] Cannot build " +
                    "RTDB REST URL. Firebase ID " +
                    "token or realtimePath is missing."
                );

                yield break;
            }

            using (
                UnityWebRequest request =
                    UnityWebRequest.Get(url)
            )
            {
                request.timeout = 10;

                yield return request.SendWebRequest();

                if (
                    request.result ==
                    UnityWebRequest.Result.Success
                )
                {
                    lastRestError = null;

                    ApplyActivityJson(
                        request.downloadHandler.text
                    );
                }
                else
                {
                    string error =
                        $"HTTP {request.responseCode}: " +
                        $"{request.error} / " +
                        $"{request.downloadHandler.text}";

                    if (error != lastRestError)
                    {
                        Debug.LogError(
                            "[RealtimeSession] RTDB REST " +
                            "read failed: " +
                            error
                        );

                        lastRestError = error;
                    }
                }
            }

            yield return new WaitForSecondsRealtime(
                1f
            );
        }
    }
#else
    private void StartNativeListening()
    {
        string realtimePath =
            SessionContext.Current.realtimePath;

        if (string.IsNullOrEmpty(realtimePath))
        {
            Debug.LogWarning(
                "[RealtimeSession] Session context " +
                "has no realtimePath."
            );

            return;
        }

        currentActivityReference =
            FirebaseDatabase.DefaultInstance
                .GetReference(
                    $"{realtimePath}/currentActivity"
                );

        currentActivityReference.ValueChanged +=
            OnCurrentActivityChanged;

        Debug.Log(
            "[RealtimeSession] Native listener started for " +
            $"{realtimePath}/currentActivity"
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
                "[RealtimeSession] Database error: " +
                eventArgs.DatabaseError.Message
            );

            return;
        }

        if (!eventArgs.Snapshot.Exists)
        {
            ApplyActivityJson(null);
            return;
        }

        ApplyActivityJson(
            eventArgs.Snapshot.GetRawJsonValue()
        );
    }
#endif

    private void ApplyActivityJson(
        string rawJson
    )
    {
        string json =
            rawJson?.Trim();

        if (
            string.IsNullOrEmpty(json) ||
            json == "null"
        )
        {
            if (lastActivityJson == "null")
            {
                return;
            }

            lastActivityJson = "null";

            Debug.Log(
                "[RealtimeSession] No active activity."
            );

            SetActiveActivity(null);
            return;
        }

        if (json == lastActivityJson)
        {
            return;
        }

        LiveActivity activity;

        try
        {
            activity =
                JsonUtility.FromJson<LiveActivity>(
                    json
                );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[RealtimeSession] Failed to parse " +
                "activity: " +
                exception.Message +
                "\nJSON: " +
                json
            );

            return;
        }

        if (activity == null)
        {
            Debug.LogError(
                "[RealtimeSession] Parsed activity was null."
            );

            return;
        }

        lastActivityJson = json;

        Debug.Log(
            $"[RealtimeSession] Activity changed: " +
            $"id={activity.id}, " +
            $"type={activity.activityType}, " +
            $"status={activity.status}"
        );

        SetActiveActivity(activity);
    }

    private void OnDestroy()
    {
#if TRIHEAL_FIREBASE_REST
        if (restPollingCoroutine != null)
        {
            StopCoroutine(restPollingCoroutine);
            restPollingCoroutine = null;
        }
#else
        if (currentActivityReference != null)
        {
            currentActivityReference.ValueChanged -=
                OnCurrentActivityChanged;
        }
#endif

        if (instance == this)
        {
            instance = null;
            SetActiveActivity(null);
        }
    }
}
