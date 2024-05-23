using BenchmarkDotNet.Attributes;
using GradientDescent.LossFunctions;
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
    public class SequentialBenchmarks
    {
        public IEnumerable<object[]> Data()
        {
            var dg = new DataGenerator();

            Func<decimal, decimal> oneCoefficientPrep = (x) => 10 * x;
            Func<decimal, decimal> twoCoefficientPrep = (x) => 10 * x + 5;
            Func<decimal, decimal> threeCoefficientPrep = (x) => 10 * x*x + 5*x+100;
            Func<decimal, decimal> fourCoefficientPrep = (x) => 10 * x * x + 5 * x + 100 + 1000/x;

            Func<decimal, decimal, decimal> oneCoefficient = (a,x) => a * x;
            Func<decimal, decimal, decimal, decimal> twoCoefficient = (a,b,x) => a * x + b;
            Func<decimal, decimal, decimal, decimal, decimal> threeCoefficient = (a,b,c,x) => a * x * x + b * x + c;
            Func<decimal, decimal, decimal, decimal, decimal, decimal> fourCoefficient = (a,b,c,d,x) => a * x * x + b * x + c + d / x;

            yield return new object[] { dg.GenerateData(1000, 1, oneCoefficientPrep, 1), 1, 1000, oneCoefficient };
            yield return new object[] { dg.GenerateData(2000, 1, oneCoefficientPrep, 1), 1, 2000, oneCoefficient };
            yield return new object[] { dg.GenerateData(3000, 1, oneCoefficientPrep, 1), 1, 3000, oneCoefficient };
            yield return new object[] { dg.GenerateData(4000, 1, oneCoefficientPrep, 1), 1, 4000, oneCoefficient };
            yield return new object[] { dg.GenerateData(5000, 1, oneCoefficientPrep, 1), 1, 5000, oneCoefficient };
            yield return new object[] { dg.GenerateData(1000, 1, oneCoefficientPrep, 2), 2, 1000, twoCoefficient };
            yield return new object[] { dg.GenerateData(2000, 1, oneCoefficientPrep, 2), 2, 2000, twoCoefficient };
            yield return new object[] { dg.GenerateData(3000, 1, oneCoefficientPrep, 2), 2, 3000, twoCoefficient };
            yield return new object[] { dg.GenerateData(4000, 1, oneCoefficientPrep, 2), 2, 4000, twoCoefficient };
            yield return new object[] { dg.GenerateData(5000, 1, oneCoefficientPrep, 2), 2, 5000, twoCoefficient };
            yield return new object[] { dg.GenerateData(1000, 1, oneCoefficientPrep, 3), 3, 1000, threeCoefficient };
            yield return new object[] { dg.GenerateData(2000, 1, oneCoefficientPrep, 3), 3, 2000, threeCoefficient };
            yield return new object[] { dg.GenerateData(3000, 1, oneCoefficientPrep, 3), 3, 3000, threeCoefficient };
            yield return new object[] { dg.GenerateData(4000, 1, oneCoefficientPrep, 3), 3, 4000, threeCoefficient };
            yield return new object[] { dg.GenerateData(5000, 1, oneCoefficientPrep, 3), 3, 5000, threeCoefficient };
            yield return new object[] { dg.GenerateData(1000, 1, oneCoefficientPrep, 4), 4, 1000, fourCoefficient };
            yield return new object[] { dg.GenerateData(2000, 1, oneCoefficientPrep, 4), 4, 2000, fourCoefficient };
            yield return new object[] { dg.GenerateData(3000, 1, oneCoefficientPrep, 4), 4, 3000, fourCoefficient };
            yield return new object[] { dg.GenerateData(4000, 1, oneCoefficientPrep, 4), 4, 4000, fourCoefficient };
            yield return new object[] { dg.GenerateData(5000, 1, oneCoefficientPrep, 4), 4, 5000, fourCoefficient };
        }

        [Benchmark]
        [ArgumentsSource(nameof(Data))]
        public void SequentialGradient(Input input,int parameters,int rows, Delegate func)
        {
            var seq = new SequentialGradientDescentCalculator();
            var res = seq.GetOptimalParameters(input.InitialParameterValues, func, new SquareErrorFunction(), input.Data, 20_000, 0.000_000_005m);
        }
        /*[Benchmark]
        public void ParallelGradient()
        {
            Func<decimal, decimal, decimal, decimal> func = (a, b, x) => a * x + b;
            var seq = new ParallelGradientDescentCalculator();
            var res = seq.GetOptimalParameters(_data.InitialParameterValues, func, new SquareErrorFunction(), _data.Data, 50000, 0.000000005m);
        }*/
    }
}
