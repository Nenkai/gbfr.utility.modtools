using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.utility.modtools;

public static class Extensions
{
    /// <summary>
    /// Registers a service with implementation but also as an interface
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TImpl"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSingletonAs<TService, TImpl>(this IServiceCollection services) where TImpl : class, TService
                                                                                                       where TService : class
    {
        services.AddSingleton<TImpl>();
        services.AddSingleton<TService>(sp => sp.GetRequiredService<TImpl>());
        return services;
    }
}
