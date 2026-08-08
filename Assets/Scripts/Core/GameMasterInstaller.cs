using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    // AppBootstrapper (existing) initializes services from a manually curated, hand-ordered
    // inspector list - the right fit when startup order matters and a scene author wants
    // explicit control over it. GameMasterInstaller instead auto-discovers every IService
    // MonoBehaviour already present in the scene via FindObjectsOfType and registers/initializes
    // them, for the common case where a scene doesn't need a curated order and would rather not
    // maintain a duplicate manual list alongside the actual GameObjects.
    public class GameMasterInstaller : MonoBehaviour
    {
        public async Task<IReadOnlyList<IService>> InstallAllAsync()
        {
            var installed = new List<IService>();
            var candidates = FindObjectsOfType<MonoBehaviour>();

            foreach (var candidate in candidates)
            {
                if (candidate is not IService service) continue;
                if (ServiceLocator.Instance.IsRegistered(candidate.GetType())) continue;

                ServiceLocator.Instance.Register(candidate.GetType(), service);
                await service.InitializeAsync();
                installed.Add(service);
            }

            Debug.Log($"{nameof(GameMasterInstaller)} installed {installed.Count} services.");
            return installed;
        }
    }
}
