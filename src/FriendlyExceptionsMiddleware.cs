using System;
using System.Threading.Tasks;
using AspNetCore.FriendlyExceptions.Extensions;
using AspNetCore.FriendlyExceptions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AspNetCore.FriendlyExceptions;

internal class FriendlyExceptionsMiddleware(
    RequestDelegate next,
    IOptions<TranformOptions> options
)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await context.HandleExceptionAsync(options, exception);
        }
    }
}