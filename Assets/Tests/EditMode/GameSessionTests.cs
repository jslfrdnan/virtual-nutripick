using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using NutriPick.Flow;
using NutriPick.Quests;

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

        private static QuestEvaluation Result(float protein, float calcium)
        {
            return new QuestEvaluation
            {
                totals = new NutritionTotals { protein = protein, calcium = calcium },
                tasks = new List<QuestTaskStatus>
                {
                    new QuestTaskStatus { label = "PROTEIN", passed = true }
                }
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
            session.RecordResult(1, Result(15f, 330f));
            Assert.AreEqual(15f, session.GetResult(1).totals.protein, 0.01f);
            Assert.AreEqual(1, session.GetResult(1).attemptCount);

            session.RecordResult(1, Result(22f, 400f));
            Assert.AreEqual(22f, session.GetResult(1).totals.protein, 0.01f);
            Assert.AreEqual(2, session.GetResult(1).attemptCount);
        }

        [Test]
        public void TotalNutritionSumsRecordedLevels()
        {
            session.BeginLevel(1);
            session.RecordResult(1, Result(15f, 330f));
            session.RecordResult(2, Result(25f, 170f));
            var totals = session.TotalNutrition();
            Assert.AreEqual(40f, totals.protein, 0.01f);
            Assert.AreEqual(500f, totals.calcium, 0.01f);
        }

        [Test]
        public void ResetClearsResultsAndQuest()
        {
            session.BeginLevel(1);
            session.RecordResult(1, Result(15f, 330f));
            session.SelectQuest(ScriptableObject.CreateInstance<QuestDefinition>());
            session.ResetSession();
            Assert.IsFalse(session.sessionActive);
            Assert.IsNull(session.GetResult(1));
            Assert.IsFalse(session.HasQuest);
            Assert.AreEqual(0f, session.TotalNutrition().protein, 0.01f);
        }
    }
}
