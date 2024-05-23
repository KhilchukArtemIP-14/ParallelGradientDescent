using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescent.LossFunctions
{
    public class SquareErrorFunction : ILossFunctionCompiler
    {
        private decimal _n;
        public SquareErrorFunction(int n=1) {
            _n = n;
        }
        public LambdaExpression CompileLossFunction(Delegate function)
        {
            List<ParameterExpression> parameters = new List<ParameterExpression>();
            
            var delegateParams = function.Method.GetParameters();
            for (int i = 0; i < delegateParams.Length; i++)
            {
                var param = Expression.Parameter(typeof(decimal), $"x{i}");
                parameters.Add(param);
            }
            var y = Expression.Parameter(typeof(decimal), "y");

            var body =
                Expression.Divide(Expression.Multiply(
                Expression.Subtract(
                    y,
                    Expression.Call(Expression.Constant(function.Target), function.Method, parameters.ToArray())
                    ),
                Expression.Subtract(
                    y,
                    Expression.Call(Expression.Constant(function.Target), function.Method, parameters.ToArray())
                )),
                Expression.Constant(_n)
                );
            parameters.Add(y);

            return Expression.Lambda(body, parameters);
        }
    }
}
