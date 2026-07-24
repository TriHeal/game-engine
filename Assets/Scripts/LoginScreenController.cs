#if UNITY_EDITOR || UNITY_STANDALONE_OSX
#define TRIHEAL_FIREBASE_REST
#endif

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
/// Verifies the child's six-digit OTP through the Tri-Heal backend.
///
/// In the Unity Editor and macOS standalone builds, Firebase Authentication
/// is performed through Firebase REST because the native Firebase desktop
/// library is unavailable.
///
/// Android and iOS builds continue using the Firebase Unity SDK.
/// </summary>
public class LoginScreenController : MonoBehaviour
{
    [Header("Screens")]
    public GameObject loginScreen;
    public CanvasGroup loginGroup;

    [Tooltip("What Login opens on success (Welcome).")]
    public GameObject nextScreen;
    public CanvasGroup nextGroup;
    public float crossFadeDuration = 0.8f;

    [Header("Code Entry")]
    [Tooltip("Exactly 6 single-character TMP_InputField boxes.")]
    public TMP_InputField[] digitFields =
        new TMP_InputField[6];

    public Button submitButton;
    public TMP_Text errorText;

    [Header("Backend")]
    public string baseUrl = "http://localhost:3003";

#if !TRIHEAL_FIREBASE_REST
    private FirebaseAuth auth;
#endif

    private bool submitting;
    private bool transitioning;

    private void Start()
    {
        if (loginScreen != null)
        {
            loginScreen.SetActive(true);
        }

        if (nextScreen != null)
        {
            nextScreen.SetActive(false);
        }

        HideError();

        for (int i = 0; i < digitFields.Length; i++)
        {
            int index = i;

            if (digitFields[i] != null)
            {
                digitFields[i].onValueChanged.AddListener(
                    value => OnDigitChanged(index, value)
                );
            }
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(Submit);
        }

        StartCoroutine(InitializeAuthentication());
    }

    private IEnumerator InitializeAuthentication()
    {
#if TRIHEAL_FIREBASE_REST
        if (
            FirebaseRestSession.Restore() &&
            SessionContext.Load()
        )
        {
            Debug.Log(
                "[Login] Existing Firebase REST session found -> skipping login"
            );

            EnterNextScreenImmediately();
        }

        yield break;
#else
        System.Threading.Tasks.Task<DependencyStatus>
            dependencyTask;

        try
        {
            dependencyTask =
                FirebaseApp.CheckAndFixDependenciesAsync();
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[Login] Firebase dependency initialization failed: " +
                exception.Message
            );

            yield break;
        }

        yield return new WaitUntil(
            () => dependencyTask.IsCompleted
        );

        if (
            dependencyTask.IsFaulted ||
            dependencyTask.Result !=
                DependencyStatus.Available
        )
        {
            Debug.LogError(
                "[Login] Firebase dependencies unavailable: " +
                (
                    dependencyTask.IsFaulted
                        ? dependencyTask.Exception?.ToString()
                        : dependencyTask.Result.ToString()
                )
            );

            yield break;
        }

        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            Debug.Log(
                "[Login] Existing Firebase session found -> skipping login"
            );

            EnterNextScreenImmediately();
        }
