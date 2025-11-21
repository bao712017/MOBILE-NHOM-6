using UnityEngine;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine.SceneManagement;

public class GoogleAuthManager : MonoBehaviour
{
    public static GoogleAuthManager Instance;

    private FirebaseAuth auth;
    private FirebaseUser user;

    private string webClientId =
        "783975870997-2ltboggapl6o2v2o4q2pk3j0irdd2cj8.apps.googleusercontent.com";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private async void Start()
    {
        await InitializeFirebase();
    }

    private async System.Threading.Tasks.Task InitializeFirebase()
    {
        var dependency = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependency == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            Debug.Log("Firebase Ready!");
        }
        else
            Debug.LogError("Firebase error: " + dependency);
    }

    public void OnClickGoogleSignIn()
    {
#if UNITY_ANDROID
        SignInWithGoogle();
#endif
    }

    private void SignInWithGoogle()
    {
        GoogleSignInConfiguration config = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };

        GoogleSignIn.Configuration = config;

        GoogleSignIn.DefaultInstance.SignIn()
            .ContinueWith(OnGoogleAuthFinished);
    }

    private void OnGoogleAuthFinished(System.Threading.Tasks.Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Google Sign-In failed: " + task.Exception);
            return;
        }

        var googleUser = task.Result;
        string idToken = googleUser.IdToken;

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.LogError("Firebase auth failed: " + t.Exception);
                else
                {
                    user = t.Result;
                    Debug.Log("LOGIN OK: " + user.DisplayName);

                    SceneManager.LoadScene("Menu");
                }
            });
    }
}
