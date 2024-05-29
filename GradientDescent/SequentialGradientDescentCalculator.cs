using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.CompilerServices;
namespace GradientDescent
{
    public class SequentialGradientDescentCalculator
    {
        public decimal[] GetOptimalParameters(
            decimal[] initialParameterValues,
            Delegate function,
            decimal[][] data,
            int epochs,
            decimal learningRate,
            bool verbal = false)
        {
            decimal[] parameters = new decimal[initialParameterValues.Length];
            initialParameterValues.CopyTo(parameters, 0);

            Delegate[] derivativeDelegates = new Delegate[parameters.Length];

            for(int i = 0; i < parameters.Length; i++)
            {
                derivativeDelegates[i] = GetDerivativeDelegate(function, i);
            }

            for(int t=0;t<epochs;t++)
            {
                var partialDerivativesValues = new decimal[parameters.Length];
                for(int i = 0; i < partialDerivativesValues.Length; i++)
                {
                    partialDerivativesValues[i] = CalculatePartialDerivative(derivativeDelegates[i], data, parameters);
                }
                for (int i = 0; i < partialDerivativesValues.Length; i++)
                {
                    parameters[i] -= partialDerivativesValues[i]*learningRate;
                }

                if (t % 10000 == 0&&verbal)
                {
                    Console.WriteLine($"Epoch is {t}.\n\t Loss func value:{CalculatePartialDerivative(function, data, parameters)}");
                }
            }

            return parameters;
        }

        private static decimal CalculatePartialDerivative(Delegate lossFunction, decimal[][] data, decimal[] parameters)
        {
            decimal values = 0m;
            for(int i = 0; i < data.Length; i++)
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
        public static Delegate GetDerivativeDelegate(Delegate func, int targetIndex)
        {
            if (func.Method.GetParameters().Length<= targetIndex||targetIndex<0) throw new Exception("Index out of dounds");

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
