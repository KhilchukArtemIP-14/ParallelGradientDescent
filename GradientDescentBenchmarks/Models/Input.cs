using GradientDescent.LossFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientDescentBenchmarks.Models
{
    public class Input
    {
        public decimal[] InitialParameterValues { get; set; }
        public decimal[][] Data { get; set; }
    }
}
