using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;
    private static bool _isQuitting;

    public static T Instance
    {
        get
        {
            if (_instance == null && !_isQuitting)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                {
                    new GameObject("_" + typeof(T).Name, typeof(T));
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = (T)this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        Init();
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    protected virtual void Init() { }
}
