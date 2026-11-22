using TMPro;
using UnityEngine;
using NutriPick.Content;

namespace NutriPick.UI
{
    /// <summary>World-space sales board fed by the plated foods.</summary>
    public class StandScoreboard : MonoBehaviour
    {
        public TMP_Text text;
        public StandPopularityTracker tracker;

        private void Start()
        {
            if (tracker == null)
                tracker = FindAnyObjectByType<StandPopularityTracker>();

            if (tracker != null)
            {
                tracker.CountsChanged += Refresh;
                Refresh();
            }
        }

        private void OnDestroy()
        {
            if (tracker != null)
                tracker.CountsChanged -= Refresh;
        }

        public void Refresh()
        {
            if (text == null || tracker == null)
                return;

            text.text = "PENJUALAN HARI INI\n"
                + "Healthy Food : " + tracker.HealthyCount + "\n"
                + "Junk Food : " + tracker.JunkCount + "\n"
                + tracker.WinnerLabel;
        }
    }
}
