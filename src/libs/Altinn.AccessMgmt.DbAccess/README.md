# AccessPackages - DbAccess

## Concepts
The datamodel is closely coupled with the database. There are three core concepts Basic, Extended and Cross. **Basic** has a one-to-one mapping from class properties to table columns. While **Extended** extend the Basic classes with related objects using foreign keys to join data together. **Cross** enables queries on many-to-many relationship tables to return one side based on the other.

## Contracts
There are two main sets of contracts in DbAccess; **Data** and **Repo**. The **Repo Services** connects to the database while **Data Service** contains a repo service to keep the database implementation seperate from the data service implementation. Doing this helps seperate object implementation to the diffrent database implementations.

### Data Services

#### IBasicDataService
```
public interface IBasicDataService<T>
{
    IDbBasicRepo<T> Repo { get; }
}
```

#### IExtendedDataService
```
public interface IExtendedDataService<T, TExtended> : IBasicDataService<T>
{
    IDbExtendedRepo<T, TExtended> ExtendedRepo { get; }
}
```

#### ICrossDataService
```
public interface ICrossDataService<TA, T, TB> : IBasicDataService<T>
{
    IDbCrossRepo<TA, T, TB> CrossRepo { get; }
}
```

### Repo Services

#### IDbBasicRepo
```
public interface IDbBasicRepo<T>
{
    Task<IEnumerable<T>> Get(RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<T?> Get(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default);
    
    Task<(IEnumerable<T> Data, PagedResult PageInfo)> Search(SearchOptions term, RequestOptions options, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> Search(string term, RequestOptions? options = null, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<T>> Get(string property, Guid value, RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> Get(string property, int value, RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> Get(string property, string value, RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> Get(Dictionary<string, object> parameters, RequestOptions? options = null, CancellationToken cancellationToken = default);
    
    Task<int> Create(T entity, CancellationToken cancellationToken = default);
    Task<int> Create(List<T> entity, CancellationToken cancellationToken = default);
    
    Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default);
    Task<int> Update(Guid id, string property, string value, CancellationToken cancellationToken = default);
    Task<int> Update(Guid id, string property, int value, CancellationToken cancellationToken = default);
    Task<int> Update(Guid id, string property, Guid value, CancellationToken cancellationToken = default);
    
    Task<int> Delete(Guid id, CancellationToken cancellationToken = default);
    
    Task<int> CreateTranslation(T entity, string language, CancellationToken cancellationToken = default);
    Task<int> UpdateTranslation(Guid id, T entity, string language, CancellationToken cancellationToken = default);
}
```

#### IDbExtendedRepo
```
public interface IDbExtendedRepo<T, TExtended> : IDbBasicRepo<T>
{
    Task<(IEnumerable<TExtended> Data, PagedResult PageInfo)> SearchExt(SearchOptions term, RequestOptions options);

    Task<IEnumerable<TExtended>> GetExtended(RequestOptions? options = null);
    Task<IEnumerable<TExtended>> GetExtended(string property, Guid value, RequestOptions? options = null);
    Task<IEnumerable<TExtended>> GetExtended(string property, int value, RequestOptions? options = null);
    Task<IEnumerable<TExtended>> GetExtended(string property, string value, RequestOptions? options = null);
    Task<TExtended?> GetExtended(Guid id, RequestOptions? options = null);

    void Join<TJoin>(string alias = "", string baseJoinProperty = "", string joinProperty = "Id", bool optional = false);
}
```

#### IDbCrossRepo
```
public interface IDbCrossRepo<TA, T, TB> : IDbBasicRepo<T>
{
    Task<IEnumerable<T>> GetX(Guid id, RequestOptions? options = null);
    Task<IEnumerable<TA>> GetA(Guid BId, RequestOptions? options = null);
    Task<IEnumerable<TB>> GetB(Guid AId, RequestOptions? options = null);
    
    void SetCrossColumns(string xAColumn, string xBColumn);
}

```

## Services
Using Base implementations for generic DataServices and Base implementations of RepoServices for mssql and postgres the implementation for each new table/object is minimal and standerdized.

#### BaseDataService\<T> : IBasicDataService\<T>
```
public class BaseDataService<T> : IBasicDataService<T>
{
    public IDbBasicRepo<T> Repo { get; }
    public BaseDataService(IDbBasicRepo<T> repo)
    {
        Repo = repo;
    }
}
```

#### BaseExtendedDataService<T, TExt> : BaseDataService<T>, IExtendedDataService<T, TExt>
```
public class BaseExtendedDataService<T, TExt> : BaseDataService<T>, IExtendedDataService<T, TExt>
{
    public IDbExtendedRepo<T, TExt> ExtendedRepo { get; }
    public BaseExtendedDataService(IDbExtendedRepo<T, TExt> repo) : base(repo)
    {
        ExtendedRepo = repo;
    }
}
```

