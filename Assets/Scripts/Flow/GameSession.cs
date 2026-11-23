using System;
using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.Flow
{
    [Serializable]
    public class LevelResult
    {
        public string levelId;
        public int levelIndex;
        public string displayName;
        public int score;
        public ScoreBand band;
        public MealResult result;
        public int attemptCount;
    }

    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        public bool sessionActive;
        public int currentLevelIndex;
        public LevelResult[] results = new LevelResult[3];
        public bool introSeen;

        public bool HasSeenIntro => introSeen;

        public void MarkIntroSeen()
        {
            introSeen = true;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static GameSession EnsureExists()
        {
            if (Instance != null)
                return Instance;

            var existing = FindAnyObjectByType<GameSession>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }

            var go = new GameObject("GameSession");
            Instance = go.AddComponent<GameSession>();
            DontDestroyOnLoad(go);
            return Instance;
        }

        public void BeginLevel(int levelIndex)
        {
            if (levelIndex == 1 && !sessionActive)
                ResetSession();

            currentLevelIndex = levelIndex;
            sessionActive = true;
        }

        public void RecordResult(int levelIndex, MealResult result)
        {
            if (levelIndex < 1 || levelIndex > 3)
                return;

            var previous = results[levelIndex - 1];
            results[levelIndex - 1] = new LevelResult
            {
                levelId = "level_" + levelIndex,
                levelIndex = levelIndex,
                displayName = "Level " + levelIndex,
                score = result.score,
                band = result.band,
                result = result,
                attemptCount = (previous != null ? previous.attemptCount : 0) + 1
            };
        }

        public LevelResult GetResult(int levelIndex)
        {
            if (levelIndex < 1 || levelIndex > 3)
                return null;
            return results[levelIndex - 1];
        }

        public bool HasResult(int levelIndex)
        {
            return GetResult(levelIndex) != null;
        }

        public void ResetSession()
        {
            sessionActive = false;
            currentLevelIndex = 0;
            results = new LevelResult[3];
            introSeen = false;
        }

        public float AverageScore()
        {
            int count = 0;
            int total = 0;
            foreach (var result in results)
            {
                if (result == null)
                    continue;
                count++;
                total += result.score;
            }

            return count == 0 ? 0f : total / (float)count;
        }
    }
}
