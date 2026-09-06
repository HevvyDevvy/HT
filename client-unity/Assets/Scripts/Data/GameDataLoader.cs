using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HustleThrough.Data
{
    /// <summary>
    /// Loads jobs.json and ranks.json from Assets/Resources/Data at runtime.
    /// This data ships INSIDE the app build (Resources folder is baked into
    /// every platform build) — no network call, no server, works fully offline.
    ///
    /// NOTE: Unity's built-in JsonUtility (used here to avoid extra dependencies)
    /// can't parse the optional "storyFlagsSet" object per job. If you need
    /// story flags read from data later, swap to Newtonsoft.Json (via Package
    /// Manager -> "com.unity.nuget.newtonsoft-json") which handles that cleanly.
    /// For now, story flags can still be set directly in code where a specific
    /// job completes (see PlayerProgress.ApplyJobCompletion).
    /// </summary>
    public static class GameDataLoader
    {
        private static List<RankDefinition> _ranks;
        private static List<JobDefinition> _jobs;

        /// <summary>
        /// Set to true once reserve content (the extra 100 post-launch levels)
        /// should go live — e.g. driven by a remote config flag, an app update,
        /// or just a hardcoded flip when you're ready to ship it. Until then,
        /// reserve-batch jobs exist in the data file but are filtered out of
        /// every query below, so they can't accidentally appear early.
        /// </summary>
        public static bool ReserveContentUnlocked = false;

        public static IReadOnlyList<RankDefinition> Ranks
        {
            get
            {
                EnsureLoaded();
                return _ranks;
            }
        }

        public static IReadOnlyList<JobDefinition> Jobs
        {
            get
            {
                EnsureLoaded();
                return _jobs;
            }
        }

        private static void EnsureLoaded()
        {
            if (_ranks != null && _jobs != null) return;

            var rankAsset = Resources.Load<TextAsset>("Data/ranks");
            var jobAsset = Resources.Load<TextAsset>("Data/jobs");

            if (rankAsset == null || jobAsset == null)
            {
                Debug.LogError("GameDataLoader: missing Data/ranks.json or Data/jobs.json in Resources.");
                _ranks = new List<RankDefinition>();
                _jobs = new List<JobDefinition>();
                return;
            }

            _ranks = JsonUtility.FromJson<RankFile>(rankAsset.text).ranks;
            _jobs = JsonUtility.FromJson<JobFile>(jobAsset.text).jobs;
        }

        public static RankDefinition RankForLevel(int level)
        {
            EnsureLoaded();
            return _ranks
                .Where(r => r.unlockLevel <= level)
                .OrderByDescending(r => r.unlockLevel)
                .FirstOrDefault() ?? _ranks.First();
        }

        public static JobDefinition FindJob(string jobId)
        {
            EnsureLoaded();
            return _jobs.FirstOrDefault(j => j.id == jobId);
        }

        /// <summary>
        /// All jobs currently available to query against — launch batch always,
        /// reserve batch only once ReserveContentUnlocked is flipped on.
        /// </summary>
        public static IEnumerable<JobDefinition> AvailableJobs()
        {
            EnsureLoaded();
            return _jobs.Where(j => j.releaseBatch != "reserve" || ReserveContentUnlocked);
        }
    }
}
