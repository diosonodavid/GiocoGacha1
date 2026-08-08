using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    public enum GameState
    {
        Boot,
        TitleScreen,
        Home,
        Combat,
        Loading
    }

    // Single source of truth for which top-level app state is active; screens/systems subscribe
    // to OnStateChanged rather than polling, mirroring GuildWarManager's phase-change event
    // pattern (Preparation/Attack/Concluded) at the whole-app scope instead of a single feature.
    public class GameStateController : MonoBehaviour, IService
    {
        public event Action<GameState, GameState> OnStateChanged; // previous, next

        public GameState CurrentState { get; private set; } = GameState.Boot;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(GameStateController)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void SetState(GameState nextState)
        {
            if (nextState == CurrentState) return;

            var previous = CurrentState;
            CurrentState = nextState;
            OnStateChanged?.Invoke(previous, nextState);
        }
    }
}
