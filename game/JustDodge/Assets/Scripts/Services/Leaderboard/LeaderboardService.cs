using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardService : MonoSingleton<LeaderboardService>
{
    private const string TokenKey = "PlayerToken";
    private const string NameKey = "PlayerName";

    [SerializeField] private ApiConfig _apiConfig;

    private Task<string> _runIdTask;

    public void StartRun()
    {
        _runIdTask = RequestRunId();
    }

    private async Task<string> RequestRunId()
    {
        using UnityWebRequest request = UnityWebRequest.Post(_apiConfig.BaseUrl + "/runs", "{}", "application/json");
        request.timeout = _apiConfig.TimeoutSeconds;
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success) return null;
        return JsonUtility.FromJson<StartRunResponse>(request.downloadHandler.text).runId;
    }

    public async Task<List<LeaderboardEntry>> GetTop10Scores()
    {
        using UnityWebRequest request = UnityWebRequest.Get(_apiConfig.BaseUrl + "/scores/top");
        request.timeout = _apiConfig.TimeoutSeconds;
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            LeaderboardEntry[] entries = ParseEntries(request.downloadHandler.text);
            Debug.Log("Parsed " + entries.Length + " leaderboard entries");
            return entries.ToList();
        }
        else
        {
            Debug.LogError($"Request failed ({request.responseCode}): {request.error}");
            return null;
        }
    }

    public bool IsRegistered => PlayerPrefs.HasKey(TokenKey) && PlayerPrefs.HasKey(NameKey);
    public string SavedName => PlayerPrefs.GetString(NameKey, "");

    public async Task<SubmitScoreResult> SubmitScore(string playerName, int score)
    {
        string runId = _runIdTask != null ? await _runIdTask : null;
        if (runId == null)
        {
            return new SubmitScoreResult(false, "Couldn't reach the server during this run");
        }

        if (!IsRegistered)
        {
            string requestJson = JsonUtility.ToJson(new RegisterRequest { playerName = playerName });
            using UnityWebRequest registerRequest = UnityWebRequest.Post(_apiConfig.BaseUrl + "/players/register", requestJson, "application/json");
            registerRequest.timeout = _apiConfig.TimeoutSeconds;
            await registerRequest.SendWebRequest();

            if (registerRequest.responseCode == 409)
            {
                return new SubmitScoreResult(false, "Name already taken");
            }
            if (registerRequest.result != UnityWebRequest.Result.Success)
            {
                return ToFailure(registerRequest);
            }

            RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(registerRequest.downloadHandler.text);
            PlayerPrefs.SetString(NameKey, response.playerName);
            PlayerPrefs.SetString(TokenKey, response.token);
            PlayerPrefs.Save();
        }

        string json = JsonUtility.ToJson(new SubmitScoreRequest { value = score, runId = runId });
        using UnityWebRequest scoreSubmitRequest = UnityWebRequest.Post(_apiConfig.BaseUrl + "/scores", json, "application/json");
        scoreSubmitRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(TokenKey));
        scoreSubmitRequest.timeout = _apiConfig.TimeoutSeconds;
        await scoreSubmitRequest.SendWebRequest();

        if (scoreSubmitRequest.result == UnityWebRequest.Result.Success)
        {
            return new SubmitScoreResult(true, "Score submitted!");
        }
        if (scoreSubmitRequest.responseCode == 401)
        {
            PlayerPrefs.DeleteKey(TokenKey);
            PlayerPrefs.DeleteKey(NameKey);
            PlayerPrefs.Save();
            return new SubmitScoreResult(false, "Your name expired, pick a name and submit again");
        }
        return ToFailure(scoreSubmitRequest);
    }

    private SubmitScoreResult ToFailure(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Request failed: " + request.error);
            return new SubmitScoreResult(false, "Couldn't reach the server, try again");
        }
        if (request.responseCode == 400)
        {
            return new SubmitScoreResult(false, ParseValidationMessage(request.downloadHandler.text));
        }

        Debug.LogError($"Request failed ({request.responseCode}): {request.downloadHandler.text}");
        return new SubmitScoreResult(false, $"Server error ({request.responseCode}), try again");
    }

    private string ParseValidationMessage(string json)
    {
        ValidationProblem problem = JsonUtility.FromJson<ValidationProblem>(json);
        List<string> messages = new List<string>();
        if (problem?.errors?.playerName != null) messages.AddRange(problem.errors.playerName);
        if (problem?.errors?.value != null) messages.AddRange(problem.errors.value);
        return messages.Count > 0 ? string.Join("\n", messages) : "The server rejected this score";
    }

    [Serializable]
    private class SubmitScoreRequest
    {
        public int value;
        public string runId;
    }

    [Serializable]
    private class StartRunResponse
    {
        public string runId;
    }

    [Serializable]
    private class RegisterRequest
    {
        public string playerName;
    }

    [Serializable]
    private class ValidationProblem
    {
        public ValidationErrors errors;
    }

    [Serializable]
    private class ValidationErrors
    {
        public string[] playerName;
        public string[] value;
    }

    [Serializable]
    private class LeaderboardEntryList
    {
        public LeaderboardEntry[] entries;
    }

    private LeaderboardEntry[] ParseEntries(string json)
    {
        LeaderboardEntryList list = JsonUtility.FromJson<LeaderboardEntryList>("{\"entries\":" + json + "}");
        return list?.entries ?? Array.Empty<LeaderboardEntry>();
    }

    [Serializable]
    private class RegisterResponse
    {
        public string playerName;
        public string token;
    }
}

public class SubmitScoreResult
{
    public bool Success { get; }
    public string Message { get; }

    public SubmitScoreResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}
