using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NutriPick.Content;
using NutriPick.Interaction;
using NutriPick.Nutrition;
using NutriPick.Player;
using NutriPick.UI;
using NutriPick.Utilities;

namespace NutriPick.Flow
{
    public class LevelFlowController : MonoBehaviour
    {
        public LevelDefinition level;
        public string firstSceneName = "MainVR";
        public NutritionRules rules;
        public PlateController plate;
        public FoodStand[] stands;
        public ServeStation serveStation;
        public FirstPersonController player;
        public ReticleInteractor reticle;
        public HeldFoodCarrier carrier;
        public GamepadInput input;
        public NutritionBar bar;
        public LevelTimerDisplay timer;
        public InteractionPrompt prompt;
        public FoodInfoCardPresenter infoCard;
        public FoodCardGridPresenter foodCards;
        public MealStatusBarPresenter mealStatus;
        public HealthReportPresenter report;
        public FinalResultPresenter finalResult;
        public RecapPresenter recap;
        public GameObject pausePanel;
        public TMP_Text pauseMessage;
        public Button pauseResumeButton;
        public Button pauseQuitButton;

        public bool IsPaused { get; private set; }
        public bool IsReportOpen { get; private set; }
        public bool IsLevelStarted => levelStarted;
        public GamepadInput Input => input;

        private float timeLeft;
        private float graceLeft;
        private bool inGrace;
        private bool timerRunning;
        private bool isServing;
        private bool disconnectPaused;
        private bool levelStarted;

        private void Start()
        {
            if (level == null)
            {
                Debug.LogError("[NutriPick] LevelFlowController has no LevelDefinition.");
                enabled = false;
                return;
            }

            if (player == null)
                player = FindAnyObjectByType<FirstPersonController>();
            if (input == null && player != null)
                input = player.GetComponent<GamepadInput>();
            if (reticle == null)
                reticle = FindAnyObjectByType<ReticleInteractor>();
            if (carrier == null && player != null)
                carrier = player.GetComponent<HeldFoodCarrier>();
            if (carrier == null)
                carrier = FindAnyObjectByType<HeldFoodCarrier>();
            if (plate == null)
                plate = FindAnyObjectByType<PlateController>();
            if (serveStation == null)
                serveStation = FindAnyObjectByType<ServeStation>();
            if (bar == null)
                bar = FindAnyObjectByType<NutritionBar>();
            if (timer == null)
                timer = FindAnyObjectByType<LevelTimerDisplay>();
            if (prompt == null)
                prompt = FindAnyObjectByType<InteractionPrompt>();
            if (infoCard == null)
                infoCard = FindAnyObjectByType<FoodInfoCardPresenter>();
            if (report == null)
                report = FindAnyObjectByType<HealthReportPresenter>();
            if (recap == null)
                recap = FindAnyObjectByType<RecapPresenter>();

            var session = GameSession.EnsureExists();
            session.BeginLevel(level.levelIndex);

            if (carrier != null)
                carrier.Changed += RefreshPreview;

            if (plate != null)
            {
                plate.minItems = level.minItems;
                plate.requiresDrink = level.requiresDrink;
                plate.PlateChanged += OnPlateChanged;
            }

            if (serveStation != null)
                serveStation.onServe.AddListener(TryServe);

            if (pauseResumeButton != null)
                pauseResumeButton.onClick.AddListener(Resume);
            if (pauseQuitButton != null)
                pauseQuitButton.onClick.AddListener(Quit);

            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (report != null)
                report.Hide();
            if (finalResult != null)
                finalResult.Hide();
            if (recap != null)
                recap.Hide();
            if (infoCard != null)
                infoCard.Hide();
            if (prompt != null)
                prompt.Hide();
            if (bar != null)
                bar.SetNeutral();

            StartLevel();
        }

        public void StartLevel()
        {
            if (levelStarted || level == null)
                return;

            levelStarted = true;
            if (player != null)
                player.SetControlEnabled(true);
            if (reticle != null)
                reticle.enabled = true;

            timeLeft = level.timeLimitSeconds;
            timerRunning = true;
            if (timer != null)
                timer.SetTime(timeLeft);

            if (foodCards != null)
                foodCards.Bind(level.foodPool, rules);
            if (mealStatus != null)
                mealStatus.SetNeutral();

            OnPlateChanged();
        }

        public void RebindLevel(LevelDefinition definition)
        {
            if (definition == null)
                return;

            level = definition;
            if (plate != null)
            {
                plate.minItems = definition.minItems;
                plate.requiresDrink = definition.requiresDrink;
            }
            if (foodCards != null)
                foodCards.Bind(definition.foodPool, rules);
            if (mealStatus != null)
                mealStatus.SetNeutral();
            OnPlateChanged();
        }

