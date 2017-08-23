using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Unix;
using Mono.Unix.Native;
using Nancy.Hosting.Self;
using StudentSystemApiCs;
using StudentSystemApiCs.Util;

namespace SelfHosted
{
    public static class Program
    {
        public static void Main()
        {
            AppConfig.Init();
            using (var host = new NancyHost(new Uri(AppConfig.HostUri), new Bootstrapper(), new HostConfiguration { UrlReservations = new UrlReservations { CreateAutomatically = true } }))
            {
                host.Start();
                Log.Write("Server started at " + AppConfig.HostUri);
                if (Type.GetType("Mono.Runtime") != null)
                {
                    UnixSignal.WaitAny(new[]
                    {
                        new UnixSignal(Signum.SIGINT),
                        new UnixSignal(Signum.SIGTERM),
                        new UnixSignal(Signum.SIGQUIT),
                        new UnixSignal(Signum.SIGHUP)
                    });
                }
                else
                {
                    while (Console.ReadKey().Key != ConsoleKey.Escape) { }
                }
                host.Stop();
            }
        }
    }
}
