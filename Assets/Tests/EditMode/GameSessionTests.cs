using NUnit.Framework;
using UnityEngine;
using NutriPick.Flow;
using NutriPick.Nutrition;

namespace NutriPick.Tests
{
    public class GameSessionTests
    {
        private GameObject go;
        private GameSession session;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("GameSessionTest");
            session = go.AddComponent<GameSession>();
        }

        [TearDown]
        public void TearDown()
        {
            if (go != null)
                Object.DestroyImmediate(go);
        }

        private static MealResult Result(int score)
        {
            return new MealResult
            {
                score = score,
                band = ScoreBand.Adequate,
                bandLabel = "Cukup",
                strengths = new[] { "Satu", "Dua" },
                improvement = "Tambah sayur."
            };
        }

        [Test]
        public void BeginLevelOneStartsCleanSession()
        {
            session.BeginLevel(1);
            Assert.IsTrue(session.sessionActive);
            Assert.AreEqual(1, session.currentLevelIndex);
            Assert.IsNull(session.GetResult(1));
        }

        [Test]
        public void RecordResultStoresLatestAttempt()
        {
            session.BeginLevel(1);
            session.RecordResult(1, Result(55));
            Assert.AreEqual(55, session.GetResult(1).score);
            Assert.AreEqual(1, session.GetResult(1).attemptCount);

            session.RecordResult(1, Result(75));
            Assert.AreEqual(75, session.GetResult(1).score);
            Assert.AreEqual(2, session.GetResult(1).attemptCount);
        }

        [Test]
        public void AverageUsesRecordedLevels()
        {
            session.BeginLevel(1);
            session.RecordResult(1, Result(60));
            session.RecordResult(2, Result(80));
            Assert.AreEqual(70f, session.AverageScore(), 0.01f);
        }

        [Test]
        public void ResetClearsResults()
        {
            session.BeginLevel(1);
            session.RecordResult(1, Result(60));
            session.ResetSession();
            Assert.IsFalse(session.sessionActive);
            Assert.IsNull(session.GetResult(1));
            Assert.AreEqual(0f, session.AverageScore(), 0.01f);
        }
    }
}
