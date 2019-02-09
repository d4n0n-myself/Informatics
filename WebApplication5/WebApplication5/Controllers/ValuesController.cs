using Microsoft.AspNetCore.Mvc;

namespace WebApplication5.Controllers
{
	/// <summary>
	/// MVC используется как реализация IDisposable
	/// </summary>
	public class ValuesController : Controller 
	{
		public void AddNote(string title, string text)
		{ 
			System.IO.File.WriteAllText($"texts/{title}.txt", text);
		}
	}
}