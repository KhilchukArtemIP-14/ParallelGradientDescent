using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
namespace GradientDescent
{
    public class ParallelGradientDescentCalculator
    {
        public decimal[] GetOptimalParameters(
            decimal[] initialParameterValues,
            Delegate function,
            decimal[][] data,
            int epochs,
            decimal learningRate,
            int blockCount,
            int threads,
            bool verbose = false)
        {
            int originalMaxThreads = -1;
            int originalPorts = -1;
            ThreadPool.GetMaxThreads(out originalMaxThreads, out originalPorts);

            ThreadPool.SetMaxThreads(threads, threads);

            var parameters = new decimal[initialParameterValues.Length];
            initialParameterValues.CopyTo(parameters, 0);

            Delegate[] derivativeDelegates = new Delegate[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                derivativeDelegates[i] = GetDerivativeDelegate(function, i);
            }

            for (int t = 0; t < epochs; t++)
            {
                var partialDerivativesValues = new decimal[parameters.Length];
                int blockSize = data.Length / blockCount;
                var paramsCountdown = new CountdownEvent(parameters.Length);

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

                                blockResults[blockStart] = CalculatePartialDerivative(derivativeDelegates[index], dataBlock, parameters);
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
                    parameters[i] -= partialDerivativesValues[i] * learningRate;
                }
                if (verbose && t % 10000 == 0)
                {
                    Console.WriteLine($"Epoch is {t}.\n\t Loss func value:{CalculatePartialDerivative(function, data, parameters)}");
                }
            }

            ThreadPool.SetMaxThreads(originalMaxThreads, originalPorts);
            return parameters;
        }

        private static decimal CalculatePartialDerivative(Delegate function, decimal[][] data, decimal[] parameters)
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

                values += (decimal)function.DynamicInvoke(variables.ToArray());
            }

            return values;
        }
        private Delegate GetDerivativeDelegate(Delegate func, int targetIndex)
        {
            if (func.Method.GetParameters().Length <= targetIndex || targetIndex < 0) throw new Exception("Index out of dounds");

            List<Expression> adjustedParameters = new List<Expression>();
            List<ParameterExpression> originalParameters = new List<ParameterExpression>();
            ConstantExpression epsilonConstant = Expression.Constant(1e-20m);

            var delegateParams = func.Method.GetParameters();
            for (int i = 0; i < delegateParams.Length; i++)
            {
                if (delegateParams[i].ParameterType != typeof(decimal)) continue;
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
                    Expression.Call(Expression.Constant(func.Target), func.Method, adjustedParameters.ToArray()),
                    Expression.Call(Expression.Constant(func.Target), func.Method, originalParameters.ToArray())
                ),
                epsilonConstant);

            return Expression.Lambda(body, originalParameters).Compile();
        }
    }
}