        private void Update()
        {
            if (!levelStarted)
                return;
            if (input == null)
                return;

            if (IsPaused)
            {
                if (input.PausePressed)
                    Resume();
                return;
            }

            if (IsReportOpen)
                return;

            if (input.PausePressed)
            {
                Pause(false, "Jeda");
                return;
            }

            if (input.GamepadConnected)
                disconnectPaused = false;
            else if (input.HadGamepad && !disconnectPaused)
            {
                disconnectPaused = true;
                Pause(true, "Controller terputus. Sambungkan kembali atau gunakan keyboard.");
                return;
            }

            if (input.ServePressed)
                TryServe();

            if (!timerRunning)
                return;

            if (inGrace)
            {
                graceLeft -= Time.deltaTime;
                if (timer != null)
                    timer.SetGrace(graceLeft);
                if (graceLeft <= 0f)
                {
                    inGrace = false;
                    if (plate == null || plate.Count == 0)
                        SceneLoader.Load(SceneManager.GetActiveScene().name);
                    else
                        ServeInternal(true);
                }
                return;
            }

            timeLeft -= Time.deltaTime;
            if (timer != null)
                timer.SetTime(timeLeft);

            if (timeLeft <= 0f)
            {
                if (plate != null && plate.CanServe(out _))
                {
                    ServeInternal(true);
                }
                else
                {
                    inGrace = true;
                    graceLeft = level.graceSeconds;
                    if (timer != null)
                        timer.SetGrace(graceLeft);
                    if (prompt != null)
                        prompt.Show("Waktu habis · Tambahkan makanan");
                }
            }
        }

        private void OnDestroy()
        {
            if (plate != null)
                plate.PlateChanged -= OnPlateChanged;
            if (carrier != null)
                carrier.Changed -= RefreshPreview;
            if (serveStation != null)
                serveStation.onServe.RemoveListener(TryServe);
        }

        private void OnPlateChanged()
        {
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (level == null || rules == null)
                return;

            var foods = new List<FoodData>();
            if (plate != null)
            {
                foreach (var food in plate.Foods)
                    foods.Add(food);
            }

            bool hasRoom = plate == null || plate.slots == null || plate.Count < plate.slots.Length;
            if (hasRoom && carrier != null && carrier.Current != null && carrier.Current.data != null)
                foods.Add(carrier.Current.data);

            var result = MealEvaluator.Evaluate(foods, rules, level.minItems, level.requiresDrink);
            if (mealStatus != null)
                mealStatus.SetPreview(result);
            else if (bar != null)
                bar.SetPreview(result);
        }

        public void TryServe()
        {
            if (!levelStarted)
                return;
            ServeInternal(false);
        }

        private void ServeInternal(bool auto)
        {
            if (isServing || IsReportOpen || plate == null)
                return;

            if (!auto && !plate.CanServe(out var reason))
            {
                if (prompt != null)
                    prompt.Show(reason);
                return;
            }

            isServing = true;
            timerRunning = false;
            inGrace = false;
            plate.Lock();
            if (serveStation != null)
                serveStation.SetEnabled(false);
            if (prompt != null)
                prompt.Hide();
            if (infoCard != null)
                infoCard.Hide();

            var result = MealEvaluator.Evaluate(plate.Foods, rules, level.minItems, level.requiresDrink);
            GameSession.Instance.RecordResult(level.levelIndex, result);
            if (mealStatus != null)
                mealStatus.SetFinal(result);
            else if (bar != null)
                bar.SetFinal(result);

            SfxPlayer.Instance?.PlayServe();
            GamepadRumble.Pulse(0.5f, 0.5f, 0.15f);
            StartCoroutine(ShowReportAfterDelay(result));
        }

        private IEnumerator ShowReportAfterDelay(MealResult result)
        {
            yield return new WaitForSeconds(1.0f);

            IsReportOpen = true;
            if (player != null)
                player.SetControlEnabled(false);
            if (reticle != null)
                reticle.enabled = false;
            if (finalResult != null)
            {
                finalResult.Show(result, level, Retry, Next);
            }
            else if (report != null)
            {
                System.Action recapAction = level.isFinalLevel ? (System.Action)Recap : null;
                report.Show(result, level, Retry, Next, recapAction);
            }
            SfxPlayer.Instance?.PlayReport();
        }

        private void Retry()
        {
            SceneLoader.Load(SceneManager.GetActiveScene().name);
        }

        private void Next()
        {
            if (!string.IsNullOrWhiteSpace(level.nextSceneName))
                SceneLoader.Load(level.nextSceneName);
            else
                Recap();
        }

        private void Recap()
        {
            if (report != null)
                report.Hide();
            if (recap != null)
                recap.Show(GameSession.Instance, Restart);
        }

        private void Restart()
        {
            if (GameSession.Instance != null)
                GameSession.Instance.ResetSession();
            SceneLoader.Load(firstSceneName);
        }

        private void Pause(bool force, string message)
        {
            if (IsPaused && !force)
                return;

            IsPaused = true;
            if (pausePanel != null)
                pausePanel.SetActive(true);
            if (pauseMessage != null)
                pauseMessage.text = message;
            if (player != null)
                player.SetControlEnabled(false);
            if (reticle != null)
                reticle.enabled = false;
            if (prompt != null)
                prompt.Hide();
        }

        private void Resume()
        {
            IsPaused = false;
            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (player != null)
                player.SetControlEnabled(true);
            if (reticle != null)
                reticle.enabled = true;
        }

        private void Quit()
        {
            Application.Quit();
        }

        public string GetServeReason()
        {
            if (plate == null)
                return "A: Sajikan";
            return plate.CanServe(out var reason) ? "A: Sajikan" : reason;
        }

        public void PlayPickup()
        {
            SfxPlayer.Instance?.PlayPickup();
            GamepadRumble.Pulse(0.4f, 0.4f, 0.2f);
        }

        public void PlayPlace()
        {
            SfxPlayer.Instance?.PlayPlace();
            GamepadRumble.Pulse(0.3f, 0.3f, 0.1f);
        }

        public void PlayDrop()
        {
            SfxPlayer.Instance?.PlayDrop();
            GamepadRumble.Pulse(0.2f, 0.2f, 0.08f);
        }
    }
}
