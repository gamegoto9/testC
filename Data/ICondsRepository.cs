using System.Data;
using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public interface ICondsRepository
    {
       public DataSet ExecuteYourStoredProcedure();
       public IEnumerable<MMT> CallYourStoredProcedure();
       public HomeItem getHomeProcude();
       public CondItem getCond();
    }
}
