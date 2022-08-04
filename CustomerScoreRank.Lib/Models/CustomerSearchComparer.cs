using System.ComponentModel.DataAnnotations;

namespace CustomerScoreRank.Lib.Models
{
    public class CustomerSearchComparer : IComparer<Customer>
    {
        public int Compare(Customer? x, Customer? y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    if (x.Score > y.Score || (x.Score == y.Score && x.Id < y.Id)) return -1;
                    if (x.Score < y.Score || (x.Score == y.Score && x.Id > y.Id)) return 1;
                    return 0;
                }
            }
        }
    }
}
