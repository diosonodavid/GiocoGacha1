using System.Threading.Tasks;

namespace GachaGame.Core
{
    public interface IService
    {
        Task InitializeAsync();
        Task ShutdownAsync();
    }
}
