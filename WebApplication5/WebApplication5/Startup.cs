using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApplication5.Controllers;

namespace WebApplication5
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
			//services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
				app.UseHsts();
			}

			//app.UseHttpsRedirection();
			//app.UseMvc();

			app.Map("/Add", builder => builder.Run(async context =>
			{
				using (var streamWriter = new StreamWriter(context.Response.Body))
				{
					var query = context.Request.Query;
					var text = query["text"];
					var title = query["title"];
					
					using (var controller = new ValuesController())
					{
						controller.AddNote(title, text);
					}
					
					await streamWriter.WriteAsync("ti sozdal");
				}
			}));
			
			app.Map("/Home", builder => builder.Run(async context =>
			{
				var htmlPage = File.ReadAllText("Home.html");
				htmlPage = htmlPage.Replace("List", GetFiles());
				using (var streamWriter = new StreamWriter(context.Response.Body))
				{
					await streamWriter.WriteAsync(htmlPage);
				}
			}));

			//app.Map("/texts", builder => builder.Run(async context => { }));
			
			app.MapWhen(context => context.Request.Path.StartsWithSegments(new PathString("/texts")), 
				builder => builder.Run(async httpContext =>
				{
					var title = httpContext.Request.Query["name"];
					var text = File.Exists("texts/" + title + ".txt") ? File.ReadAllText("texts/" + title + ".txt") : "";
					await httpContext.Response.WriteAsync(text);
				}) );

			app.Map("/Files", builder => builder.Run(async context =>
			{
				using (var str = new StreamWriter(context.Response.Body))
				{
					var stream = new StreamWriter("/files");
					var formFile = context.Request.Form.Files["file"].
					await str.WriteAsync("234567890");
				}
			}));
			
			app.Run(async context =>
			{
				context.Response.StatusCode = 308;
				context.Response.Headers["Location"] = "/Home";
			});
		}

		private static string GetFiles()
		{
			var directoryInfo = new DirectoryInfo("texts");
			var files = directoryInfo.GetFiles();

			var sb = new StringBuilder();
			sb.Append("<ul>");
			foreach (var file in files)
			{
				var replace = file.Name.Replace(".txt","");
				sb.Append($@"<li><a href=""texts?name={replace}"">{replace}</a></li>");
			}
			sb.Append("</ul>");
			return sb.ToString();
		}
	}
}