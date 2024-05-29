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
    [WarmupCount(20)]
    [IterationCount(20)]
    public class EightParamsBenchmarks
    {
        public IEnumerable<object[]> Data()
        {
            var dg = new DataGenerator();

            int dataSize = 10000;

            Func<decimal, decimal, decimal, decimal> oneCoefficient = (a, x, y) => 0 * (y - a * x) * (y - a * x) / (dataSize * dataSize * dataSize);
            Func<decimal, decimal, decimal, decimal, decimal> twoCoefficient = (a, b, x, y) => 0 * (y - (a * x + b)) * (y - (a * x + b)) / (dataSize * dataSize);
            Func<decimal, decimal, decimal, decimal, decimal, decimal> threeCoefficient = (a, b, c, x, y) => 0 * (y - (a * x + b + c / x)) * (y - (a * x + b + c / x)) / (dataSize * dataSize * dataSize);
            Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal> fourCoefficient = (a, b, c, d, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x))) * (y - (a * x + b + c / x + d / (x * x))) / (dataSize * dataSize);
            Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal> fiveCoefficient = (a, b, c, d, f, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) / (dataSize * dataSize * dataSize);
            Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal> sixCoefficient = (a, b, c, d, f,e, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) / (dataSize * dataSize * dataSize);
            Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal> sevenCoefficient = (a, b, c, d, f,e,g, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) / (dataSize * dataSize * dataSize);
            Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal> eightCoefficient = (a, b, c, d, f,e,g,h, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) / (dataSize * dataSize * dataSize);

            Delegate[] funcs = new Delegate[]{
                            oneCoefficient,
                            twoCoefficient,
                            threeCoefficient,
                            fourCoefficient,
                            fiveCoefficient,
                            sixCoefficient,
                            sevenCoefficient,
                            eightCoefficient,
                        };
             for (int coefficientCount = 1; coefficientCount <= 8; coefficientCount++)
             {
                  yield return new object[] { dg.GenerateRandomData(dataSize, 1, coefficientCount), coefficientCount, funcs[coefficientCount - 1] };
             }
            
        }
        public IEnumerable<object[]> FixedBlockCountData()
        {
            var dg = new DataGenerator();

            int dataSize = 10000;

            for (int threads = 3; threads <= 21; threads += 3)
            {
                Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal> eightCoefficient =
                    (a, b, c, d, f, e, g, h, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) *
                                                        (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) /
                                                        (dataSize * dataSize * dataSize);

                yield return new object[] { dg.GenerateRandomData(dataSize, 1, 8), threads, eightCoefficient };
            }
        }
        public IEnumerable<object[]> FixedThreadCountData()
        {
            var dg = new DataGenerator();

            int dataSize = 10000;

            for (int blockCount = 4; blockCount <= 20; blockCount += 4)
            {
                Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal> eightCoefficient =
                    (a, b, c, d, f, e, g, h, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) *
                                                        (y - (a * x + b + c / x + d / (x * x) + f / (x * x * x))) /
                                                        (dataSize * dataSize * dataSize);

                yield return new object[] { dg.GenerateRandomData(dataSize, 1, 8), blockCount, eightCoefficient };
            }
        }


        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void SequentialGradient(Input input, int parameters, Delegate func)
        {
            var seq = new SequentialGradientDescentCalculator();
            var res = seq.GetOptimalParameters(input.InitialParameterValues, func, input.Data, 100, 0.000_000_000_000_005m);
        }

        [Benchmark]
        [ArgumentsSource(nameof(FixedBlockCountData))]
        public void ParallelGradient_FixedBlockCount(Input input, int threadsCount, Delegate func)
        {
            var seq = new ParallelGradientDescentCalculator();
            var res = seq.GetOptimalParameters(input.InitialParameterValues, func, input.Data, 100, 0.000_000_000_000_005m, 4, threadsCount);
        }
        [Benchmark]
        [ArgumentsSource(nameof(FixedThreadCountData))]
        public void ParallelGradient_FixedThreadCount(Input input, int blockCount, Delegate func)
        {
            var seq = new ParallelGradientDescentCalculator();
            var res = seq.GetOptimalParameters(input.InitialParameterValues, func, input.Data, 100, 0.000_000_000_000_005m, blockCount, 18);
        }
        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void ParallelGradient_(Input input, int parameters, Delegate func)
        {
            var seq = new ParallelGradientDescentCalculator();
            var res = seq.GetOptimalParameters(input.InitialParameterValues, func, input.Data, 100, 0.000_000_000_000_005m, 8, 18);
        }

        
    }
}
