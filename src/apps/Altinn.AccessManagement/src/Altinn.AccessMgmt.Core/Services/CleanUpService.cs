namespace Altinn.AccessMgmt.Core.Services;

public interface ICleanUpService
{

}
public class CleanUpService
{
    /*
    
    Sjekk alle cleanup tabellene. 
    Om tiden nærmer seg => Send notifikasjon
    Om tiden er over => Fjern det refererte objektet


    Bruke samme felt som audit for å slippe å ha en modell til? Eller holder det med ID'er? Kan alt være i samme tabell? 
    Vi sletter ikke Requests før 12/18 mnd etter bruk. Da kan man enkelt be om det samme igjen.

    */

    public async Task Check()
    {
        await CheckAssignments();
    }

    public async Task CheckAssignments()
    {
        // Get all dbo.checkassignment hvor valid to nærmer seg eller er over
        // Om den fremdeles eksisterer så slett den
    }

    private async Task<Dictionary<Guid, DateTimeOffset>> CheckAssignmentPackages()
    {
        return default;
    }
}
