using BenchmarkDotNet.Attributes;
using GradientDescent;
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
    
    public class ParallelBenchmarks
    {
        public IEnumerable<object[]> BestThreadsAndBlockCountsData()
        {
            var dg = new DataGenerator();

            //threads 3 6 9 12 15
            //blocks 4 8 12 16
            int dataSize = 5000;
            for (int threads = 3; threads <= 15; threads += 3)
            {
                for (int blockCount = 4; blockCount <= 16; blockCount += 4)
                {
                    {
                        Func<decimal, decimal, decimal, decimal> oneCoefficient = (a, x, y) => 0 * (y - a * x) * (y - a * x) / (dataSize * dataSize * dataSize);
                        Func<decimal, decimal, decimal, decimal, decimal> twoCoefficient = (a, b, x, y) => 0 * (y - (a * x + b)) * (y - (a * x + b)) / (dataSize * dataSize * dataSize);
                        Func<decimal, decimal, decimal, decimal, decimal, decimal> threeCoefficient = (a, b, c, x, y) => 0 * (y - (a * x + b + c / x)) * (y - (a * x + b + c / x)) / (dataSize * dataSize * dataSize);
                        Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal> fourCoefficient = (a, b, c, d, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x))) * (y - (a * x + b + c / x + d / (x * x))) / (dataSize * dataSize * dataSize);
                        Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal> fiveCoefficient = (a, b, c, d, f, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) / (dataSize * dataSize * dataSize);

                        Delegate[] funcs = new Delegate[]{
                            oneCoefficient,
                            twoCoefficient,
                            threeCoefficient,
                            fourCoefficient,
                            fiveCoefficient,
                        };
                        
                        for (int coefficientCount = 1; coefficientCount <= 5; coefficientCount++)
                        {
                            yield return new object[] { dg.GenerateRandomData(dataSize, 1, coefficientCount), threads, blockCount, coefficientCount, funcs[coefficientCount - 1] };
                        }

                    }
                }
            }
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(BestThreadsAndBlockCountsData))]
        public void ParallelGradient_BestThreadsAndBlocks(Input input, int threads, int blockCount, int parameters, Delegate func)
        {
            var seq = new ParallelGradientDescentCalculator();
            var res = seq.GetOptimalParameters(input.InitialParameterValues, func, input.Data, 100, 0.000_000_000_000_005m, threads,blockCount);
        }
    }
}
