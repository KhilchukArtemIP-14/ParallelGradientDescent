using BenchmarkDotNet.Attributes;
using GradientDescent;
using GradientDescent.LossFunctions;
using GradientDescentBenchmarks.DataGenerators;
using GradientDescentBenchmarks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentBenchmarks.Benchmarks
{
    [IterationCount(20)]
    [WarmupCount(20)]
    public class TotalBenchmarks
    {
        private Input _data;
        [GlobalSetup]
        public void PrepareData()
        {
            var manager = new DataGenerator();
            Func<decimal,decimal> func = x => 10*x+20;
            _data = manager.GenerateData(2000, 1, func, 2);
        }
        /*[Benchmark]
        public void SequentialGradient()
        {
            Func<decimal, decimal,decimal, decimal> func = (a, b, x) => a * x + b;
            var seq = new SequentialGradientDescentCalculator();
            var res = seq.GetOptimalParameters(_data.InitialParameterValues, func, new SquareErrorFunction(), _data.Data, 10000, 0.000000005m);
        }*/
        [Benchmark]
        public void ParallelGradient()
        {
            Func<decimal, decimal, decimal, decimal> func = (a, b, x) => a * x + b;
            var seq = new ParallelGradientDescentCalculator();
            var res = seq.GetOptimalParameters(_data.InitialParameterValues, func, new SquareErrorFunction(), _data.Data, 10000, 0.000000005m, Environment.ProcessorCount);
        }
    }
}
