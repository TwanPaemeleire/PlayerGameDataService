using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardService : MonoSingleton<LeaderboardService>
{
    [SerializeField] private string _baseUrl = "http://localhost:5269";

    public async Task<List<LeaderboardEntry>> GetTop10Scores()
    {
        using UnityWebRequest request = UnityWebRequest.Get(_baseUrl + "/scores/top");
        request.timeout = 10;
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

    public async Task<SubmitScoreResult> SubmitScore(string playerName, int score)
    {
        string json = JsonUtility.ToJson(new SubmitScoreRequest { playerName = playerName, value = score });
        using UnityWebRequest request = UnityWebRequest.Post(_baseUrl + "/scores", json, "application/json");
        request.timeout = 10;
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            return new SubmitScoreResult(true, "Score submitted!");
        }
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Submit failed: " + request.error);
            return new SubmitScoreResult(false, "Couldn't reach the server, try again");
        }
        if (request.responseCode == 400)
        {
            return new SubmitScoreResult(false, ParseValidationMessage(request.downloadHandler.text));
        }

        Debug.LogError($"Submit failed ({request.responseCode}): {request.downloadHandler.text}");
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
        public string playerName;
        public int value;
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
