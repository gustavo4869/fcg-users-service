using FluentValidation;

namespace TechChallengeAPI.Endpoints
{
    public sealed class ValidationFilter<T> : IEndpointFilter where T : class
    {
        public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
        {
            var model = ctx.GetArgument<T>(0);
            var validator = ctx.HttpContext.RequestServices.GetService<IValidator<T>>();
            if (validator is not null)
            {
                var result = await validator.ValidateAsync(model, ctx.HttpContext.RequestAborted);
                if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());
            }
            return await next(ctx);
        }
    }

    public static class ValidationFilterExtensions
    {
        public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder b) where T : class
            => b.AddEndpointFilter(new ValidationFilter<T>());
    }
}
