using InternTrack.Business.Interfaces;
using InternTrack.DataAccess.Interfaces;

namespace InternTrack.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInternTrackServices(this IServiceCollection services)
    {
        AddScopedImplementations(services, typeof(IScopedRepository));
        AddScopedImplementations(services, typeof(IScopedService));

        return services;
    }

    private static void AddScopedImplementations(IServiceCollection services, Type markerType)
    {
        // Scan only the owning assembly, keeping infrastructure and framework types explicit.
        var assemblyTypes = markerType.Assembly.GetTypes();

        var contracts = assemblyTypes.Where(type =>
            type.IsInterface &&
            type != markerType &&
            !type.ContainsGenericParameters &&
            markerType.IsAssignableFrom(type));

        var concreteTypes = assemblyTypes.Where(type =>
            type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters).ToArray();

        foreach (var contract in contracts)
        {
            var implementations = concreteTypes
                .Where(contract.IsAssignableFrom)
                .ToArray();

            if (implementations.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one implementation of {contract.FullName} in " +
                    $"{markerType.Assembly.GetName().Name}, but found {implementations.Length}.");
            }

            services.AddScoped(contract, implementations[0]);
        }
    }
}
