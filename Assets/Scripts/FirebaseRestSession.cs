using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class FirebaseRestSession
{
    private const string ApiKey =
        "AIzaSyBeO__hztZWbwPoqdz8FpR7WrwG1qoZRps";

    private const string IosBundleId =
        "com.devspirit.triheal";

    private const string DatabaseUrl =
        "https://tri-heal-dev-d9484-default-rtdb.europe-west1.firebasedatabase.app";

    private const string IdTokenPlayerPrefsKey =
        "TriHealFirebaseRestIdToken";

    private const string RefreshTokenPlayerPrefsKey =
        "TriHealFirebaseRestRefreshToken";

    private const string ExpiresAtPlayerPrefsKey =
        "TriHealFirebaseRestExpiresAt";

    public static string IdToken { get; private set; }

    public static string RefreshToken { get; private set; }

    public static bool HasValidIdToken =>
        !string.IsNullOrEmpty(IdToken) &&
        GetCurrentUnixTime() < GetStoredExpirationTime();

    public static bool Restore()
    {
        if (HasValidIdToken)
        {
            return true;
        }

        string storedIdToken =
            PlayerPrefs.GetString(IdTokenPlayerPrefsKey, "");

        string storedRefreshToken =
            PlayerPrefs.GetString(RefreshTokenPlayerPrefsKey, "");

        long expirationTime = GetStoredExpirationTime();

        if (
            string.IsNullOrEmpty(storedIdToken) ||
            GetCurrentUnixTime() >= expirationTime
        )
        {
            Clear();
            return false;
        }

        IdToken = storedIdToken;
        RefreshToken = storedRefreshToken;

        return true;
    }

    public static IEnumerator SignInWithCustomToken(
        string customToken,
        Action<bool, string> completed
    )
    {
        if (string.IsNullOrEmpty(customToken))
        {
            completed?.Invoke(false, "Custom token is empty.");
            yield break;
        }

        string url =
            "https://identitytoolkit.googleapis.com/v1/" +
            "accounts:signInWithCustomToken?key=" +
            UnityWebRequest.EscapeURL(ApiKey);

        var payload = new CustomTokenRequest
        {
            token = customToken,
            returnSecureToken = true
        };

        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (
            var request = new UnityWebRequest(
                url,
                UnityWebRequest.kHttpVerbPOST
            )
        )
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            // Required when the Firebase API key has an iOS bundle restriction.
            request.SetRequestHeader(
                "X-Ios-Bundle-Identifier",
                IosBundleId
            );

            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string message =
                    $"HTTP {request.responseCode}: " +
                    $"{request.error} / " +
                    $"{request.downloadHandler.text}";

                completed?.Invoke(false, message);
                yield break;
            }

            CustomTokenResponse response;

            try
            {
                response =
                    JsonUtility.FromJson<CustomTokenResponse>(
                        request.downloadHandler.text
                    );
            }
            catch (Exception exception)
            {
                completed?.Invoke(
                    false,
                    $"Failed to parse Firebase Auth response: " +
                    exception.Message
                );

                yield break;
            }

            if (
                response == null ||
                string.IsNullOrEmpty(response.idToken)
            )
            {
                completed?.Invoke(
                    false,
                    "Firebase Auth response contained no ID token."
                );

                yield break;
            }

            long expiresInSeconds = 3600;

            if (
                !string.IsNullOrEmpty(response.expiresIn) &&
                long.TryParse(
                    response.expiresIn,
                    out long parsedExpiresIn
                )
            )
            {
                expiresInSeconds = parsedExpiresIn;
            }

            // Keep a one-minute margin before the actual expiration.
            long expiresAt =
                GetCurrentUnixTime() +
                Math.Max(60, expiresInSeconds - 60);

            IdToken = response.idToken;
            RefreshToken = response.refreshToken;

            PlayerPrefs.SetString(
                IdTokenPlayerPrefsKey,
                IdToken
            );

            PlayerPrefs.SetString(
                RefreshTokenPlayerPrefsKey,
                RefreshToken ?? ""
            );

            PlayerPrefs.SetString(
                ExpiresAtPlayerPrefsKey,
                expiresAt.ToString()
            );

            PlayerPrefs.Save();

            Debug.Log(
                "[FirebaseRest] Custom token exchanged for Firebase ID token."
            );

            completed?.Invoke(true, null);
        }
    }

    public static string BuildCurrentActivityUrl(
        string realtimePath
    )
    {
        if (
            !HasValidIdToken ||
            string.IsNullOrWhiteSpace(realtimePath)
        )
        {
            return null;
        }

        string normalizedPath =
            realtimePath.Trim().Trim('/');

        return
            $"{DatabaseUrl.TrimEnd('/')}/" +
            $"{normalizedPath}/currentActivity.json" +
            $"?auth={UnityWebRequest.EscapeURL(IdToken)}";
    }

    public static void Clear()
    {
        IdToken = null;
        RefreshToken = null;

        PlayerPrefs.DeleteKey(IdTokenPlayerPrefsKey);
        PlayerPrefs.DeleteKey(RefreshTokenPlayerPrefsKey);
        PlayerPrefs.DeleteKey(ExpiresAtPlayerPrefsKey);

        PlayerPrefs.Save();
    }

    private static long GetStoredExpirationTime()
    {
        string value =
            PlayerPrefs.GetString(ExpiresAtPlayerPrefsKey, "0");

        return long.TryParse(value, out long result)
            ? result
            : 0;
    }

    private static long GetCurrentUnixTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    [Serializable]
    private class CustomTokenRequest
    {
        public string token;
        public bool returnSecureToken;
    }

    [Serializable]
    private class CustomTokenResponse
    {
        public string idToken;
        public string refreshToken;
        public string expiresIn;
        public string localId;
    }
}
