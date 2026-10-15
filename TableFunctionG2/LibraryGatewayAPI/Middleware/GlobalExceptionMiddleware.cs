using LibraryGatewayAPI.DTOs;
using System.Net;
using System.Text.Json;

namespace LibraryGatewayAPI.Middleware
{
	// request coming in -> hit the middleware before it gets to the controller -> once the middleware is done, 
	// it gets passed to the controller
	public class GlobalExceptionMiddleware
	{
		// RequestDelegate -> this is like a baton in a relay race
		private readonly RequestDelegate _next;
		// logger -> we can use to log to the terminal if any error occurs
		private readonly ILogger<GlobalExceptionMiddleware> _logger;
	
		// constructor -> configure our singletons for the logger, and to recieve the baton for RequestDelegate
		public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
		{
			// receieve the baton to take control of the incoming request
			_next = next;
			// setup logger
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// try and pass the incoming request to the function
			try
			{
				await _next(context);
			}
			// catch if the function doesn't respond
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Failed to communicate downstream to the function project");
				await WriteErrorResponse(context, HttpStatusCode.BadGateway, "Can't connect to functions",
				ex.Message);
			}
			// catch any other general errors, to handle and log them gracefully
			catch (Exception ex)
			{
				_logger.LogError(ex, "General error occured");
				await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "Unhandled error occured",
				ex.Message);
			}
		}
		
		private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message, 
		string details)
		{
			// prepare the API to return JSON to the user
			context.Response.ContentType = "application/json";
			// set the desired status code of the response
			context.Response.StatusCode = (int)statusCode;

			// create and prepare the DTO for transmission
			var error = new ErrorResponseDto
			{
				StatusCode = (int)statusCode,
				Message = message,
				Detailed = details
			};

			// turn the DTO into JSON
			var json = JsonSerializer.Serialize(error);
			// send off the response!
			await context.Response.WriteAsync(json);
		}
	}
}
