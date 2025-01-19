﻿namespace TopModel.ModelGenerator.Database;

public class DatabaseConfig
{
    public string OutputDirectory { get; set; } = "./";

    public IList<DomainMapping> Domains { get; set; } = [];

    public DatabaseSource Source { get; set; } = new();

    public List<string> Exclude { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    public List<string> ExtractValues { get; set; } = [];

    public Dictionary<string, string> ClassNameOverrides { get; set; } = [];

    public List<ModuleConfig> Modules { get; set; } = [];

    public string ConnectionString => Source.DbType switch
    {
        DbType.POSTGRESQL => PgConnectionString,
        DbType.ORACLE => OracleConnectionString,
        DbType.MYSQL => MySqlConnectionString,
        _ => string.Empty
    };

    private string OracleConnectionString => $@"DATA SOURCE={Source.Host}:{Source.Port}/{Source.DbName};USER ID={Source.User};password={Source.Password}";

    private string PgConnectionString => @$"Host={Source.Host};Port={Source.Port};Database={Source.DbName};Username={Source.User}{(Source.Password != null ? $";Password={Source.Password}" : string.Empty)}";

    private string MySqlConnectionString => $@"Server={Source.Host};Port={Source.Port};User ID={Source.User};Password={Source.Password};Database={Source.DbName}";
}
