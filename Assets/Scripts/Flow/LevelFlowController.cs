using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NutriPick.Content;
using NutriPick.Interaction;
using NutriPick.Nutrition;
using NutriPick.Player;
using NutriPick.Quests;
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
        public FinalResultPresenter finalResult;
        public StartGuidePresenter startGuide;
        public QuestSelectPresenter questSelect;
        public CampaignSummaryPresenter campaignSummary;
        public QuestDefinition quest;
        public QuestDefinition[] questOptions;
        public GameObject pausePanel;
        public TMP_Text pauseMessage;
        public Button pauseResumeButton;
        public Button pauseQuitButton;

        public bool IsPaused { get; private set; }
        public bool IsReportOpen { get; private set; }
        public bool IsLevelStarted => levelStarted;
        public GamepadInput Input => input;
        public QuestDefinition Quest => quest;

        private Button pauseHelpButton;
        private TMP_Text sprintIndicator;
        private bool introFromPause;
        private bool isServing;
        private bool disconnectPaused;
        private bool levelStarted;
        private string serveHint;
        private bool serveHintDirty = true;

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
            if (startGuide == null)
                startGuide = FindHudPresenter<StartGuidePresenter>();
            if (questSelect == null)
                questSelect = FindHudPresenter<QuestSelectPresenter>();
            if (campaignSummary == null)
                campaignSummary = FindHudPresenter<CampaignSummaryPresenter>();

            var hud = GameObject.Find("FigmaHUD");
            if (hud != null && sprintIndicator == null)
                sprintIndicator = FindDeepChild(hud.transform, "SprintIndicator")?.GetComponent<TMP_Text>();

            var session = GameSession.EnsureExists();
            session.BeginLevel(level.levelIndex);

            if (session.HasQuest)
                quest = session.quest;
            if (quest == null && level.levelIndex != 1)
                quest = ResolveDefaultQuest();
            if (quest != null)
                session.SelectQuest(quest);

            ApplyQuestToPlate();

            if (carrier != null)
                carrier.Changed += RefreshPreview;

            if (plate != null)
                plate.PlateChanged += OnPlateChanged;

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
            if (finalResult != null)
                finalResult.Hide();
            if (infoCard != null)
                infoCard.Hide();
            if (prompt != null)
                prompt.Hide();
            if (questSelect != null)
                questSelect.Hide();
            if (campaignSummary != null)
                campaignSummary.Hide();
            startGuide?.Hide();
            if (sprintIndicator != null)
                sprintIndicator.gameObject.SetActive(false);

            if (level.levelIndex == 1 && startGuide != null && !session.HasSeenIntro)
            {
                OpenStartGuide(false);
            }
            else if (!session.HasQuest && questSelect != null)
            {
                OpenQuestSelect();
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

            if (quest == null)
                quest = ResolveDefaultQuest();
            ApplyQuestToPlate();

            if (questSelect != null && questSelect.IsOpen)
                questSelect.Hide();

            levelStarted = true;
            if (player != null)
                player.SetControlEnabled(true);
            if (reticle != null)
                reticle.enabled = true;

            if (foodCards != null)
                foodCards.Bind(level.foodPool, rules);
            if (mealStatus != null)
                mealStatus.SetNeutral(quest, level.levelIndex);
            SetGameplayHudVisible(true);

            OnPlateChanged();
        }

        private void ApplyQuestToPlate()
        {
            if (plate == null)
                return;

            plate.quest = quest;
            plate.questLevelIndex = level != null ? level.levelIndex : 1;
            plate.minItems = ResolvePlateMinItems();
            plate.requiresDrink = level != null && level.requiresDrink;
            plate.SetVisualScale(quest != null ? quest.plateScale : 1f);
            InvalidateServeHint();
        }

        private int ResolvePlateMinItems()
        {
            if (quest != null && quest.plateMinItems > 0)
                return quest.plateMinItems;

            return level != null ? level.minItems : 5;
        }

        private void InvalidateServeHint()
        {
            serveHintDirty = true;
            serveHint = null;
        }

        private QuestDefinition ResolveDefaultQuest()
        {
            var session = GameSession.Instance;
            if (session != null && session.HasQuest)
                return session.quest;

            if (quest != null)
                return quest;

            if (questOptions != null)
            {
                foreach (var option in questOptions)
                {
                    if (option != null)
                        return option;
                }
            }

            return null;
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

            startGuide.SetQuestSummary(BuildQuestSummary());
            startGuide.Show(OnStartGuideClosed);
        }

        private string BuildQuestSummary()
        {
            var activeQuest = ResolveQuestForDisplay();
            if (activeQuest == null || activeQuest.levels == null)
                return "QUEST\nPilih kelompok saat game dimulai.";

            int index = Mathf.Clamp(level.levelIndex - 1, 0, activeQuest.levels.Length - 1);
            var tasks = activeQuest.levels[index];
            var builder = new StringBuilder();
            builder.Append("QUEST AKTIF\n");
            builder.Append(activeQuest.displayName).Append(" · usia ").Append(activeQuest.ageRange).Append('\n');
            builder.Append(tasks != null ? tasks.label.ToUpperInvariant() : "LEVEL").Append('\n');
            if (tasks != null)
            {
                builder.Append(QuestEvaluator.CategoryLabel(tasks.requiredA)).Append(" + ");
                builder.Append(QuestEvaluator.CategoryLabel(tasks.requiredB)).Append('\n');
                builder.Append(QuestEvaluator.NutrientLabel(tasks.targetA.nutrient)).Append(" ≥ ");
                builder.Append(QuestEvaluator.FormatAmount(tasks.targetA.minimum, tasks.targetA.nutrient)).Append('\n');
                builder.Append(QuestEvaluator.NutrientLabel(tasks.targetB.nutrient)).Append(" ≥ ");
                builder.Append(QuestEvaluator.FormatAmount(tasks.targetB.minimum, tasks.targetB.nutrient));
            }

            return builder.ToString();
        }

        private QuestDefinition ResolveQuestForDisplay()
        {
            if (quest != null)
                return quest;
            if (GameSession.Instance != null && GameSession.Instance.HasQuest)
                return GameSession.Instance.quest;
            return ResolveDefaultQuest();
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
            if (GameSession.Instance != null && !GameSession.Instance.HasQuest && questSelect != null)
                OpenQuestSelect();
            else
                StartLevel();
        }

        private void OpenQuestSelect()
        {
            if (questSelect == null)
            {
                StartLevel();
                return;
            }

            if (player != null)
                player.SetControlEnabled(false);
            if (reticle != null)
                reticle.enabled = false;
            if (prompt != null)
                prompt.Hide();
            SetGameplayHudVisible(false);

            questSelect.Show(questOptions, OnQuestChosen);
        }

        private void OnQuestChosen(QuestDefinition chosen)
        {
            if (chosen == null)
                return;

            quest = chosen;
            GameSession.Instance?.SelectQuest(chosen);
            ApplyQuestToPlate();
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
            ApplyQuestToPlate();
            if (foodCards != null)
                foodCards.Bind(definition.foodPool, rules);
            if (mealStatus != null)
                mealStatus.SetNeutral(quest, definition.levelIndex);
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
            InvalidateServeHint();
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (level == null)
                return;

            var foods = new List<FoodData>();
            if (plate != null)
            {
                foreach (var food in plate.Foods)
                    foods.Add(food);
            }

            bool hasRoom = plate == null || plate.Count < plate.Capacity;
            if (hasRoom && carrier != null && carrier.Current != null && carrier.Current.data != null)
                foods.Add(carrier.Current.data);

            var evaluation = QuestEvaluator.Evaluate(foods, quest, level.levelIndex);
            if (mealStatus != null)
                mealStatus.SetEvaluation(evaluation);
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

            if (!plate.CanServe(out _))
            {
                if (prompt != null)
                    prompt.Show(GetServeReason());
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

            var evaluation = QuestEvaluator.Evaluate(foods, quest, level.levelIndex);
            GameSession.Instance.RecordResult(level.levelIndex, evaluation);
            if (mealStatus != null)
                mealStatus.SetEvaluation(evaluation);

            StartCoroutine(ShowReportAfterDelay(evaluation));
        }

        private IEnumerator ShowReportAfterDelay(QuestEvaluation evaluation)
        {
            yield return new WaitForSeconds(1.0f);

            IsReportOpen = true;
            if (player != null)
                player.SetControlEnabled(false);
            if (reticle != null)
                reticle.enabled = false;
            if (finalResult != null)
                finalResult.Show(evaluation, level, Retry, Next);
            SfxPlayer.Instance?.PlayReport();
        }

        private void Retry()
        {
            SceneLoader.Load(SceneManager.GetActiveScene().name);
        }

        private void Next()
        {
            if (level.isFinalLevel)
            {
                ShowCampaignSummary();
                return;
            }

            if (!string.IsNullOrWhiteSpace(level.nextSceneName))
                SceneLoader.Load(level.nextSceneName);
            else
                Restart();
        }

        private void ShowCampaignSummary()
        {
            var session = GameSession.Instance;
            if (campaignSummary == null || session == null)
            {
                Restart();
                return;
            }

            if (finalResult != null)
                finalResult.Hide();
            IsReportOpen = true;
            campaignSummary.Show(quest, session.results, session.TotalNutrition(), Restart);
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

            if (plate.CanServe(out var reason))
            {
                InvalidateServeHint();
                return "A: Sajikan";
            }

            if (serveHintDirty || string.IsNullOrEmpty(serveHint))
            {
                serveHint = QuestHintBuilder.Build(
                    plate.Evaluation, level, plate.Foods, plate.RequiredItems);
                if (string.IsNullOrWhiteSpace(serveHint))
                    serveHint = reason;
                serveHintDirty = false;
            }

            return serveHint;
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
