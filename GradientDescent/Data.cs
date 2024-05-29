using Microsoft.Data.Analysis;

namespace GradientDescent
{
    public class Data
    {
        private DataFrame _dataFrame;
        private string _targetName;
        public List<string> Columns => _dataFrame.Columns.Select(c=>c.Name).ToList();
        public Data() { }
        public static Data GetSampleData(Func<double,double> func)
        {

            return null;
        }
        public Dictionary<string,double> GetRow(int row)
        {
            var rowDict = new Dictionary<string, double>();
            var columns = _dataFrame.Columns;

            for(int i = 0; i < _dataFrame.Columns.Count; i++)
            {
                rowDict[columns[i].Name] = Convert.ToDouble(_dataFrame.Rows[row][i]);
            }

            return rowDict;
        }
        public static decimal[][] GetSampleData(int rows, Func<decimal,decimal> dependency)
        {
            var res = new decimal[rows][];
            for(int i=0;i<rows; i++)
            {
                res[i] = new decimal[2];
                res[i][0] = i + 1;
                res[i][1] = dependency(i + 1);
            }
            return res;
        }
    }
}
