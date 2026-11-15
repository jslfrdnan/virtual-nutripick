using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NutriPick.Flow;
using NutriPick.Nutrition;

namespace NutriPick.UI
{
    public class RecapPresenter : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text titleText;
        public TMP_Text levelsText;
        public TMP_Text averageText;
        public TMP_Text strengthsText;
        public TMP_Text improvementText;
        public TMP_Text closingText;
        public TMP_Text disclaimerText;
        public Button restartButton;

        public void Show(GameSession session, Action onRestart)
        {
            if (root != null)
                root.SetActive(true);
            if (titleText != null)
                titleText.text = "Rekap NutriPick";

            if (levelsText != null)
            {
                string text = string.Empty;
                for (int i = 0; i < 3; i++)
                {
                    var result = session != null ? session.GetResult(i + 1) : null;
                    if (result != null)
                        text += $"Level {i + 1} — {result.displayName}: {result.score} ({result.band})\n";
                    else
                        text += $"Level {i + 1}: belum dimainkan\n";
                }
                levelsText.text = text;
            }

            if (averageText != null && session != null)
                averageText.text = "Rata-rata: " + Mathf.RoundToInt(session.AverageScore());

            var latest = session != null ? session.GetResult(3) ?? session.GetResult(2) ?? session.GetResult(1) : null;
            if (strengthsText != null)
            {
                if (latest != null && latest.result != null && latest.result.strengths != null && latest.result.strengths.Length >= 2)
                    strengthsText.text = "✓ " + latest.result.strengths[0] + "\n✓ " + latest.result.strengths[1];
                else
                    strengthsText.text = "✓ Kamu sudah menyelesaikan sesi.\n✓ Kamu bisa mencoba kombinasi lain.";
            }
            if (improvementText != null)
                improvementText.text = "→ " + (latest != null && latest.result != null ? latest.result.improvement : "Tambahkan sayur di setiap makan.");
            if (closingText != null)
                closingText.text = "Mulai dari satu perubahan kecil: tambahkan sayur di setiap makan.";
            if (disclaimerText != null)
                disclaimerText.text = "Ini adalah game edukasi, bukan nasihat medis. Nilai gizi adalah perkiraan.";

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => onRestart?.Invoke());
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
            }
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
