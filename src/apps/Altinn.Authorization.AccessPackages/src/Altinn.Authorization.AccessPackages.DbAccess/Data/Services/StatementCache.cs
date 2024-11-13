namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Services;

public static class StatementCache
{
    private static List<Statement> Statemensts { get; set; } = new List<Statement>();

    public static void Set<T>(string statement, string[] filtercolumns, bool useTranslation, bool useHistory)
    {
        Statemensts.Add(new Statement()
        {
            Type = typeof(T),
            Query = statement,
            UseHistory = useHistory,
            UseTranslation = useTranslation,
            Filters = filtercolumns
        });
    }

    public static string? Get<T>(string[] filtercolumns, bool useTranslation, bool useHistory)
    {
        return Statemensts.Where(t =>
            t.Type == typeof(T) &&
            t.UseTranslation == useTranslation &&
            t.UseHistory == useHistory &&
            t.Filters == filtercolumns
            ).First().Query ?? null;
    }
}

public class Statement
{
    public Type Type { get; set; }
    public string Query { get; set; }
    public bool UseTranslation { get; set; }
    public bool UseHistory { get; set; }
    public string[] Filters { get; set; }
}
