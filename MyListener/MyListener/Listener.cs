using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace MyListener
{
    public class Listener
    {
        private HttpListener _httpListener;
        private bool isListening;
        private string _url;

        private Listener(string url)
        {
            _url = url;
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(url);
            _httpListener.Start();
        }

        public static void Main(params string[] args)
        {
            RunNew();
        }

        public static Listener RunNew()
        {
            var listener = new Listener($"http://localhost:1234/");
            listener.Run();
            return listener;
        }

        private async void Run()
        {
            if (isListening) return;
            isListening = true;
            Console.WriteLine($"Now listening on {_url}");

            while (true)
            {
                var context = await _httpListener.GetContextAsync(); 
                var result = await GetDifferentiation(context.Request.QueryString["Expression"]);
                var buffer = Encoding.UTF8.GetBytes(result);
                using (var outputStream = context.Response.OutputStream)
                {
                    outputStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private static async Task<string> GetDifferentiation(string input)
        {
            try
            {
                var diff = ConvertStringToExpression(input);
                return Differentiation.Algebra.Differentiate(diff).ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "Wrong function";
            }
        }
        
        private static Expression<Func<double, double>> ConvertStringToExpression(string str)
        {
            var providerOptions = new Dictionary<string, string>
            {
                { "CompilerVersion", "v3.5" }
            };
            var provider = new CSharpCodeProvider(providerOptions);

            var results = provider.CompileAssemblyFromSource
            (
                new CompilerParameters(new[] { "System.Core.dll" }),
                @"using System;
using System.Linq.Expressions;

class Converter
{
    public static Expression<Func<double, double>> Convert()
    {
        return " + str + @";
    }
}"
            );

            return (Expression<Func<double, double>>)results.CompiledAssembly
                .GetType("Converter")
                .GetMethod("Convert")
                .Invoke(null, null);
        }
    }
}