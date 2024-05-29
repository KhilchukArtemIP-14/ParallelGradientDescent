using BenchmarkDotNet.Attributes;
using GradientDescent;
using GradientDescentBenchmarks.DataGenerators;
using GradientDescentBenchmarks.Models;

namespace GradientDescentBenchmarks.Benchmarks
{
    [IterationCount(20)]
    [WarmupCount(20)]
    public class SequentialBenchmarks
    {
        public IEnumerable<object[]> Data()
        {
            var dg = new DataGenerator();

            for (int dataSize = 1000; dataSize <= 5000; dataSize += 1000)
            {
                Func<decimal, decimal, decimal, decimal> oneCoefficient = (a, x, y) => 0*(y - a * x) * (y - a * x) / (dataSize* dataSize * dataSize);
                Func<decimal, decimal, decimal, decimal, decimal> twoCoefficient = (a, b, x, y) => 0 * (y - (a * x + b)) * (y - (a * x + b)) / (dataSize * dataSize);
                Func<decimal, decimal, decimal, decimal, decimal, decimal> threeCoefficient = (a, b, c, x, y) => 0 * (y - (a * x + b + c / x)) * (y - (a * x + b + c / x)) / (dataSize * dataSize * dataSize);
                Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal> fourCoefficient = (a, b, c, d, x, y) => 0 * (y - (a * x + b + c / x + d / (x * x))) * (y - (a * x + b + c / x + d / (x * x))) / (dataSize * dataSize);
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
                    yield return new object[] { dg.GenerateRandomData(dataSize, 1, coefficientCount), coefficientCount, dataSize, funcs[coefficientCount - 1] };
                }
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void SequentialGradient(Input input,int parameters,int rows, Delegate func)
        {
            var seq = new SequentialGradientDescentCalculator();
            var res = seq.GetOptimalParameters(input.InitialParameterValues, func, input.Data, 100, 0.000_000_000_000_005m);
        }
    }
}
