using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
        public InteractionPrompt prompt;
        public FoodInfoCardPresenter infoCard;
        public FoodCardGridPresenter foodCards;
        public MealStatusBarPresenter mealStatus;
        public HealthReportPresenter report;
        public FinalResultPresenter finalResult;
        public StartGuidePresenter startGuide;
        public FriedWarningPresenter friedWarning;
        public GameObject pausePanel;
        public TMP_Text pauseMessage;
        public Button pauseResumeButton;
        public Button pauseQuitButton;

        public bool IsPaused { get; private set; }
        public bool IsReportOpen { get; private set; }
        public bool IsLevelStarted => levelStarted;
        public GamepadInput Input => input;

        private Button pauseHelpButton;
        private TMP_Text sprintIndicator;
        private bool introFromPause;
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
            if (prompt == null)
                prompt = FindAnyObjectByType<InteractionPrompt>();
            if (infoCard == null)
                infoCard = FindAnyObjectByType<FoodInfoCardPresenter>();
            if (report == null)
                report = FindAnyObjectByType<HealthReportPresenter>();
            if (startGuide == null)
                startGuide = FindHudPresenter<StartGuidePresenter>();
            if (friedWarning == null)
                friedWarning = FindHudPresenter<FriedWarningPresenter>();

            var hud = GameObject.Find("FigmaHUD");
            if (hud != null && sprintIndicator == null)
                sprintIndicator = FindDeepChild(hud.transform, "SprintIndicator")?.GetComponent<TMP_Text>();

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
            {
                var help = FindDeepChild(pausePanel.transform, "HelpButton");
                if (help != null)
                {
                    pauseHelpButton = help.GetComponent<Button>();
                    if (pauseHelpButton != null)
                    {
                        pauseHelpButton.onClick.RemoveListener(ShowGuideFromPause);
                        pauseHelpButton.onClick.AddListener(ShowGuideFromPause);
                    }
                }
            }

            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (report != null)
                report.Hide();
            if (finalResult != null)
                finalResult.Hide();
            if (infoCard != null)
                infoCard.Hide();
            if (prompt != null)
                prompt.Hide();
            startGuide?.Hide();
            friedWarning?.Hide();
            if (sprintIndicator != null)
                sprintIndicator.gameObject.SetActive(false);

            if (level.levelIndex == 1 && startGuide != null && !session.HasSeenIntro)
            {
                OpenStartGuide(false);
            }
            else
            {
                StartLevel();
            }
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

            if (foodCards != null)
                foodCards.Bind(level.foodPool, rules);
            if (mealStatus != null)
                mealStatus.SetNeutral();
            SetGameplayHudVisible(true);

            OnPlateChanged();
        }

        private void SetGameplayHudVisible(bool visible)
        {
            if (foodCards != null)
                foodCards.gameObject.SetActive(visible);
            if (mealStatus != null)
                mealStatus.gameObject.SetActive(visible);
            if (prompt != null)
                prompt.gameObject.SetActive(visible);
            if (infoCard != null)
                infoCard.gameObject.SetActive(visible);
        }

        private void OpenStartGuide(bool fromPause)
        {
            if (startGuide == null)
            {
                if (fromPause)
                    Resume();
                else
                    StartLevel();
                return;
            }

            introFromPause = fromPause;
            if (pausePanel != null)
                pausePanel.SetActive(false);
            if (player != null)
                player.SetControlEnabled(false);
            if (reticle != null)
                reticle.enabled = false;
            if (prompt != null)
                prompt.Hide();
            SetGameplayHudVisible(false);

            startGuide.Show(OnStartGuideClosed);
        }

        private void ShowGuideFromPause()
        {
            if (!IsPaused || startGuide == null)
                return;

            OpenStartGuide(true);
        }

        private void OnStartGuideClosed()
        {
            if (introFromPause)
            {
                introFromPause = false;
                if (pausePanel != null)
                    pausePanel.SetActive(true);
                ConfigurePauseButtons();
                SelectPauseResume();
                return;
            }

            GameSession.Instance?.MarkIntroSeen();
            StartLevel();
        }

        private static T FindHudPresenter<T>() where T : Component
        {
            var hud = GameObject.Find("FigmaHUD");
            if (hud != null)
            {
                var found = hud.GetComponentInChildren<T>(true);
                if (found != null)
                    return found;
            }

            return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            foreach (var child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                    return child;
            }

            return null;
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
            if (sprintIndicator != null)
                sprintIndicator.gameObject.SetActive(levelStarted && !IsPaused && !IsReportOpen && player != null && player.IsSprinting);

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
            var requirement = MealRequirement.Evaluate(foods);
            if (!requirement.IsComplete)
                result.isIncomplete = true;
            if (mealStatus != null)
            {
                mealStatus.SetPreview(result);
                mealStatus.SetRequirements(requirement);
            }
        }

        public void TryServe()
        {
            if (!levelStarted)
                return;
            ServeInternal();
        }

        private void ServeInternal()
        {
            if (isServing || IsReportOpen || plate == null)
                return;

            if (!plate.CanServe(out var reason))
            {
                if (prompt != null)
                    prompt.Show(reason);
                return;
            }

            isServing = true;
            plate.Lock();
            if (serveStation != null)
                serveStation.SetEnabled(false);
            if (prompt != null)
                prompt.Hide();
            if (infoCard != null)
                infoCard.Hide();

            var foods = new List<FoodData>(plate.Foods);
            SfxPlayer.Instance?.PlayServe();

            if (MealRequirement.TryGetFriedFoods(foods, out var friedNames))
            {
                StartCoroutine(ShowFriedWarningAfterDelay(friedNames));
                return;
            }

            var result = MealEvaluator.Evaluate(foods, rules, level.minItems, level.requiresDrink);
            GameSession.Instance.RecordResult(level.levelIndex, result);
            if (mealStatus != null)
                mealStatus.SetFinal(result);

            StartCoroutine(ShowReportAfterDelay(result));
        }

        private IEnumerator ShowFriedWarningAfterDelay(List<string> friedNames)
        {
            yield return new WaitForSeconds(0.6f);

            IsReportOpen = true;
            if (player != null)
                player.SetControlEnabled(false);
            if (reticle != null)
                reticle.enabled = false;

            if (friedWarning != null)
            {
                friedWarning.Show(friedNames, Retry);
            }
            else
            {
                Debug.LogWarning("[NutriPick] Fried warning presenter missing.");
                Retry();
            }
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
                report.Show(result, level, Retry, Next);
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
                Restart();
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
            if (sprintIndicator != null)
                sprintIndicator.gameObject.SetActive(false);

            ConfigurePauseButtons();
            SelectPauseResume();
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
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
            SetGameplayHudVisible(true);
        }

        private void ConfigurePauseButtons()
        {
            EnsureFocus(pauseResumeButton);
            EnsureFocus(pauseHelpButton);
            EnsureFocus(pauseQuitButton);

            ConfigureVerticalNavigation(pauseResumeButton, null, pauseHelpButton);
            ConfigureVerticalNavigation(pauseHelpButton, pauseResumeButton, pauseQuitButton);
            ConfigureVerticalNavigation(pauseQuitButton, pauseHelpButton, null);
        }

        private static void EnsureFocus(Button button)
        {
            if (button == null)
                return;
            if (button.GetComponent<SelectableFocusHighlight>() == null)
                button.gameObject.AddComponent<SelectableFocusHighlight>();
        }

        private static void ConfigureVerticalNavigation(Button target, Button up, Button down)
        {
            if (target == null)
                return;

            var navigation = target.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = up != null ? up : target;
            navigation.selectOnDown = down != null ? down : target;
            navigation.selectOnLeft = null;
            navigation.selectOnRight = null;
            target.navigation = navigation;
        }

        private void SelectPauseResume()
        {
            if (EventSystem.current == null || pauseResumeButton == null)
                return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(pauseResumeButton.gameObject);
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
        }

        public void PlayPlace()
        {
            SfxPlayer.Instance?.PlayPlace();
        }

        public void PlayDrop()
        {
            SfxPlayer.Instance?.PlayDrop();
        }
    }
}
