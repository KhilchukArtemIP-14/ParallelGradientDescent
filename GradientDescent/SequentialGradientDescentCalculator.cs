using GradientDescent.LossFunctions;
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
            ILossFunctionCompiler lossFunctionCompiler,
            decimal[][] data,
            int epochs,
            decimal learningRate,
            bool verbal = false)
        {
            var lossFunction = lossFunctionCompiler.CompileLossFunction(function);

            Delegate[] derivativeDelegates = new Delegate[initialParameterValues.Length];

            for(int i = 0; i < initialParameterValues.Length; i++)
            {
                derivativeDelegates[i] = GetDerivativeDelegate(lossFunction, i);
            }

            for(int t=0;t<epochs;t++)
            {
                var partialDerivativesValues = new decimal[initialParameterValues.Length];
                for(int i = 0; i < partialDerivativesValues.Length; i++)
                {
                    partialDerivativesValues[i] = CalculatePartialDerivative(derivativeDelegates[i], data, initialParameterValues);
                }
                for (int i = 0; i < partialDerivativesValues.Length; i++)
                {
                    initialParameterValues[i] -= partialDerivativesValues[i]*learningRate;
                }
                if (t % 1000 == 0&&verbal)
                {
                    Console.WriteLine($"Epoch is {t}.\n\t Loss func value:{GetLossFunctionValue(lossFunction.Compile(), data, initialParameterValues)}");
                }
            }

            return initialParameterValues;
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
        public static Delegate GetDerivativeDelegate(LambdaExpression func, int targetIndex)
        {
            if (func.Parameters.Any(p => p.Type != typeof(decimal))) throw new Exception("Not all parameters are decimals");
            if (func.Parameters.Count<= targetIndex||targetIndex<0) throw new Exception("Index out of dounds");

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
