using BenchmarkDotNet.Running;

namespace ConstraintMcpServer.Performance;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        var config = new ConstraintServerBenchmarkConfig();
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}
