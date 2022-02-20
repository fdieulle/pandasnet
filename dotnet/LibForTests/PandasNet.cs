using System;
using System.Collections.Generic;
using System.Linq;

namespace LibForTests
{
    public class PandasNet
    {
        public static Dictionary<string, Array> BasicDataFrame(Dictionary<string, Array> df)
            => df;

        public static Dictionary<string, Array> DateTimeDataFrame(Dictionary<string, Array> df)
        {
            foreach (var values in df.Values.OfType<DateTime[]>())
            {
                if (values.Any(p => p.Kind != DateTimeKind.Utc))
                    throw new Exception($"Utc expected.");       
            }

            return df;
        }
    }
}
