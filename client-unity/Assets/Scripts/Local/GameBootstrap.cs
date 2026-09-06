using UnityEngine;
using HustleThrough.Progression;
using HustleThrough.JobRack;

namespace HustleThrough.Local
{
    /// <summary>
    /// Attach to a single GameObject in your very first scene (e.g. a "Bootstrap"
    /// scene that loads before the main menu). Ensures PlayerProgress and
    /// JobRackManager exist before any UI tries to read from them.
    ///
    /// This replaces the old flow of "log in, then fetch player state from
    /// server" — here, the save file just loads instantly and synchronously
    /// on Awake, so gameplay can start immediately with no loading spinner
    /// waiting on a network call.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Name of the first real gameplay scene to load after setup.")]
        public string firstSceneName = "MainMenu";

        private void Awake()
        {
            EnsureSingleton<PlayerProgress>("PlayerProgress");
            EnsureSingleton<JobRackManager>("JobRackManager");
            // IAPManager self-initializes via its own Awake/Instance pattern;
            // add EnsureSingleton<IAPManager>("IAPManager") here too if you
            // want it alive from the very first frame rather than lazily.
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(firstSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(firstSceneName);
            }
        }

        private static void EnsureSingleton<T>(string objectName) where T : Component
        {
            if (FindObjectOfType<T>() == null)
            {
                var go = new GameObject(objectName);
                go.AddComponent<T>();
            }
        }
    }
}
