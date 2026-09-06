using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HustleThrough.Data;
using HustleThrough.Progression;

namespace HustleThrough.JobRack
{
    /// <summary>
    /// Offline equivalent of the old jobRackService.ts. Reads directly from
    /// GameDataLoader (bundled JSON) and PlayerProgress (local save) — no
    /// network call, works with the device fully in airplane mode.
    /// </summary>
    public class JobRackManager : MonoBehaviour
    {
        public static JobRackManager Instance { get; private set; }

        public event Action OnRackRefreshed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public List<JobDefinition> GetStoryJobs(int maxCount = 3)
        {
            var progress = PlayerProgress.Instance;
            return GameDataLoader.AvailableJobs()
                .Where(j => !j.isBonusPool
                            && j.rankRequired <= progress.Rank
                            && j.level > progress.Level)
                .OrderBy(j => j.level)
                .Take(maxCount)
                .ToList();
        }

        public List<JobDefinition> GetBonusJobs(int maxCount = 3)
        {
            var progress = PlayerProgress.Instance;
            var rnd = new System.Random();
            return GameDataLoader.AvailableJobs()
                .Where(j => j.isBonusPool && j.rankRequired <= progress.Rank)
                .OrderBy(_ => rnd.Next())
                .Take(maxCount)
                .ToList();
        }

        public void RefreshRack()
        {
            // No network round-trip needed — this just signals UI to re-pull
            // GetStoryJobs()/GetBonusJobs() and redraw. Kept as an event for
            // parity with the old API-based flow so UI code barely changes.
            OnRackRefreshed?.Invoke();
        }

        public CompleteJobResult CompleteJob(string jobId)
        {
            var result = PlayerProgress.Instance.CompleteJob(jobId);
            if (result.success)
            {
                RefreshRack();
            }
            return result;
        }
    }
}
