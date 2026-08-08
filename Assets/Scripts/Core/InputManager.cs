using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GachaGame.Core
{
    // Exposes one pointer-position/press API regardless of whether the platform provides a touch
    // screen or a mouse - reads exclusively through the new Input System (UnityEngine.InputSystem),
    // since this project's Active Input Handling is set to "Input System Package (New)" only (see
    // ProjectSettings.asset: activeInputHandler: 1); legacy Input.* calls throw here at runtime,
    // the same issue fixed earlier in InGameConsole.cs.
    public class InputManager : MonoBehaviour, IService
    {
        public event Action<Vector2> OnPointerDown;
        public event Action<Vector2> OnPointerUp;

        public Vector2 PointerPosition { get; private set; }
        public bool IsPointerDown { get; private set; }

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(InputManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        private void Update()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                PointerPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                UpdatePressState(true);
                return;
            }

            if (Mouse.current != null)
            {
                PointerPosition = Mouse.current.position.ReadValue();
                UpdatePressState(Mouse.current.leftButton.isPressed);
            }
        }

        private void UpdatePressState(bool isDown)
        {
            if (isDown && !IsPointerDown) OnPointerDown?.Invoke(PointerPosition);
            else if (!isDown && IsPointerDown) OnPointerUp?.Invoke(PointerPosition);

            IsPointerDown = isDown;
        }
    }
}
