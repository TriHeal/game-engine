// We always want to use REST work-around since the firebase framework
// doesn't seems to work (TODO: fix in the future)
//#if UNITY_EDITOR || UNITY_STANDALONE_OSX
#define TRIHEAL_FIREBASE_REST
//#endif

using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if !TRIHEAL_FIREBASE_REST
using Firebase;
using Firebase.Auth;
#endif

/// <summary>
/// Controls session authorization and OTP verification.
/// </summary>
public class LoginScreenController : MonoBehaviour
{
#if UNITY_EDITOR
    private const bool BypassOtpInEditor = false;
#endif

    private const string DEMO_CODE = "111111";

    private const string TXT_START_VISIT = "התחל ביקור";
    private const string TXT_END_VISIT = "סיים ביקור";

    [Header("Screens")]
    public GameObject loginScreen;
    public CanvasGroup loginGroup;

    [Tooltip("What Login opens on success (Welcome).")]
    public GameObject nextScreen;
    public CanvasGroup nextGroup;
    public float crossFadeDuration = 0.8f;

    [Header("Code Entry")]
    [Tooltip("The single invisible input field overlaying the code area.")]
    public TMP_InputField hiddenInputField;

    [Tooltip("Exactly 6 TMP_Text UI elements inside your visual orb containers.")]
    public TMP_Text[] digitLabels = new TMP_Text[6];

    public Button submitButton;
    public TMP_Text errorText;

    [Header("Session Toggle Button UI")]
    [Tooltip("Drag the Text component of your toggle button here.")]
    public TMP_Text sessionButtonText;

    [Header("Backend")]
    public string baseUrl = "http://localhost:3003";

#if !TRIHEAL_FIREBASE_REST
    private FirebaseAuth auth;
#endif

    private bool submitting;
    private bool transitioning;
    private bool isSessionActive = false;

    private void Awake()
    {
        SyncSessionState();
    }

    private void OnEnable()
    {
        SyncSessionState();
    }

    private void SyncSessionState()
    {
        // Only mark active if there is a session AND it's not the default fallback home session
        if (SessionContext.HasSession && SessionContext.Current.sessionId != "session-home")
        {
            isSessionActive = true;
        }
        else
        {
            isSessionActive = false;
        }

        UpdateToggleButtonText();
    }

    private void Start()
    {
        Debug.Log("[Login Debug] --- Start() Initializing Controller ---");

        SyncSessionState();

        // Ensure login panel UI starts hidden unless explicitly opened
        if (loginScreen != null)
        {
            loginScreen.SetActive(false);
        }

        HideError();

        // Register input listener for the single hidden field
        if (hiddenInputField != null)
        {
            hiddenInputField.onValueChanged.AddListener(OnInputChanged);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(Submit);
        }

        // Only start initialization coroutine if the GameObject is active in hierarchy
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(InitializeAuthentication());
        }
    }

    private IEnumerator InitializeAuthentication()
    {
        Debug.Log("[Login Debug] InitializeAuthentication started.");
#if TRIHEAL_FIREBASE_REST
    if (FirebaseRestSession.Restore() && SessionContext.Load())
    {
        Debug.Log("[Login Debug] Existing Firebase REST session found -> skipping login");
        
        isSessionActive = true;
        UpdateToggleButtonText();
        EnterNextScreenImmediately();
    }

    yield break;
#else
        System.Threading.Tasks.Task<DependencyStatus> dependencyTask;

        try
        {
            dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        }
        catch (Exception exception)
        {
            Debug.LogError("[Login Debug] Firebase dependency initialization failed: " + exception.Message);
            yield break;
        }

        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        if (dependencyTask.IsFaulted || dependencyTask.Result != DependencyStatus.Available)
        {
            Debug.LogError(
                "[Login Debug] Firebase dependencies unavailable: " +
                (dependencyTask.IsFaulted ? dependencyTask.Exception?.ToString() : dependencyTask.Result.ToString())
            );

            yield break;
        }

        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            Debug.Log("[Login Debug] Active Firebase user found in SDK.");
            isSessionActive = true;
            UpdateToggleButtonText();
        }
