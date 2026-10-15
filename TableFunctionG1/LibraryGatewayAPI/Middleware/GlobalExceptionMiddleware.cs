using LibraryGatewayAPI.DTOs;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryGatewayAPI.Middleware
{
	// request comes in -> before it reaches a controller, it will go through any and
	// all middleware that exists
	public class GlobalExceptionMiddleware
	{
		// RequestDelegate -> this is like the baton in a relay race
		private readonly RequestDelegate _next;
		// logger -> this allows us to take note of any errors that occur
		private readonly ILogger<GlobalExceptionMiddleware> _logger;
	
		public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
		{
			// take IN the baton when it is this piece of middlewares turn to process the request
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// try and pass the request off to the controller
			try
			{
				await _next(context);
			}
			// if there are any errors that occur when trying to communicate from the API to the function
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Failed to communicate downstream to the Functions project");
				await WriteErrorResponse(context, HttpStatusCode.BadGateway, "Can't connect to Function",
				ex.Message);
			}
			// catch any other errors that may not be related to communication
			catch (Exception ex)
			{
				_logger.LogError(ex, "General unhandled exception occured");
				await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "General error occured",
				ex.Message);
			}
		}
		private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode code, 
		string message, string details)
		{
			// prepare a response object that the API will return to the user
			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)code;

			// create the DTO which contains all the information and context about the error
			var error = new ErrorResponseDto
			{
				StatusCode = (int)code,
				Message = message,
				Detailed = details
			};

			// transform the DTO into JSON so it can be sent over HTTP
			var json = JsonSerializer.Serialize(error);
			// and send it
			await context.Response.WriteAsync(json);
		}
	}
}
