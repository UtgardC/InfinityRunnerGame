using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace InfinityRunner
{
    public sealed class RunnerInputReader : MonoBehaviour
    {
        public RunnerConfig config;
        public GameCoordinator coordinator;

        private bool touchActive;
        private Vector2 touchStartPosition;
        private float touchStartTime;

        private void Update()
        {
            if (coordinator == null || config == null)
            {
                return;
            }

            ReadKeyboard();
            ReadTouch();
        }

        private void ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                coordinator.RequestLaneChange(-1);
            }

            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                coordinator.RequestLaneChange(1);
            }

            if (keyboard.spaceKey.wasPressedThisFrame
                || keyboard.enterKey.wasPressedThisFrame
                || keyboard.wKey.wasPressedThisFrame
                || keyboard.upArrowKey.wasPressedThisFrame)
            {
                coordinator.RequestJump();
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                coordinator.RequestRestart();
            }

            if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                coordinator.RequestFastFall();
            }
        }

        private void ReadTouch()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return;
            }

            TouchControl primaryTouch = touchscreen.primaryTouch;
            bool isPressed = primaryTouch.press.isPressed;

            if (isPressed && !touchActive)
            {
                touchActive = true;
                touchStartPosition = primaryTouch.position.ReadValue();
                touchStartTime = Time.unscaledTime;
                return;
            }

            if (!isPressed && touchActive)
            {
                touchActive = false;
                Vector2 endPosition = primaryTouch.position.ReadValue();
                Vector2 delta = endPosition - touchStartPosition;
                float duration = Time.unscaledTime - touchStartTime;

                if (Mathf.Abs(delta.x) >= config.touchSwipeThresholdPixels && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    coordinator.RequestLaneChange(delta.x > 0f ? 1 : -1);
                }
                else if (duration <= config.touchTapMaxDuration)
                {
                    coordinator.RequestJump();
                }
            }
        }
    }
}
