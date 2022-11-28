using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Check to see if we're about to be destroyed.
    protected static bool _shuttingDown = false;
    private static object _lock = new object();
    private static T _instance;

    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_shuttingDown)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // Search for existing instance.
                    _instance = (T)FindObjectOfType(typeof(T));
                }

                return _instance;
            }
        }
    }


    private void OnApplicationQuit()
    {
        _shuttingDown = true;
    }


    private void OnDestroy()
    {
        _shuttingDown = true;
    }
}
