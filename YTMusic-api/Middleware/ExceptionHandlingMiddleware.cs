using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;

namespace YTMusicApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            (HttpStatusCode statusCode, string title, string detail) = exception switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resource Not Found", exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Forbidden, "Access Denied", exception.Message),
                DbUpdateException => (HttpStatusCode.Conflict, "Database Conflict", "A database conflict occurred, which might be due to a duplicate entry."),
                ArgumentException or ArgumentNullException => (HttpStatusCode.BadRequest, "Invalid Argument", exception.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid Operation", exception.Message),
                AuthenticationException => (HttpStatusCode.Unauthorized, "Unauthorized", exception.Message),
                HttpRequestException => (HttpStatusCode.BadGateway, "External Service Error", "Failed to communicate with an external service."),
                _ => (HttpStatusCode.InternalServerError, "Internal Server Error", _env.IsDevelopment() ? exception.ToString() : "An unexpected error occurred.")
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails { Status = (int)statusCode, Title = title, Detail = detail };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}