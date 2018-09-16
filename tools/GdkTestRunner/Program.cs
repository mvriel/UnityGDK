using System;

namespace GdkTestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var options = GdkTestRunnerOptions.ParseArguments(args);
                var runner = new TestRunner(options);
                runner.Run();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }
    }
}
