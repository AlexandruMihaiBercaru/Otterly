using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Proj.Data;

public static partial class Extensions
{
    public static WebApplicationBuilder ConfigureDataLayer(
        this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<DataConfiguration>()
            .BindConfiguration(DataConfiguration.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services
            .AddSingleton<IValidateOptions<DataConfiguration>, ValidateDataConfiguration>();

        builder.Services.AddDbContextPool<ApplicationDbContext>(
            (services, options) =>
            {
                var connectionString = services
                    .GetRequiredService<IOptionsMonitor<DataConfiguration>>()
                    .CurrentValue.ConnectionString;

                options
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        return builder;
    }
}

public sealed class DataConfiguration
{
    public const string Section = "Data";

    [Required] public required string Server { get; init; }
    [Required] public required uint Port { get; init; }
    [Required] public required string User { get; init; }
    [Required] public required string Password { get; init; }
    [Required] public required string Database { get; init; }

    public string ConnectionString =>
        $"server={Server};port={Port};user id={User};password={Password};database={Database};";
}

[OptionsValidator]
public partial class ValidateDataConfiguration : IValidateOptions<DataConfiguration>;
