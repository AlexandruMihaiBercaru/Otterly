using Serilog;
using Serilog.Exceptions;

namespace Proj.Configurations;

public static partial class Extensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<LoggingConfiguration>()
            .BindConfiguration(LoggingConfiguration.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = new LoggingConfiguration();
        builder.Configuration.GetSection(LoggingConfiguration.Section).Bind(options);

        builder.Host.UseSerilog((ctx, config) =>
        {
            var env = ctx.HostingEnvironment.EnvironmentName;

            config.ReadFrom.Configuration(
                ctx.Configuration,
                readerOptions: new() { SectionName = LoggingConfiguration.Section });

            config
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironment(env)
                .Enrich.WithExceptionDetails();

            if (options.UseConsole)
            {
                config.WriteTo.Async(writeTo =>
                    writeTo.Console(outputTemplate: options.Template));
            }
        });

        return builder;
    }
}

public sealed class LoggingConfiguration
{
    public const string Section = "Logging";

    public bool UseConsole { get; init; } = true;

    public string Template { get; init; } =
        "{Timestamp:yyyy/MM/dd HH:mm:ss} {Level} - {Message:lj}{NewLine}{Exception}";
}