using GradientDescent;
using GradientDescentBenchmarks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentBenchmarks.DataGenerators
{
    public class DataGenerator
    {
        public Input GenerateData(int rows, int predictorsCount, Delegate dependency, int parametersCount)
        {
            var input = new Input();
            Random rand = new Random();

            var res = new decimal[rows][];
            for (int i = 0; i < rows; i++)
            {
                res[i] = new decimal[predictorsCount+1];
                int j;
                for(j =0;j< predictorsCount; j++)
                {
                    res[i][j] = i + j + 1;
                }
                res[i][predictorsCount] = (decimal)dependency.DynamicInvoke(res[i].Take(predictorsCount).Select(d=>(object)d).ToArray());
            }
            input.Data = res;

            return new Input() {
                InitialParameterValues = Enumerable.Range(1, parametersCount).Select(_ => (decimal)rand.NextDouble()).ToArray(),
                Data = res
            };
        }
        public Input GenerateRandomData(int rows, int predictorsCount, int parametersCount)
        {
            var input = new Input();
            Random rand = new Random();

            var res = new decimal[rows][];
            for (int i = 0; i < rows; i++)
            {
                res[i] = new decimal[predictorsCount + 1];
                for (int j = 0; j < predictorsCount+1; j++)
                {
                    res[i][j] = (decimal)(rand.NextDouble()*Math.Pow(10,rand.Next(1,3)));
                }
            }
            input.Data = res;

            return new Input()
            {
                InitialParameterValues = Enumerable.Range(1, parametersCount).Select(_ => (decimal)rand.NextDouble()).ToArray(),
                Data = res
            };
        }
    }
}
