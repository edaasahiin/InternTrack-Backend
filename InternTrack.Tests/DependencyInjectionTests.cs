using InternTrack.Api.Extensions;
using InternTrack.Business.Interfaces;
using InternTrack.Business.Services;
using InternTrack.DataAccess.Context;
using InternTrack.DataAccess.Interfaces;
using InternTrack.DataAccess.Repositories;
using InternTrack.Infrastructure.Security;
using InternTrack.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InternTrack.Tests;

public class DependencyInjectionTests
{
    private static readonly Dictionary<Type, Type> ExpectedRegistrations = new()
    {
        [typeof(IAuthService)] = typeof(AuthService),
        [typeof(IDashboardService)] = typeof(DashboardService),
        [typeof(IDepartmentService)] = typeof(DepartmentService),
        [typeof(IInternService)] = typeof(InternService),
        [typeof(ITaskService)] = typeof(TaskService),
        [typeof(IDepartmentRepository)] = typeof(DepartmentRepository),
        [typeof(IInternRepository)] = typeof(InternRepository),
        [typeof(IRefreshTokenRepository)] = typeof(RefreshTokenRepository),
        [typeof(ITaskRepository)] = typeof(TaskRepository),
        [typeof(IUserRepository)] = typeof(UserRepository)
    };

    [Fact]
    public void AddInternTrackServices_ShouldRegisterEveryExistingContractAsScoped()
    {
        var services = new ServiceCollection();

        services.AddInternTrackServices();

        foreach (var (contract, implementation) in ExpectedRegistrations)
        {
            var registration = Assert.Single(services, service => service.ServiceType == contract);

            Assert.Equal(implementation, registration.ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
        }
    }

    [Fact]
    public void AddInternTrackServices_ShouldExcludeMarkersAndInfrastructure()
    {
        var services = new ServiceCollection();

        services.AddInternTrackServices();

        Assert.DoesNotContain(services, service => service.ServiceType == typeof(IScopedService));
        Assert.DoesNotContain(services, service => service.ServiceType == typeof(IScopedRepository));
        Assert.DoesNotContain(services, service => service.ServiceType == typeof(ITokenService));
        Assert.DoesNotContain(services, service => service.ServiceType == typeof(IAppLogger));
        Assert.DoesNotContain(services, service => service.ServiceType == typeof(AppDbContext));
        Assert.All(services, service => Assert.True(service.ServiceType.IsInterface));
    }

    [Fact]
    public void AddInternTrackServices_ShouldResolveDependenciesAndRespectScopeBoundaries()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=:memory:"));
        services.AddInternTrackServices();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAppLogger, ConsoleAppLogger>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        foreach (var contract in ExpectedRegistrations.Keys.Append(typeof(ITokenService)).Append(typeof(IAppLogger)))
        {
            var firstInstance = firstScope.ServiceProvider.GetRequiredService(contract);
            var repeatedInstance = firstScope.ServiceProvider.GetRequiredService(contract);
            var secondInstance = secondScope.ServiceProvider.GetRequiredService(contract);

            Assert.Same(firstInstance, repeatedInstance);
            Assert.NotSame(firstInstance, secondInstance);
        }

        Assert.IsType<JwtTokenService>(firstScope.ServiceProvider.GetRequiredService<ITokenService>());
        Assert.IsType<ConsoleAppLogger>(firstScope.ServiceProvider.GetRequiredService<IAppLogger>());
    }
}
