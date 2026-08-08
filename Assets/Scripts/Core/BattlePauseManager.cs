using System;

namespace GachaGame.Core
{
    public enum BattlePauseState
    {
        Running,
        Paused,
        Abandoned
    }

    public class BattlePauseManager
    {
        public event Action<BattlePauseState> OnPauseStateChanged;

        public BattlePauseState State { get; private set; } = BattlePauseState.Running;
        public bool IsPaused => State == BattlePauseState.Paused;

        public void Pause()
        {
            if (State != BattlePauseState.Running) return;
            State = BattlePauseState.Paused;
            OnPauseStateChanged?.Invoke(State);
        }

        public void Resume()
        {
            if (State != BattlePauseState.Paused) return;
            State = BattlePauseState.Running;
            OnPauseStateChanged?.Invoke(State);
        }

        public void Abandon()
        {
            if (State == BattlePauseState.Abandoned) return;
            State = BattlePauseState.Abandoned;
            OnPauseStateChanged?.Invoke(State);
        }
    }
}
