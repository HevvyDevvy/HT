using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HustleThrough.Local
{
    [Serializable]
    public class SaveData
    {
        public int rank = 1;
        public int level = 1;
        public long cashBalance = 0;
        public long notesBalance = 0;
        public List<string> completedJobIds = new List<string>();
        public List<string> ownedStoreSkus = new List<string>();
        public List<string> storyFlagKeys = new List<string>();
        public List<string> storyFlagValues = new List<string>(); // parallel array to storyFlagKeys
        public bool adsRemoved = false;
    }

    /// <summary>
    /// Replaces the Postgres-backed player record entirely. All progress lives
    /// in a single JSON file in Application.persistentDataPath, which Unity
    /// maps to the correct writable app-data location on every platform
    /// (Windows, macOS, Android, iOS) automatically — no server, no account,
    /// no network required.
    ///
    /// Trade-off worth knowing: this save lives on-device only. If the player
    /// uninstalls the app or switches phones, progress is gone unless you add
    /// cloud save later (Google Play Games Services / Apple Game Center both
    /// offer free cloud save hooks — a good next step, still doesn't require
    /// your own backend).
    /// </summary>
    public static class LocalSaveManager
    {
        private const string SaveFileName = "hustle_through_save.json";
        private static SaveData _cache;

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public static SaveData Load()
        {
            if (_cache != null) return _cache;

            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    _cache = JsonUtility.FromJson<SaveData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load save, starting fresh: {e.Message}");
                    _cache = new SaveData();
                }
            }
            else
            {
                _cache = new SaveData();
            }

            return _cache;
        }

        public static void Save(SaveData data)
        {
            _cache = data;
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write save file: {e.Message}");
            }
        }

        public static void SetStoryFlag(string key, string value)
        {
            var data = Load();
            int idx = data.storyFlagKeys.IndexOf(key);
            if (idx >= 0)
            {
                data.storyFlagValues[idx] = value;
            }
            else
            {
                data.storyFlagKeys.Add(key);
                data.storyFlagValues.Add(value);
            }
            Save(data);
        }

        public static string GetStoryFlag(string key)
        {
            var data = Load();
            int idx = data.storyFlagKeys.IndexOf(key);
            return idx >= 0 ? data.storyFlagValues[idx] : null;
        }

        /// <summary>Wipes all progress. Wire this to a confirmed "New Game" button only.</summary>
        public static void ResetAll()
        {
            _cache = new SaveData();
            Save(_cache);
        }
    }
}
