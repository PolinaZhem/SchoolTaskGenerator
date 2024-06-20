using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class VariableSetter
    {
        public string Name { get; set; }
        public double RangeFrom { get; set; }
        public double RangeTo { get; set; }
        public double Step { get; set; }
        public int DigitsToRound { get; set; }
        public double Value { get; set; } = 0;

        public override string ToString()
        {
            return $"{Name} {RangeFrom} {RangeTo} {Step} {DigitsToRound}";
        }
    }
}
