using System;
using UnityEngine;
using HustleThrough.Progression;

namespace HustleThrough.IAP
{
    /// <summary>
    /// Rewarded video ads are usually the single biggest revenue lever for a
    /// free-download game, and — done right — the most player-friendly one:
    /// nobody is forced to watch anything, and every reward is something a
    /// player could otherwise just wait or grind for. This keeps the "no
    /// pay-to-win" rule intact for the exact same reason it applies to IAP:
    /// ads only ever grant Notes or minor conveniences, never rank/level.
    ///
    /// INTEGRATION NEEDED: this is a clean interface, not a working ad
    /// network yet. Wire it to Unity LevelPlay (formerly ironSource), AdMob,
    /// or Unity Ads proper — install the relevant SDK via Package Manager,
    /// then implement ShowRewardedAd() to call that SDK and invoke
    /// onRewardEarned only after the network confirms the video was watched
    /// to completion (every network's SDK has this callback — never grant
    /// the reward on "ad started," only on "ad completed").
    /// </summary>
    public class RewardedAdManager : MonoBehaviour
    {
        public static RewardedAdManager Instance { get; private set; }

        [Tooltip("Notes granted for watching a rewarded ad.")]
        public long NotesPerAdWatch = 15;

        [Tooltip("Minimum seconds between ad-earned rewards, to avoid an ad-spam loop.")]
        public float CooldownSeconds = 60f;

        private float _lastRewardTime = -9999f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool IsOnCooldown => Time.unscaledTime - _lastRewardTime < CooldownSeconds;

        public void ShowRewardedAdForNotes(Action<bool> onComplete = null)
        {
            if (IsOnCooldown)
            {
                Debug.Log("Rewarded ad on cooldown.");
                onComplete?.Invoke(false);
                return;
            }

            // TODO: replace this stub with a real SDK call, e.g.:
            //   IronSource.Agent.showRewardedVideo();
            // and hook this method's continuation into that SDK's "ad
            // completed" event rather than calling it immediately.
            Debug.Log("STUB: would show a rewarded ad here — wire up an ad network SDK.");
            OnAdWatchedSuccessfully();
            onComplete?.Invoke(true);
        }

        private void OnAdWatchedSuccessfully()
        {
            _lastRewardTime = Time.unscaledTime;
            PlayerProgress.Instance.CreditNotes(NotesPerAdWatch);
        }
    }
}
