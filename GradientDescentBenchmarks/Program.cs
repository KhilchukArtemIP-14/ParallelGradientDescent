using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using DecimalMath;
using GradientDescent;
using GradientDescentBenchmarks.Benchmarks;
using GradientDescentBenchmarks.DataGenerators;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Support;
using GradientDescentBenchmarks.Models;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Environments;
class Program
{

    static void Main(string[] args)
    {
        var dg = new DataGenerator();
        Func<decimal,decimal> dependency = (x) => 2*x+10;
        var data = dg.GenerateData(500,1,dependency,2);

        Func<decimal, decimal, decimal, decimal, decimal> func = (a, b, x,y) =>(y - (a * x + b))* (y - (a * x + b)) /500;

        var seq = new SequentialGradientDescentCalculator();
        var par = new ParallelGradientDescentCalculator();
        var resSeq = seq.GetOptimalParameters(data.InitialParameterValues, func, data.Data, 1, 0.000_01m);
        Console.WriteLine($"Sequential algorithm has finished it's work! Results:\n\tA:{resSeq[0]}\n\tB:{resSeq[1]}");
        var resPar = par.GetOptimalParameters(data.InitialParameterValues, func, data.Data, 1, 0.000_01m, Environment.ProcessorCount, Environment.ProcessorCount);
        Console.WriteLine($"Parallel algorithm has finished it's work! Results:\n\tA:{resPar[0]}\n\tB:{resPar[1]}");

        var config = DefaultConfig.Instance;
        BenchmarkRunner.Run<EightParamsBenchmarks>(config, args);

        Console.ReadLine();
    }
}
