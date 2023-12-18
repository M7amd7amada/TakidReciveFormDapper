using System.Data;

namespace TakidReciveForm.DataAccess.Data;

public interface IDapperContext
{
    public IDbConnection CreateConnection();
}