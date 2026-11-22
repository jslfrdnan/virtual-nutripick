using TMPro;
using UnityEngine;
using NutriPick.Player;

namespace NutriPick.Content
{
    /// <summary>
    /// Shows a world-space speech bubble when the player is near the stand and reacts
    /// when the player picks one of that stand's foods.
    /// </summary>
    public class StandVendor : MonoBehaviour
    {
        public FoodStand stand;
        public Transform player;
        public GameObject bubbleRoot;
        public TMP_Text bubbleText;
        public string[] greetings;
        public string[] reactions;
        public float radius = 3.0f;
        public float lineSeconds = 5f;

        private StandPopularityTracker tracker;
        private int lineIndex;
        private float lineTimer;
        private float reactionTimer;
        private bool visible;

        private void Start()
        {
            if (stand == null)
                stand = GetComponentInParent<FoodStand>();

            if (player == null)
            {
                var controller = FindAnyObjectByType<FirstPersonController>();
                if (controller != null)
                    player = controller.transform;
            }

            tracker = FindAnyObjectByType<StandPopularityTracker>();
            if (tracker != null)
                tracker.StandPicked += OnStandPicked;

            if (bubbleRoot != null)
                bubbleRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (tracker != null)
                tracker.StandPicked -= OnStandPicked;
        }

        private void Update()
        {
            if (player == null || bubbleRoot == null)
                return;

            bool inRange = DistanceXZ(player.position, transform.position) <= radius;

            if (inRange && !visible)
            {
                visible = true;
                bubbleRoot.SetActive(true);
                lineTimer = 0f;
                reactionTimer = 0f;
                ShowGreeting();
            }
            else if (!inRange && visible)
            {
                visible = false;
                bubbleRoot.SetActive(false);
                reactionTimer = 0f;
            }

            if (!visible)
                return;

            if (reactionTimer > 0f)
            {
                reactionTimer -= Time.deltaTime;
                if (reactionTimer <= 0f)
                    ShowGreeting();
                return;
            }

            lineTimer += Time.deltaTime;
            if (lineTimer >= lineSeconds)
            {
                lineTimer = 0f;
                ShowGreeting();
            }
        }

        private void OnStandPicked(FoodStand picked)
        {
            if (picked != stand || !visible || bubbleText == null)
                return;

            if (reactions == null || reactions.Length == 0)
                return;

            bubbleText.text = reactions[Random.Range(0, reactions.Length)];
            reactionTimer = 4f;
        }

        private void ShowGreeting()
        {
            if (bubbleText == null || greetings == null || greetings.Length == 0)
                return;

            bubbleText.text = greetings[lineIndex % greetings.Length];
            lineIndex++;
        }

        private static float DistanceXZ(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
