using System;
using UnityEngine;
using HustleThrough.Data;
using HustleThrough.Local;

namespace HustleThrough.Progression
{
    public struct CompleteJobResult
    {
        public long cashAwarded;
        public int rankAfter;
        public int levelAfter;
        public bool rankedUp;
        public bool success;
        public string errorMessage;
    }

    /// <summary>
    /// The offline equivalent of the old backend's progressionService.ts.
    /// Same rule, enforced the same way, just running on-device instead of
    /// on a server: rank/level can ONLY change through CompleteJob. Nothing
    /// in IAPManager or the economy layer calls into this class — that
    /// separation is what keeps "no IAP touches rank" true even without a
    /// server watching over it.
    /// </summary>
    public class PlayerProgress : MonoBehaviour
    {
        public static PlayerProgress Instance { get; private set; }

        public event Action OnProgressChanged;

        private SaveData _data;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _data = LocalSaveManager.Load();
        }

        public int Rank => _data.rank;
        public int Level => _data.level;
        public long CashBalance => _data.cashBalance;
        public long NotesBalance => _data.notesBalance;

        public string RankTitle
        {
            get
            {
                var def = GameDataLoader.RankForLevel(_data.level);
                return def?.title ?? "Rookie";
            }
        }

        public bool HasCompletedJob(string jobId) => _data.completedJobIds.Contains(jobId);

        public CompleteJobResult CompleteJob(string jobId)
        {
            var job = GameDataLoader.FindJob(jobId);
            if (job == null)
            {
                return new CompleteJobResult { success = false, errorMessage = "Job not found" };
            }
            if (job.rankRequired > _data.rank)
            {
                return new CompleteJobResult { success = false, errorMessage = "Rank too low for this job" };
            }
            if (!job.isBonusPool && HasCompletedJob(jobId))
            {
                return new CompleteJobResult { success = false, errorMessage = "Story job already completed" };
            }

            int newLevel = job.isBonusPool ? _data.level : Math.Max(_data.level, job.level);
            var newRankDef = GameDataLoader.RankForLevel(newLevel);
            int newRank = newRankDef?.rank ?? _data.rank;
            bool rankedUp = newRank > _data.rank;

            _data.cashBalance += job.cashReward;
            _data.rank = newRank;
            _data.level = newLevel;
            if (!_data.completedJobIds.Contains(jobId))
            {
                _data.completedJobIds.Add(jobId);
            }

            LocalSaveManager.Save(_data);
            OnProgressChanged?.Invoke();

            return new CompleteJobResult
            {
                success = true,
                cashAwarded = job.cashReward,
                rankAfter = newRank,
                levelAfter = newLevel,
                rankedUp = rankedUp,
            };
        }

        /// <summary>
        /// The ONLY way Notes (hard currency) enters the save file — called by
        /// IAPManager after a purchase, and by nothing else. This function does
        /// not touch rank or level, by design.
        /// </summary>
        public void CreditNotes(long amount)
        {
            _data.notesBalance += amount;
            LocalSaveManager.Save(_data);
            OnProgressChanged?.Invoke();
        }

        public bool SpendNotes(long amount)
        {
            if (_data.notesBalance < amount) return false;
            _data.notesBalance -= amount;
            LocalSaveManager.Save(_data);
            OnProgressChanged?.Invoke();
            return true;
        }

        public bool OwnsStoreItem(string sku) => _data.ownedStoreSkus.Contains(sku);

        public bool AdsRemoved => _data.adsRemoved;

        public void SetAdsRemoved(bool value)
        {
            _data.adsRemoved = value;
            LocalSaveManager.Save(_data);
            OnProgressChanged?.Invoke();
        }

        public void GrantStoreItem(string sku)
        {
            if (!_data.ownedStoreSkus.Contains(sku))
            {
                _data.ownedStoreSkus.Add(sku);
                LocalSaveManager.Save(_data);
                OnProgressChanged?.Invoke();
            }
        }
    }
}
