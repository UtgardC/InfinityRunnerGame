using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace InfinityRunner
{
    public sealed class RunnerUI : MonoBehaviour
    {
        public GameCoordinator coordinator;

        private Canvas canvas;
        private GameObject menuPanel;
        private GameObject hudPanel;
        private GameObject gameOverPanel;
        private GameObject clashPanel;
        private Text scoreText;
        private Text stageText;
        private Text gameOverScoreText;
        private Slider clashSlider;

        private void Awake()
        {
            BuildIfNeeded();
        }

        public void SetCoordinator(GameCoordinator targetCoordinator)
        {
            coordinator = targetCoordinator;
        }

        public void ShowMenu()
        {
            BuildIfNeeded();
            menuPanel.SetActive(true);
            hudPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            clashPanel.SetActive(false);
        }

        public void ShowRunning()
        {
            BuildIfNeeded();
            menuPanel.SetActive(false);
            hudPanel.SetActive(true);
            gameOverPanel.SetActive(false);
            clashPanel.SetActive(false);
        }

        public void ShowGameOver(int score)
        {
            BuildIfNeeded();
            menuPanel.SetActive(false);
            hudPanel.SetActive(false);
            gameOverPanel.SetActive(true);
            clashPanel.SetActive(false);
            gameOverScoreText.text = "Puntaje: " + score;
        }

        public void ShowClash()
        {
            BuildIfNeeded();
            menuPanel.SetActive(false);
            hudPanel.SetActive(true);
            gameOverPanel.SetActive(false);
            clashPanel.SetActive(true);
            SetClashProgress(0f);
        }

        public void SetClashProgress(float progress)
        {
            if (clashSlider != null)
            {
                clashSlider.value = Mathf.Clamp01(progress);
            }
        }

        public void UpdateHud(int score, float distance, DifficultyStage stage)
        {
            if (scoreText != null)
            {
                scoreText.text = "Puntos: " + score + "\nDistancia: " + Mathf.FloorToInt(distance) + " m";
            }

            if (stageText != null)
            {
                stageText.text = "Etapa: " + StageLabel(stage);
            }
        }

        private void BuildIfNeeded()
        {
            if (canvas != null)
            {
                return;
            }

            EnsureEventSystem();

            GameObject canvasObject = new GameObject("Runner UI");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            menuPanel = CreatePanel("Menu", new Color(0f, 0f, 0f, 0.35f));
            Text title = CreateText(menuPanel.transform, "Title", "ROCA DIVINA", 56, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0f, 0f), new Vector2(640f, 90f));
            Button playButton = CreateButton(menuPanel.transform, "Play Button", "Jugar");
            SetRect(playButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(220f, 72f));
            playButton.onClick.AddListener(OnPlayPressed);

            hudPanel = CreatePanel("HUD", new Color(0f, 0f, 0f, 0f));
            scoreText = CreateText(hudPanel.transform, "Score", "Puntos: 0", 26, TextAnchor.UpperLeft);
            SetRect(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(360f, 90f));
            stageText = CreateText(hudPanel.transform, "Stage", "Etapa: Inicio", 24, TextAnchor.UpperRight);
            SetRect(stageText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(280f, 50f));

            clashPanel = CreatePanel("Clash", new Color(0f, 0f, 0f, 0f));
            Text clashText = CreateText(clashPanel.transform, "Clash Text", "ROMPE LA MURALLA", 30, TextAnchor.MiddleCenter);
            SetRect(clashText.rectTransform, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(520f, 60f));
            clashSlider = CreateSlider(clashPanel.transform, "Clash Progress");
            SetRect(clashSlider.GetComponent<RectTransform>(), new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.13f), Vector2.zero, new Vector2(460f, 28f));

            gameOverPanel = CreatePanel("Game Over", new Color(0f, 0f, 0f, 0.55f));
            Text gameOverTitle = CreateText(gameOverPanel.transform, "Game Over Title", "GAME OVER", 52, TextAnchor.MiddleCenter);
            SetRect(gameOverTitle.rectTransform, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero, new Vector2(540f, 80f));
            gameOverScoreText = CreateText(gameOverPanel.transform, "Final Score", "Puntaje: 0", 30, TextAnchor.MiddleCenter);
            SetRect(gameOverScoreText.rectTransform, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(540f, 60f));
            Button retryButton = CreateButton(gameOverPanel.transform, "Retry Button", "Reintentar");
            SetRect(retryButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), Vector2.zero, new Vector2(240f, 72f));
            retryButton.onClick.AddListener(OnRetryPressed);
        }

        private GameObject CreatePanel(string panelName, Color color)
        {
            GameObject panel = new GameObject(panelName);
            panel.transform.SetParent(canvas.transform, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (color.a > 0f)
            {
                Image image = panel.AddComponent<Image>();
                image.color = color;
            }

            return panel;
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        private Text CreateText(Transform parent, string objectName, string value, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private Button CreateButton(Transform parent, string objectName, string label)
        {
            GameObject buttonObject = new GameObject(objectName);
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.84f, 0.1f, 0.08f, 0.95f);
            Button button = buttonObject.AddComponent<Button>();

            Text text = CreateText(buttonObject.transform, "Label", label, 30, TextAnchor.MiddleCenter);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            return button;
        }

        private Slider CreateSlider(Transform parent, string objectName)
        {
            GameObject sliderObject = new GameObject(objectName);
            sliderObject.transform.SetParent(parent, false);
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;

            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObject.transform, false);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            RectTransform bgRect = bgImage.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(4f, 4f);
            fillAreaRect.offsetMax = new Vector2(-4f, -4f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.9f, 0.08f, 0.02f, 1f);
            RectTransform fillRect = fillImage.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            return slider;
        }

        private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private void OnPlayPressed()
        {
            if (coordinator != null)
            {
                coordinator.StartGameFromMenu();
            }
        }

        private void OnRetryPressed()
        {
            if (coordinator != null)
            {
                coordinator.RestartToMenu();
            }
        }

        private string StageLabel(DifficultyStage stage)
        {
            switch (stage)
            {
                case DifficultyStage.Start:
                    return "Inicio";
                case DifficultyStage.Middle:
                    return "Medio";
                case DifficultyStage.Late:
                    return "Tarde";
                default:
                    return stage.ToString();
            }
        }
    }
}
