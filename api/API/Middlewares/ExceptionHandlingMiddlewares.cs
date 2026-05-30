namespace API.Middlewares;

public class ExceptionHandlingMiddlewares : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            await ExceptionHandling(context, e);
        }
    }
    
    public async Task ExceptionHandling(HttpContext context, Exception e)
    {
        context.Response.StatusCode = e switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsJsonAsync(new {error = e.Message});
    }
}