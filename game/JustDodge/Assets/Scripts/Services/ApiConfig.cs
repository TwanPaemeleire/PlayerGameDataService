using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfig", menuName = "Config/APIConfig")]
public class ApiConfig : ScriptableObject
{
    [SerializeField] private string _baseUrl = "http://localhost:5269";
    [SerializeField] private int _timeoutSeconds = 10;

    public string BaseUrl => _baseUrl.TrimEnd('/');
    public int TimeoutSeconds => _timeoutSeconds;
}