#endif
    }

    public void ToggleSession()
    {
        Debug.Log($"[Login Debug] >>> ToggleSession() CLICKED. Current isSessionActive state = {isSessionActive} <<<");

        if (isSessionActive)
        {
            Debug.Log("[Login Debug] State is ACTIVE -> Executing EndSession()");
            EndSession();
        }
        else
        {
            Debug.Log("[Login Debug] State is INACTIVE -> Executing StartSession()");
            StartSession();
        }
    }

    public void StartSession()
    {
        Debug.Log($"[Login Debug] StartSession() called. Submitting={submitting}, Transitioning={transitioning}");

        this.enabled = true;
        submitting = false;
        transitioning = false;

        if (loginScreen != null)
        {
            loginScreen.SetActive(true);
            Debug.Log("[Login Debug] Login screen panel activated.");
        }

        if (loginGroup != null)
        {
            loginGroup.alpha = 1f;
        }

        ClearDigits();
        HideError();
        SetInteractable(true);

#if UNITY_EDITOR
        if (BypassOtpInEditor)
        {
            Debug.Log("[Login Debug] Editor OTP bypass active -> Bypassing code entry.");
            SaveDemoSessionContext("patient-editor-bypass", "session-editor-bypass");
            OnSessionSuccessfullyStarted();
            EnterNextScreenImmediately();
        }
#endif
    }

    public void EndSession()
    {
        Debug.Log("[Login Debug] EndSession() called -> Reverting to Home games context.");

        isSessionActive = false;
        UpdateToggleButtonText();

#if TRIHEAL_FIREBASE_REST
        FirebaseRestSession.Clear();
#else
        if (auth != null && auth.CurrentUser != null)
        {
            auth.SignOut();
        }
#endif

        SessionContext.Save(
            "patient-home",
            "session-home",
            "liveSessions/session-home",
            new[]
            {
                new SessionActivitySelection { type = "breathing", order = 1, status = "pending" },
                new SessionActivitySelection { type = "bonding_forest", order = 2, status = "pending" }
            }
        );

        submitting = false;
        transitioning = false;

        if (loginScreen != null)
        {
            loginScreen.SetActive(false);
        }

        if (nextScreen != null)
        {
            nextScreen.SetActive(true);
        }

        if (nextGroup != null)
        {
            nextGroup.alpha = 1f;
        }

        Debug.Log("[Login Debug] Session ended successfully.");
    }

    private void OnSessionSuccessfullyStarted()
    {
        Debug.Log("[Login Debug] Session successfully started -> Updating state & button text.");
        isSessionActive = true;
        UpdateToggleButtonText();
    }

    private void UpdateToggleButtonText()
    {
        if (sessionButtonText != null)
        {
            string oldText = sessionButtonText.text;
            sessionButtonText.text = isSessionActive ? TXT_END_VISIT : TXT_START_VISIT;
            Debug.Log($"[Login Debug] Updated button text: '{oldText}' -> '{sessionButtonText.text}'");
        }
    }

    private void EnterNextScreenImmediately()
    {
        if (loginScreen != null)
        {
            loginScreen.SetActive(false);
        }

        if (nextScreen != null)
        {
            nextScreen.SetActive(true);
        }

        if (nextGroup != null)
        {
            nextGroup.alpha = 1f;
        }
    }

    private void OnInputChanged(string value)
    {
        // Update visual labels matching the string length
        for (int i = 0; i < digitLabels.Length; i++)
        {
            if (digitLabels[i] == null) continue;

            if (i < value.Length)
            {
                digitLabels[i].text = value[i].ToString();
            }
            else
            {
                digitLabels[i].text = "";
            }
        }

        // Auto-submit when all 6 digits are typed
        if (AllDigitsFilled())
        {
            Submit();
        }
    }

    private bool AllDigitsFilled()
    {
        return hiddenInputField != null && hiddenInputField.text.Length == 6;
    }

    public void Submit()
    {
        if (submitting || transitioning)
        {
            return;
        }

#if !TRIHEAL_FIREBASE_REST
        if (auth == null)
        {
            return;
        }
#endif

        if (!AllDigitsFilled())
        {
            return;
        }

        // Force the input field to lose focus so Android closes the keyboard
        hiddenInputField.DeactivateInputField();

        StartCoroutine(VerifyCode(hiddenInputField.text.Trim()));
    }

    private IEnumerator VerifyCode(string code)
    {
        Debug.Log($"[Login Debug] VerifyCode() called with code: {code}");
        submitting = true;
        SetInteractable(false);
        HideError();

        if (code == DEMO_CODE)
        {
            Debug.Log("[Login Debug] Demo code entered (111111) -> Launching full demo session.");
            SaveDemoSessionContext("patient-demo-111111", "session-demo-111111");

            OnSessionSuccessfullyStarted();
            transitioning = true;
            yield return CrossFade();

            submitting = false;
            yield break;
        }

        var payload = new VerifyCodeRequest { code = code };
        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (var request = new UnityWebRequest($"{baseUrl.TrimEnd('/')}/auth/otp/verify", UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                FailAndReset("קוד שגוי, נסה שוב");
                yield break;
            }

            VerifyResponse response;

            try
            {
                response = JsonUtility.FromJson<VerifyResponse>(request.downloadHandler.text);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[Login] Failed to parse verify response: " +
                    exception.Message
                );
                FailAndReset("משהו השתבש, נסה שוב");
                yield break;
            }

            if (response == null || string.IsNullOrEmpty(response.token))
            {
                FailAndReset("משהו השתבש, נסה שוב");
                yield break;
            }

            SessionContext.Save(
                response.patientId,
                response.sessionId,
                response.realtimePath,
                response.activities
            );

            OnSessionSuccessfullyStarted();
            yield return SignIn(response.token);
        }
    }

    private void SaveDemoSessionContext(string patientId, string sessionId)
    {
        SessionContext.Save(
            patientId,
            sessionId,
            $"liveSessions/{sessionId}",
            new[]
            {
                new SessionActivitySelection { type = "breathing", order = 1, status = "pending" },
                new SessionActivitySelection { type = "event_processing", order = 2, status = "pending" },
                new SessionActivitySelection { type = "memory_lake", order = 3, status = "pending" },
                new SessionActivitySelection { type = "bonding_forest", order = 4, status = "pending" }
            }
        );

        RealtimeSessionListener.SetActiveActivity(null);
    }

    private IEnumerator SignIn(string token)
    {
#if TRIHEAL_FIREBASE_REST
        bool succeeded = false;
        string error = null;

        yield return FirebaseRestSession.SignInWithCustomToken(
            token,
            (wasSuccessful, errorMessage) =>
            {
                succeeded = wasSuccessful;
                error = errorMessage;
            }
        );

        if (!succeeded)
        {
            Debug.LogError(
                "[Login] Firebase REST sign-in failed: " +
                error
            );

            FirebaseRestSession.Clear();
            SessionContext.Clear();

            FailAndReset("משהו השתבש, נסה שוב");
            yield break;
        }

        Debug.Log(
            "[Login] Firebase REST sign-in complete."
        );
#else
        var signInTask = auth.SignInWithCustomTokenAsync(token);

        yield return new WaitUntil(() => signInTask.IsCompleted);

        if (signInTask.IsFaulted || signInTask.IsCanceled)
        {
            Debug.LogError(
                "[Login] Firebase sign-in failed: " +
                signInTask.Exception
            );

            SessionContext.Clear();
            FailAndReset("משהו השתבש, נסה שוב");
            yield break;
        }

        Debug.Log(
            "[Login] Firebase SDK sign-in complete."
        );
#endif

        transitioning = true;
        yield return CrossFade();
    }

    private IEnumerator CrossFade()
    {
        if (nextScreen != null)
        {
            nextScreen.SetActive(true);
        }

        if (nextGroup != null)
        {
            nextGroup.alpha = 0f;
        }

        float startLoginAlpha = loginGroup != null ? loginGroup.alpha : 1f;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime / Mathf.Max(0.01f, crossFadeDuration);
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));

            if (loginGroup != null)
            {
                loginGroup.alpha = Mathf.Lerp(startLoginAlpha, 0f, eased);
            }

            if (nextGroup != null)
            {
                nextGroup.alpha = eased;
            }

            yield return null;
        }

        if (nextGroup != null)
        {
            nextGroup.alpha = 1f;
        }

        if (loginScreen != null)
        {
            loginScreen.SetActive(false);
        }

        Debug.Log(
            "[Login] Cross-fade complete -> login hidden, next screen active"
        );
    }

    private void FailAndReset(string message)
    {
        ShowError(message);
        ClearDigits();

        submitting = false;
        SetInteractable(true);
    }

    private void SetInteractable(bool interactable)
    {
        if (hiddenInputField != null)
        {
            hiddenInputField.interactable = interactable;
        }

        if (submitButton != null)
        {
            submitButton.interactable = interactable;
        }
    }

    private void ClearDigits()
    {
        if (hiddenInputField != null)
        {
            hiddenInputField.SetTextWithoutNotify("");
        }

        foreach (TMP_Text label in digitLabels)
        {
            if (label != null)
            {
                label.text = "";
            }
        }

        if (hiddenInputField != null && hiddenInputField.gameObject.activeInHierarchy)
        {
            hiddenInputField.Select();
            hiddenInputField.ActivateInputField();
        }
    }

    private void ShowError(string message)
    {
        if (errorText == null)
        {
            return;
        }

        errorText.text = message;
        errorText.gameObject.SetActive(true);
    }

    private void HideError()
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called when the user clicks outside the login panel or taps a cancel button.
    /// Closes the login overlay without altering session states.
    /// </summary>
    public void CancelLogin()
    {
        Debug.Log("[Login Debug] CancelLogin() called -> Closing login overlay.");

        submitting = false;
        transitioning = false;

        // Clear typed digits and hide errors for next time
        ClearDigits();
        HideError();

        // Close the login screen panel
        if (loginScreen != null)
        {
            loginScreen.SetActive(false);
        }
    }

    [Serializable]
    private class VerifyCodeRequest
    {
        public string code;
    }

    [Serializable]
    private class VerifyResponse
    {
        public string token;
        public string role;
        public string patientId;
        public string sessionId;
        public string realtimePath;
        public SessionActivitySelection[] activities;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem(
        "Tri-Heal/Login/Clear Editor Session"
    )]
    private static void ClearEditorSession()
    {
        FirebaseRestSession.Clear();
        SessionContext.Clear();

        // Remove the legacy mock-login flag.
        PlayerPrefs.DeleteKey(
            "EditorMockSignedIn"
        );

        PlayerPrefs.Save();

        Debug.Log(
            "[Login] Editor session cleared."
        );
    }
#endif
}