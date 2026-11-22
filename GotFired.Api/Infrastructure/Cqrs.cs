using System.Reflection;

namespace GotFired.Api.Infrastructure
{
    // CQRS without a mediator: every use case is a command (changes state) or a query (reads only), handled by
    // exactly one handler that lives in the same feature file as its endpoint. Endpoints inject the handler directly.

    /// <summary>Changes state. Loads aggregates, calls domain methods, saves.</summary>
    public interface ICommandHandler<in TCommand, TResult>
    {
        Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    }

    /// <summary>Reads only: projects straight from the database to a response, without tracking entities.</summary>
    public interface IQueryHandler<in TQuery, TResult>
    {
        Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
    }

    public static class CqrsRegistration
    {
        /// <summary>Registers every command and query handler in this assembly as a scoped service of its own type.</summary>
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            var handlerInterfaces = new[] { typeof(ICommandHandler<,>), typeof(IQueryHandler<,>) };
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            {
                if (type.GetInterfaces().Any(i => i.IsGenericType && handlerInterfaces.Contains(i.GetGenericTypeDefinition())))
                {
                    services.AddScoped(type);
                }
            }
            return services;
        }
    }
}
