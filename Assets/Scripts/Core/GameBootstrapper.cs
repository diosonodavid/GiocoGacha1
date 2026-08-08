using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    // The game's single entry point. AppBootstrapper is a reusable "initialize this ordered list
    // of services" building block that any scene can opt into; GameBootstrapper is the concrete
    // MonoBehaviour meant to live once in the boot scene. It drives GameMasterInstaller's
    // auto-discovery installation, then hands off to GameStateController's first real transition
    // once every service has finished initializing.
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameMasterInstaller installer;
        [SerializeField] private GameStateController stateController;

        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);
            await BootAsync();
        }

        private async Task BootAsync()
        {
            if (installer != null) await installer.InstallAllAsync();

            if (stateController == null) ServiceLocator.Instance.TryGet(out stateController);
            stateController?.SetState(GameState.TitleScreen);
        }
    }
}
