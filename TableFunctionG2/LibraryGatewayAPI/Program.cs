
using Azure.Data.Tables;
using LibraryGatewayAPI.Clients;
using LibraryGatewayAPI.Middleware;
using LibraryGatewayAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using Microsoft.OpenApi;
using System.Reflection.Metadata;
using System.Text;

namespace LibraryGatewayAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();
			// to discover all the routes in the controllers, we need the endpoints explorer
			builder.Services.AddEndpointsApiExplorer();

			// this section is for the Azurite configuration - so that we can store stuff in the users table
			string connectionString = "UseDevelopementStorage=True;";
			builder.Services.AddSingleton(new TableServiceClient(connectionString));
			// prep the authservice to connect to the table client
			builder.Services.AddScoped<AuthService>();

			// this section deals with the JWT authentication
			var jwtSettings = builder.Configuration.GetSection("JwtSettings");
			// get the secret key from the settings
			var secret = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

			// add in the auth service
			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtSettings["Issuer"],
					ValidAudience = jwtSettings["Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(secret)
				};
			});
			builder.Services.AddAuthorization();

			// configure and setup swagger, with additional options to allow for token authentication
			builder.Services.AddSwaggerGen(options => {
			options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo {
				Title = "Library Swagger Docs",
				Version = "v1"
			});
			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				Scheme = "Bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "Enter JWT Token"
			});
			});
			// setup downstream connection to the functions app
			string functionUrl = builder.Configuration.GetValue<string>("DownstreamServices:FunctionsUrl");
			var funcUri = new Uri(functionUrl);

			builder.Services.AddHttpClient<BooksTableClient>(c => c.BaseAddress = funcUri);
			builder.Services.AddHttpClient<DocumentBlobClient>(c => c.BaseAddress = funcUri);
			builder.Services.AddHttpClient<LoanQueueClient>(c => c.BaseAddress = funcUri);

			var app = builder.Build();

			// enable the global error handling middleware, so that we return appopriate error messages
			app.UseMiddleware<GlobalExceptionMiddleware>();


			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
				// enable swagger, and allow us to access its UI webpage
				app.UseSwagger();
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Gateway API");
					c.RoutePrefix = string.Empty;
				});
			}

			app.UseHttpsRedirection();

			// you must authenticate before you can be authorized
			app.UseAuthentication();
			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
