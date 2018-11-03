using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace MyListener
{
    public class Listener
    {
        private readonly HttpListener _httpListener;
        private bool _isListening;
        private readonly string _url;

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
            Console.WriteLine("Drop the mic");
            Console.ReadKey();
        }

        private static Listener RunNew()
        {
            var listener = new Listener($"http://localhost:1234/");
            listener.Run();
            return listener;
        }

        private void Run()
        {
            if (_isListening) return;
            _isListening = true;
            Console.WriteLine($"Now listening on {_url}");

            while (true)
            {
                var context1 = _httpListener.GetContext(); // work
                //var context = await _httpListener.GetContextAsync(); // doesn't work ? 
                var result = GetDifferentiation(context1.Request.QueryString["Expression"]);
                var buffer = Encoding.UTF8.GetBytes(result);
                using (var outputStream = context1.Response.OutputStream)
                    outputStream.Write(buffer, 0, buffer.Length);
            }
        }

        private static string GetDifferentiation(string input)
        {
            try
            {
                var diff = ConvertStringToExpression(input);
                var result = Differentiation.Algebra.Differentiate(diff).ToString();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "Wrong function" + e.Message;
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

            try
            {
                return (Expression<Func<double,double>>)results.CompiledAssembly
                    .GetType("Converter")
                    .GetMethod("Convert")
                    .Invoke(null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return arg => 1.0;
            }
        }
    }
}