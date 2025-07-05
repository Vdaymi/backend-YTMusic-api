using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CarsMarket
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch(KeyNotFoundException e)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Not Found",
                    Detail = $"{e.Message}"
                };
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await httpContext.Response.WriteAsJsonAsync(problemDetails); 
            }
            catch(DbUpdateException e)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Not Found",
                    Detail = $"Entity with this Id not found. {e.Message}"
                };
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await httpContext.Response.WriteAsJsonAsync(problemDetails);
            }
            catch(ArgumentNullException e)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Bad Request",
                    Detail = $"{e.Message}"
                };
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                await httpContext.Response.WriteAsJsonAsync(problemDetails);
            }
            catch(Exception e)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = $"{e.Message}"
                };
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                await httpContext.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}