#endif
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

    private void OnDigitChanged(
        int index,
        string value
    )
    {
        if (string.IsNullOrEmpty(value))
        {
            if (index > 0)
            {
                digitFields[index - 1].Select();
                digitFields[index - 1].ActivateInputField();
            }

            return;
        }

        char character = value[value.Length - 1];

        if (!char.IsDigit(character))
        {
            digitFields[index].SetTextWithoutNotify("");
            return;
        }

        if (
            digitFields[index].text !=
            character.ToString()
        )
        {
            digitFields[index].SetTextWithoutNotify(
                character.ToString()
            );
        }

        if (index < digitFields.Length - 1)
        {
            digitFields[index + 1].Select();
            digitFields[index + 1].ActivateInputField();
        }
        else
        {
            digitFields[index].DeactivateInputField();
        }

        if (AllDigitsFilled())
        {
            Submit();
        }
    }

    private bool AllDigitsFilled()
    {
        foreach (TMP_InputField field in digitFields)
        {
            if (
                field == null ||
                field.text.Length != 1
            )
            {
                return false;
            }
        }

        return true;
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

        var code = new StringBuilder(
            digitFields.Length
        );

        foreach (TMP_InputField field in digitFields)
        {
            code.Append(field.text);
        }

        StartCoroutine(
            VerifyCode(code.ToString())
        );
    }

    private IEnumerator VerifyCode(string code)
    {
        submitting = true;

        SetInteractable(false);
        HideError();

        var payload = new VerifyCodeRequest
        {
            code = code
        };

        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (
            var request = new UnityWebRequest(
                $"{baseUrl.TrimEnd('/')}/auth/otp/verify",
                UnityWebRequest.kHttpVerbPOST
            )
        )
        {
            request.uploadHandler =
                new UploadHandlerRaw(body);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            request.timeout = 15;

            yield return request.SendWebRequest();

            if (
                request.result !=
                UnityWebRequest.Result.Success
            )
            {
                Debug.LogWarning(
                    $"[Login] Verify failed: " +
                    $"{request.responseCode} / " +
                    $"{request.error} / " +
                    $"{request.downloadHandler.text}"
                );

                FailAndReset(
                    "Wrong code, try again"
                );

                yield break;
            }

            VerifyResponse response;

            try
            {
                response =
                    JsonUtility.FromJson<VerifyResponse>(
                        request.downloadHandler.text
                    );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[Login] Failed to parse verify response: " +
                    exception.Message
                );

                FailAndReset(
                    "Something went wrong, try again"
                );

                yield break;
            }

            if (
                response == null ||
                string.IsNullOrEmpty(response.token)
            )
            {
                FailAndReset(
                    "Something went wrong, try again"
                );

                yield break;
            }

            Debug.Log(
                $"[Login] OTP verified. " +
                $"role={response.role}, " +
                $"patientId={response.patientId}, " +
                $"sessionId={response.sessionId}, " +
                $"realtimePath={response.realtimePath}, " +
                $"activities={response.activities?.Length ?? 0}"
            );

            SessionContext.Save(
                response.patientId,
                response.sessionId,
                response.realtimePath,
                response.activities
            );

            yield return SignIn(response.token);
        }
    }

    private IEnumerator SignIn(string token)
    {
#if TRIHEAL_FIREBASE_REST
        bool succeeded = false;
        string error = null;

        yield return FirebaseRestSession
            .SignInWithCustomToken(
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

            FailAndReset(
                "Something went wrong, try again"
            );

            yield break;
        }

        Debug.Log(
            "[Login] Firebase REST sign-in complete."
        );
#else
        var signInTask =
            auth.SignInWithCustomTokenAsync(token);

        yield return new WaitUntil(
            () => signInTask.IsCompleted
        );

        if (
            signInTask.IsFaulted ||
            signInTask.IsCanceled
        )
        {
            Debug.LogError(
                "[Login] Firebase sign-in failed: " +
                signInTask.Exception
            );

            SessionContext.Clear();

            FailAndReset(
                "Something went wrong, try again"
            );

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

        float startLoginAlpha =
            loginGroup != null
                ? loginGroup.alpha
                : 1f;

        float progress = 0f;

        while (progress < 1f)
        {
            progress +=
                Time.deltaTime /
                Mathf.Max(
                    0.01f,
                    crossFadeDuration
                );

            float eased =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(progress)
                );

            if (loginGroup != null)
            {
                loginGroup.alpha =
                    Mathf.Lerp(
                        startLoginAlpha,
                        0f,
                        eased
                    );
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
        foreach (TMP_InputField field in digitFields)
        {
            if (field != null)
            {
                field.interactable = interactable;
            }
        }

        if (submitButton != null)
        {
            submitButton.interactable = interactable;
        }
    }

    private void ClearDigits()
    {
        foreach (TMP_InputField field in digitFields)
        {
            if (field != null)
            {
                field.SetTextWithoutNotify("");
            }
        }

        if (
            digitFields.Length > 0 &&
            digitFields[0] != null
        )
        {
            digitFields[0].Select();
            digitFields[0].ActivateInputField();
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
