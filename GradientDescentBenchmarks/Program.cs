using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using DecimalMath;
using GradientDescent;
using GradientDescent.LossFunctions;
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
using GradientDescent.TaskSchedulers;
using Lucene.Net.Support;
class Program
{

    static void Main(string[] args)
    {
        var dg = new DataGenerator();
        Func<decimal,decimal> dependency = (x) => 2*x+10;
        var data = dg.GenerateRandomData(500,1,2);
        Func<decimal, decimal, decimal, decimal> func = (a, b, x) => a * x + b;


        decimal[] initialValues = { 5m, 1m,};
        var loss = new SquareErrorFunction(data.Data.Length);
        var keke = new decimal[2];
        data.InitialParameterValues.CopyTo(keke,0);
        var seq = new SequentialGradientDescentCalculator();
        var par = new ParallelGradientDescentCalculator(Environment.ProcessorCount);
        var resSeq = seq.GetOptimalParameters(data.InitialParameterValues, func, loss, data.Data, 800_000, 0.000_01m, false);
        Console.WriteLine($"Sequential algorithm has finished it's work! Results:\n\tA:{resSeq[0]}\n\tB:{resSeq[1]}");
        var resPar = par.GetOptimalParameters(keke, func, loss, data.Data, 800_000, 0.000_01m, Environment.ProcessorCount,false);
        Console.WriteLine($"Parallel algorithm has finished it's work! Results:\n\tA:{resPar[0]}\n\tB:{resPar[1]}");

        //var config = DefaultConfig.Instance;
        //BenchmarkRunner.Run<TotalBenchmarks>(config, args);
    }
    private static Delegate GetDerivativeDelegate(Delegate del, int targetIndex)
    {
        //to-do: add parameters validation

        List<Expression> adjustedParameters = new List<Expression>();
        List<ParameterExpression> originalParameters = new List<ParameterExpression>();
        ConstantExpression epsilonConstant = Expression.Constant(1e-20m);

        var delegateParams = del.Method.GetParameters();
        for(int i = 0; i < delegateParams.Length; i++)
        {

            var param = Expression.Parameter(typeof(decimal), $"x{i}");
            if (targetIndex == i)
            {
                adjustedParameters.Add(Expression.Add(param, epsilonConstant));
            }
            else adjustedParameters.Add(param);
            originalParameters.Add(param);
        }

        BinaryExpression body = Expression.Divide(
            Expression.Subtract(
                Expression.Call(Expression.Constant(del.Target), del.Method, adjustedParameters.ToArray()),
                Expression.Call(Expression.Constant(del.Target), del.Method, originalParameters.ToArray())
            ),
            epsilonConstant);

        return Expression.Lambda(body,originalParameters).Compile();
    }
}
