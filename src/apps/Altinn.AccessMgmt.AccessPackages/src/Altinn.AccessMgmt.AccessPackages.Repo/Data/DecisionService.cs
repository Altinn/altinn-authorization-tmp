using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data;

public class DecisionService
{

    /*
     Hent alle resources for en Provider/ResourceGroup
     
     */

    public async Task<List<ExtResource>> GetResources(Guid currentId, Guid fromId, Guid toId, Guid providerId)
    {
        var result = new List<ExtResource>();

        

        /*
        Hent alle Assignments fra From til To som Current har lov å se
        */

        return result;
    }

    public async Task<bool> Check(Guid currentId, Guid fromId, Guid resourceId)
    {
        

        

        return false;
    }


}
