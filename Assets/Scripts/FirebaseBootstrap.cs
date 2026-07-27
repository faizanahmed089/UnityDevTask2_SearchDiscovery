using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseBootstrap : MonoBehaviour
{
    public static bool IsReady { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                IsReady = true;
                Debug.Log("Firebase ready.");
            }
            else
            {
                Debug.LogError($"Firebase dependency error: {task.Result}");
            }
        });
    }
}