using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace AngularLearning
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
				.AddBasic(options =>
				{
					options.Realm = "idunno";
					options.Events = new BasicAuthenticationEvents
					{
						OnValidateCredentials = Auth
					};
				});

			services.AddAuthorization(options =>
			{
				options.AddPolicy("AlwaysFail", policy => policy.Requirements.Add(new AlwaysFailRequirement()));
			});
			services.AddMvc(config =>
			{
				var policy = new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.Build();
				config.Filters.Add(new AuthorizeFilter(policy));
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseSpaStaticFiles();

			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller}/{action=Index}/{id?}");
			});

			app.UseSpa(spa =>
			{
				// To learn more about options for serving an Angular SPA from ASP.NET Core,
				// see https://go.microsoft.com/fwlink/?linkid=864501

				spa.Options.SourcePath = "ClientApp";

				if (env.IsDevelopment())
				{
					spa.UseAngularCliServer(npmScript: "start");
				}
			});			
		}
		
		private Task Auth(ValidateCredentialsContext context)
		{
			if (context.Username == context.Password)
			{
				var claims = new[]
				{
					new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String,
						context.Options.ClaimsIssuer),
					new Claim(ClaimTypes.Name, context.Username, ClaimValueTypes.String,
						context.Options.ClaimsIssuer)
				};

				context.Principal =
					new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
				context.Success();
			}
			else
			{
				var claims = new[] { new Claim(ClaimTypes.Authentication, "ErrorClaim", ClaimValueTypes.String, context.Options.ClaimsIssuer) };
				context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
				context.Response.StatusCode = 401;
				context.Fail("Error");
			}

			return Task.CompletedTask;
		}
	}
}