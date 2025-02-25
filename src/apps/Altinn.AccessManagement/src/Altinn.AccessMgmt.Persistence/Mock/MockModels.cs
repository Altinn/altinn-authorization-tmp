namespace Altinn.AccessMgmt.Repo.Mock;

public class MiniSpec
{
    public Dictionary<string, OrganizationSpec> IndustryOrganizationCount { get; set; }

}

public class OrganizationSpec
{
    public int AmountPercent { get; set; }

    public AssignmentSpec AssignmentSpec { get; set; }

    public OrganizationSpec(int size)
    {
        SetAssignmentSpec(size);
    }

    private void SetAssignmentSpec(int size, bool regnIsOrg = true, bool reviIsOrg = true)
    {
        int ansattPercent = 100;
        int styrePercent = 1;
        if (size < 10)
        {
            //// small
        }
        if (size >= 10 && size < 50)
        {
            //// medium
        }
        if (size >= 50 && size < 100)
        {
            //// large
        }
        if (size >= 100)
        {
            //// huge
        }

        AssignmentSpec = new AssignmentSpec()
        {
            Roles = new Dictionary<string, int>()
                {
                    { "ANSATT", 5 },
                    { "DAGL", 1 },
                    { "LEDE", 1 },
                    { "MEDL", 2 },
                    { "REGN", 1 },
                    { "REVI", 1 },
                },
            RegnIsOrg = regnIsOrg,
            ReviIsOrg = reviIsOrg
        };
    }
}

public class AssignmentSpec
{
    public Dictionary<string, int> Roles { get; set; }

    public bool RegnIsOrg { get; set; }

    public bool ReviIsOrg { get; set; }
}

public class DistributedRange(int size)
{
    public List<Range> Ranges { get; set; }
}

public class Range
{
    public int From { get; set; }

    public int To { get; set; }

    public int Value { get; set; }
}

public class MockModels
{

}

public class RangeBool(int percentTrue)
{
    public int PercentTrue { get; set; } = 50;
}

public class OldRange(int min, int max)
{
    public int Min { get; set; } = min;

    public int Max { get; set; } = max;
}

public class ExtBool
{
    public bool Har { get; set; }

    public bool Internal { get; set; }
}

public class OrgPack
{
    public string Industry { get; set; } // Bygg og anlegg
    public OldRange PacksOfIntrestPercent { get; set; } // 40%
    public OldRange EmployeePacks { get; set; } // 10%
    public OldRange EmployeeResourcePercent { get; set; } //5%

}

public class OrgSpec
{
    public OldRange Ansatte { get; set; }
    public OldRange Styre { get; set; }

    public int MinBoardSizeForDaglSameAsLede { get; set; } // 10%
    public int ExternalBoardMemberPercent { get; set; } //Hvor stor andel av styret er eksterne ikke ansatte

    /*

    Opprett et firma basert på OldRange's så lag en collection av de settene og sett en range på dem igjen.
    F.eks 20% små firma, 30%mellom, 30% store, 20% svære ... 

    Kanskje behandle grupperinger: ansatte, nøkkelpersoner, styre, externe, fremmede eksterne?

     */

    /*
     Assignmenst... => delegations %
     */


    public OldRange ExternalRegnDelegations { get; set; } // 1-5 personer i regnskapsfirma => 0: internal ?
    public OldRange ExternalReviDelegations { get; set; } // 1-5 personer i revisorfirma => 0: internal ?

    public OldRange InternalPackDelegations { get; set; }
    public OldRange ExternalPackDelegations { get; set; }

    public OldRange InternalResourceDelegations { get; set; }
    public OldRange ExternalResourceDelegations { get; set; }

    /*
    Regn Delegation => 1-5 ansatte hos regnskapsfører
    Revi Delegation => 1-5 ansatte hos revisorfirma
    */


    /*
     Firma X
    - 60 ansatte
    - 1 Daglig leder
    - 1 styreleder
    - 5 styremedlemmer
    - 1 regnskapsfører
    - 1 revisor

    Dagligleder og styreleder er samme person (om styret er mindre enn 10% av ansatte så er dagl og lede sammeperson)
    Regnskapsfører er et selskap
    Revisor er et selskap

    Firma X 
        - driver med BransjeY 
            - benytter seg av 40% av pakkene 
                - 10% av de ansatte har fått 20% av valgte pakker
                - 5% av de ansatte har fått tildelt 50% av ressursene i valgte pakker
        - driver med BrnasjeZ
            - ...

    Firma X
        - Har 2 personer på HR
        - Har 3 personer på Lønn

    Firma X 
        - Har delegert Lønn
        - Har delegert HR
     */
}
