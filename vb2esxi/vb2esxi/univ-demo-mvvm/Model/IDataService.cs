using System.Threading.Tasks;

namespace univ_demo_mvvm.Model
{
    public interface IDataService
    {
        Task<DataItem> GetData();
    }
}