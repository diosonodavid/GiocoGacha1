using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    // Assign the game's MonoBehaviour services in the inspector in the exact order they must
    // initialize; this drives the fixed startup sequence for the whole game.
    public class AppBootstrapper : MonoBehaviour
    {
        [SerializeField] private List<MonoBehaviour> serviceBehaviours = new();

        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);
            await InitializeServicesAsync();
        }

        private async void OnApplicationQuit()
        {
            await ShutdownServicesAsync();
        }

        private async Task InitializeServicesAsync()
        {
            foreach (var behaviour in serviceBehaviours)
            {
                if (behaviour is not IService service)
                {
                    Debug.LogWarning($"{behaviour.name} does not implement {nameof(IService)} and will be skipped.", behaviour);
                    continue;
                }

                ServiceLocator.Instance.Register(behaviour.GetType(), service);
                await service.InitializeAsync();
            }
        }

        private async Task ShutdownServicesAsync()
        {
            for (int i = serviceBehaviours.Count - 1; i >= 0; i--)
            {
                if (serviceBehaviours[i] is IService service)
                    await service.ShutdownAsync();
            }
        }
    }
}