#### BaseCrossDataService<TA, T, TB> : BaseDataService<T>, ICrossDataService<TA, T, TB>
```
public class BaseCrossDataService<TA, T, TB> : BaseDataService<T>, ICrossDataService<TA, T, TB>
{
    public IDbCrossRepo<TA, T, TB> CrossRepo { get; }
    public BaseCrossDataService(IDbCrossRepo<TA, T, TB> repo) : base(repo)
    {
        CrossRepo = repo;
    }
}
```

### Postgres

```
PostgresBasicRepo<T> : IDbBasicRepo<T> 
{
    ...
}
```
```
PostgresExtendedRepo<T, TExtended> : PostgresBasicRepo<T>, IDbExtendedRepo<T, TExtended>
{
    ...
}
```
```
PostgresCrossRepo<TA, T, TB> : PostgresBasicRepo<T>, IDbCrossRepo<TA, T, TB>
{
    ...
}
```

### Mssql

```
SqlBasicRepo<T> : IDbBasicRepo<T>
{
    ...
}
```
```
SqlExtendedRepo<T, TExtended> : SqlBasicRepo<T>, IDbExtendedRepo<T, TExtended>
{
    ...
}
```
```
SqlCrossRepo<TA, T, TB> : SqlBasicRepo<T>, IDbCrossRepo<TA, T, TB> 
{
    ...
}
```

## Example
Here is an example of how to implement Tags in this system.

### Model

```
public class Tag
{
    public Guid Id { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; }
}
public class ExtTag : Tag
{
    public TagGroup? Group { get; set; }
    public Tag? Parent { get; set; }
}
```

### Migration
```
public class DatabaseMigration : IDatabaseMigration
{
    ...
    await _factory.CreateTable<Tag>(withHistory: UseHistory, withTranslation: UseTranslation);
    await _factory.CreateColumn<Tag>("Name", DataTypes.String(50));
    await _factory.CreateColumn<Tag>("GroupId", DataTypes.Guid, nullable: true);
    await _factory.CreateColumn<Tag>("ParentId", DataTypes.Guid, nullable: true);
    await _factory.CreateUniqueConstraint<Tag>(["Name"]);
    await _factory.CreateForeignKeyConstraint<Tag, TagGroup>("GroupId");
    await _factory.CreateForeignKeyConstraint<Tag, Tag>("ParentId");
    ...
}
```

### Ingestion
```
public class TagJsonIngestService : BaseJsonIngestService<Tag, ITagService>, IIngestService<Tag, ITagService>
{
    public TagJsonIngestService(ITagService service, IOptions<JsonIngestConfig> config) : base(service, config) { }
}
```

### Converter
```
public partial class DbConverter : IDbConverter
{
    private List<ExtTag> ConvertTagFromDataReader(IDataReader reader)
    {
        var result = new List<ExtTag>();
        while (reader.Read())
        {
            result.Add(new ExtTag()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                GroupId = (Guid)reader["groupid"],
                ParentId = (Guid?)reader["parentid"],
                Group = ConvertSingleTagGroup(reader, "group_"),
                Parent = ConvertSingleTag(reader, "parent_")
            });
        }

        return result;
    }
}
```

### Contract
```
public interface ITagService : IExtendedDataService<Tag, ExtTag> { }
```

### Service
```csharp
public class TagDataService : BaseExtendedDataService<Tag, ExtTag>, ITagService
{
    public TagDataService(IDbExtendedRepo<Tag, ExtTag> repo) : base(repo)
    {
        ExtendedRepo.Join<TagGroup>("Group", optional: true);
        ExtendedRepo.Join<Tag>("Parent", optional: true);
    }
}
```

### DI
```
Program.cs
...
/* Migrate database schema */
builder.Services.Configure<DbMigrationConfig>(builder.Configuration.GetSection("DbMigration"));
builder.Services.AddSingleton<IDbMigrationFactory, PostgresMigrationFactory>();
builder.Services.AddSingleton<IDatabaseMigration, DatabaseMigration>();

/* Ingest data */
builder.Services.Configure<JsonIngestConfig>(builder.Configuration.GetSection("JsonIngest"));
builder.Services.AddSingleton<IDatabaseIngest, JsonIngestFactory>();

/*Converters*/
builder.Services.AddSingleton<IDbConverter, DbConverter>();
...
builder.Services.AddSingleton<IDbExtendedRepo<Tag, ExtTag>, PostgresExtendedRepo<Tag, ExtTag>>();
builder.Services.AddSingleton<ITagService, TagDataService>();
...
var host = builder.Build();
...
/* Migrate database schema */
var dbMigration = host.Services.GetRequiredService<IDatabaseMigration>();
await dbMigration.Init();

/* Ingest data */
var dbIngest = host.Services.GetRequiredService<IDatabaseIngest>();
await dbIngest.IngestAll();
```