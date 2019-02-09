using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

			#region Mappers
			// Note Adder
			app.Map("/Add", builder => builder.Run(async context =>
			{
				var query = context.Request.Query;
				var text = query["text"];
				var title = query["title"];

				using (var controller = new ValuesController())
					controller.AddNote(title, text);

				await LoadPageInResponse(context, "confirmation");
			}));
			
//			 Home Page
			app.Map("/Home", builder => builder.Run(async context =>
			{
				var htmlPage = File.ReadAllText("Views/Home.html");
				htmlPage = htmlPage.Replace("@List", GetFiles());
				using (var streamWriter = new StreamWriter(context.Response.Body))
				{
					await streamWriter.WriteAsync(htmlPage);
				}
			}));
			
			// Note Reader
			app.MapWhen(context => context.Request.Path.StartsWithSegments(new PathString("/texts")), 
				builder => builder.Run(async httpContext =>
				{
					var title = httpContext.Request.Query["name"];
					var text = File.Exists("texts/" + title + ".txt") ? File.ReadAllText("texts/" + title + ".txt") : "";
					await httpContext.Response.WriteAsync(text);
				}) );

			// File Saver
			app.Map("/Files", builder => builder.Run(async context =>
			{	
				await LoadPageInResponse(context, "fileLoad");
			}));

			app.Map("/fileupload", builder => builder.Run(async context =>
			{
					var file = context.Request.Form.Files["file"];
					using (var fileStream = new FileStream($"files/{file.FileName}", FileMode.Create))
						file.CopyTo(fileStream);	
					await LoadPageInResponse(context, "confirmation");
				
			}));
			
			app.Map("/Notes", builder => builder.Run(async context =>
			{
				await LoadPageInResponse(context, "notes");
			}));
			#endregion
			
			app.Run(async context =>
			{
				context.Response.StatusCode = 308;
				context.Response.Headers["Location"] = "/Home";
				await Task.CompletedTask;
			});
		}

		private static Task LoadPageInResponse(HttpContext context, string pageName)
		{
			var page = File.ReadAllText($"Views/{pageName}.html");
			using (var streamWriter = new StreamWriter(context.Response.Body))
			{
				return streamWriter.WriteAsync(page);
			}
		}
		
		private static string GetFiles()
		{
			var directoryInfo = new DirectoryInfo("texts");
			var files = directoryInfo.GetFiles();

			var sb = new StringBuilder();
			sb.Append(@"<ul style=""padding-left: 50px; padding-top: 15px;"">");
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