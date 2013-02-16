using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTP2DemoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            using (var server = new Http2Server(app.Default.Port))
            {
                server.Start();
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(((Exception)e.ExceptionObject).Message);
        }
    }
}
