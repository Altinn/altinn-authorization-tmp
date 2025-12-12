using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public interface IConnectionStringSelector
{
    string GetConnectionString();
}
