using GradientDescent.LossFunctions;
using GradientDescent.TaskSchedulers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescent
{
    public class ParallelGradientDescentCalculator
    {
        public ParallelGradientDescentCalculator(int threads = 12)
        {
            ThreadPool.SetMaxThreads(threads, threads);
            ThreadPool.SetMinThreads(threads, threads);

        }
        public decimal[] GetOptimalParameters(
            decimal[] initialParameterValues,
            Delegate function,
            ILossFunctionCompiler lossFunctionCompiler,
            decimal[][] data,
            int epochs,
            decimal learningRate,
            int blockCount,
            bool verbose = false)
        {
            var lossFunction = lossFunctionCompiler.CompileLossFunction(function);

            Delegate[] derivativeDelegates = new Delegate[initialParameterValues.Length];

            for (int i = 0; i < initialParameterValues.Length; i++)
            {
                derivativeDelegates[i] = GetDerivativeDelegate(lossFunction, i);
            }

            for (int t = 0; t < epochs; t++)
            {
                var partialDerivativesValues = new decimal[initialParameterValues.Length];
                int blockSize = data.Length / blockCount;
                var paramsCountdown = new CountdownEvent(partialDerivativesValues.Length);

                for (int i = 0; i < partialDerivativesValues.Length; i++)
                {
                    int index = i;
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        var blockResults = new decimal[blockCount];
                        var blockSize = data.Length / blockCount;
                        var blocksCountdown = new CountdownEvent(blockCount);

                        for (int block = 0; block < blockCount; block++)
                        {
                            int blockStart = block;
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                int start = blockStart * blockSize;
                                int end = (blockStart == blockCount - 1) ? data.Length : start + blockSize;
                                var dataBlock = data.Skip(start).Take(end - start).ToArray();

                                blockResults[blockStart] = CalculatePartialDerivative(derivativeDelegates[index], dataBlock, initialParameterValues);
                                blocksCountdown.Signal();
                            });
                        }

                        blocksCountdown.Wait();

                        partialDerivativesValues[index] = blockResults.Sum();
                        paramsCountdown.Signal();
                    });
                }

                paramsCountdown.Wait();
                for (int i = 0; i < partialDerivativesValues.Length; i++)
                {
                    initialParameterValues[i] -= partialDerivativesValues[i] * learningRate;
                }

                if (verbose && t % 1000 == 0)
                {
                    Console.WriteLine($"Epoch is {t}.\n\t Loss func value:{GetLossFunctionValue(lossFunction.Compile(), data, initialParameterValues)}");
                }
            }

            return initialParameterValues;
        }

        private static decimal CalculatePartialDerivative(Delegate lossFunction, decimal[][] data, decimal[] parameters)
        {
            decimal values = 0m;
            for (int i = 0; i < data.Length; i++)
            {
                List<object> variables = new List<object>();


                foreach (var param in parameters)
                {
                    variables.Add((object)param);
                }

                foreach (var param in data[i])
                {
                    variables.Add((object)param);
                }

                values += (decimal)lossFunction.DynamicInvoke(variables.ToArray());
            }

            return values;
        }
        private static Delegate GetDerivativeDelegate(LambdaExpression func, int targetIndex)
        {
            if (func.Parameters.Any(p => p.Type != typeof(decimal))) throw new Exception("Not all parameters are decimals");
            if (func.Parameters.Count <= targetIndex || targetIndex < 0) throw new Exception("Index out of dounds");

            List<Expression> adjustedParameters = new List<Expression>();
            List<ParameterExpression> originalParameters = new List<ParameterExpression>();
            ConstantExpression epsilonConstant = Expression.Constant(1e-20m);

            var delegateParams = func.Parameters;
            for (int i = 0; i < delegateParams.Count; i++)
            {
                if (targetIndex == i)
                {

                    adjustedParameters.Add(Expression.Add(delegateParams[i], epsilonConstant));
                }
                else adjustedParameters.Add(delegateParams[i]);
                originalParameters.Add(delegateParams[i]);
            }

            var body = Expression.Divide(
                Expression.Subtract(
                    Expression.Invoke(func, adjustedParameters.ToArray()),
                    Expression.Invoke(func, originalParameters.ToArray())
                ),
                epsilonConstant);

            var tmp = Expression.Lambda(body, originalParameters);

            return tmp.Compile();
        }
        private decimal GetLossFunctionValue(Delegate loss, decimal[][] data, decimal[] parameters)
        {
            decimal values = 0m;
            for (int i = 0; i < data.Length; i++)
            {
                List<object> variables = new List<object>();

                foreach (var param in parameters)
                {
                    variables.Add((object)param);
                }

                foreach (var param in data[i])
                {
                    variables.Add((object)param);
                }

                values += (decimal)loss.DynamicInvoke(variables.ToArray());
            }

            return values;
        }
    }
}
