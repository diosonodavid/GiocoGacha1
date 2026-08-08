using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GachaGame.Utilities
{
    // On-screen scrolling log terminal for test builds, toggled with the backquote key; renders
    // via OnGUI (not a UGUI prefab) so it works without any scene wiring in every build that
    // ships it. Self-destructs outside dev/editor builds so it never reaches players. Uses the new
    // Input System (Keyboard.current) rather than the legacy Input class, since this project's
    // Active Input Handling is set to "Input System Package (New)" only - legacy Input calls would
    // throw at runtime here.
    public class InGameConsole : MonoBehaviour
    {
        [SerializeField] private int maxLines = 100;
        [SerializeField] private Key toggleKey = Key.Backquote;

        private readonly List<string> logLines = new();
        private Vector2 scrollPosition;
        private bool isVisible;

        private void Awake()
        {
            if (!Debug.isDebugBuild)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable() => Application.logMessageReceived += HandleLog;
        private void OnDisable() => Application.logMessageReceived -= HandleLog;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
                isVisible = !isVisible;
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            logLines.Add($"[{type}] {condition}");
            if (logLines.Count > maxLines) logLines.RemoveAt(0);
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height * 0.4f), GUI.skin.box);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (var line in logLines)
                GUILayout.Label(line);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
