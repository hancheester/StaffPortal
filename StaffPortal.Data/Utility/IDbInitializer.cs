using System.Threading.Tasks;

namespace StaffPortal.Data.Utility
{
    public interface IDbInitializer
    {
        Task Initialize();
    }
}