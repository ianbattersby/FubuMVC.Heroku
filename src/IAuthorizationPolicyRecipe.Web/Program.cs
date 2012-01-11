namespace IAuthorizationPolicyRecipe.Web
{
  using System;
  using System.Diagnostics;
  using System.Web.Routing;
	using System.Net;
  using FubuCore;
  using StructureMap;
  using FubuMVC.Core;
  using FubuMVC.Spark;
	using Kayak;
	using FubuMVC.StructureMap;
	using IAuthorizationPolicyRecipe.Web.Security;

	class Program
	{
		private static IApplicationSource source;
		private static Listener listener;
		private static int port = 80;

		static void Main ()
		{
			source = new HerokuApplication();
			
			if (listener != null) {
				throw new InvalidOperationException ("This FubuKayakApplication is already running");
			}
			
			Listen(null);

			var line = Console.ReadLine ();

			while (line != "quit") {
				line = Console.ReadLine ();
			}

			if (listener != null) {
				listener.SafeDispose ();
			}
		}
		
		private static void Listen(Action<FubuRuntime> activation)
		{
			listener = new Listener (new IPEndPoint(IPAddress.Any, port), new SchedulerDelegate());
			FubuRuntime runtime = rebuildFubuMVCApplication();
	        listener.Start (runtime, () => activation (runtime));

		}

		private static FubuRuntime rebuildFubuMVCApplication ()
		{
			RouteTable.Routes.Clear ();
			return source.BuildApplication ().Bootstrap ();
		}
	}
	
    public class HerokuApplication : IApplicationSource
    {
        public FubuApplication BuildApplication()
        {
			return FubuApplication
			  .For<ConfigureFubuMVC>()
			  .StructureMapObjectFactory(sof =>
                    {
                        sof.AddRegistry<SecurityRegistry>();
                    });
        }
    }
	
	public class SchedulerDelegate : ISchedulerDelegate
    {
        public void OnException(IScheduler scheduler, Exception e)
        {
            // called whenever an exception occurs on Kayak's event loop.
            // this is good place for logging. here's a start:
            Console.WriteLine("Exception on scheduler");
            Console.Out.WriteStackTrace(e);
        }

        public void OnStop(IScheduler scheduler)
        {
            // called when Kayak's run loop is about to exit.
            // this is a good place for doing clean-up or other chores.
            Console.WriteLine("Scheduler is stopping.");
        }
    }
}
