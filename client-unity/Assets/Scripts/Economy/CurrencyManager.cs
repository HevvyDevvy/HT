using UnityEngine;
using HustleThrough.Progression;

namespace HustleThrough.Economy
{
    public class CurrencyManager : MonoBehaviour
    {
        public TMPro.TMP_Text CashLabel;
        public TMPro.TMP_Text NotesLabel;

        private void OnEnable()
        {
            PlayerProgress.Instance.OnProgressChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (PlayerProgress.Instance != null)
                PlayerProgress.Instance.OnProgressChanged -= Refresh;
        }

        private void Refresh()
        {
            var progress = PlayerProgress.Instance;
            if (CashLabel != null) CashLabel.text = $"£{progress.CashBalance}";
            if (NotesLabel != null) NotesLabel.text = $"{progress.NotesBalance} Notes";
        }
    }
}
