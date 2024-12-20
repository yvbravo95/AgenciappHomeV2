using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Agenciapp.Service.IReportServices
{
    public interface IReportOrders
    {
        Task<Result> GetUtility();
        Task<Result> GetLiquidation();
        Task<Result> GetSales();
    }

}