using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InfinityRunner
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        public GameCoordinator coordinator;
        public GameObject menuRoot;
        public GameObject howToPanel;
        public CanvasGroup blackScreen;

        public void ShowMenu()
        {
            if (menuRoot != null)
            {
                menuRoot.SetActive(true);
            }

            if (howToPanel != null)
            {
                howToPanel.SetActive(false);
            }
        }

        public void HideMenu()
        {
            if (howToPanel != null)
            {
                howToPanel.SetActive(false);
            }

            if (menuRoot != null)
            {
                menuRoot.SetActive(false);
            }
        }

        public void StartGameButton()
        {
            if (coordinator != null)
            {
                coordinator.StartGameFromMenu();
            }
        }

        public void ExitGameButton()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OpenHowToButton()
        {
            if (howToPanel != null)
            {
                howToPanel.SetActive(true);
            }
        }

        public void CloseHowToButton()
        {
            if (howToPanel != null)
            {
                howToPanel.SetActive(false);
            }
        }

        public void HideBlackScreenImmediate()
        {
            SetBlackScreen(0f);
        }

        public IEnumerator FadeBlackScreenRoutine(float targetAlpha, float duration)
        {
            if (blackScreen == null)
            {
                yield break;
            }

            float startAlpha = blackScreen.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
                SetBlackScreen(Mathf.Lerp(startAlpha, targetAlpha, t));
                yield return null;
            }

            SetBlackScreen(targetAlpha);
        }

        private void SetBlackScreen(float alpha)
        {
            if (blackScreen == null)
            {
                return;
            }

            blackScreen.alpha = alpha;
            blackScreen.blocksRaycasts = alpha > 0.001f;
            blackScreen.interactable = alpha > 0.001f;
        }
    }
}
