using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverParse_NT.DamageDump
{
    public static class DamageDumpUtilities
    {
        public struct Entry
        {
            public long Timestamp;
            public long InstanceID;
            public long SourceID;
            public string SourceName;
            public long TargetID;
            public string TargetName;
            public long AttackID;
            public long Damage;
            public bool IsJustAttack;
            public bool IsCritical;
            public bool IsMultiHit;
            public bool IsMisc;
            public bool IsMisc2;
        }

        public static Entry? ParseDumpLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return null;

            var parts = line.Split(',');

            // number of segments in csv line
            if (parts.Length != 13)
                return null;

            // try catch for conversion errors
            try
            {
                return new Entry
                {
                    Timestamp = long.Parse(parts[0]),
                    InstanceID = long.Parse(parts[1]),
                    SourceID = long.Parse(parts[2]),
                    SourceName = parts[3],
                    TargetID = long.Parse(parts[4]),
                    TargetName = parts[5],
                    AttackID = long.Parse(parts[6]),
                    Damage = long.Parse(parts[7]),
                    IsJustAttack = int.Parse(parts[8]) > 0,
                    IsCritical = int.Parse(parts[9]) > 0,
                    IsMultiHit = int.Parse(parts[10]) > 0,
                    IsMisc = int.Parse(parts[11]) > 0,
                    IsMisc2 = int.Parse(parts[12]) > 0
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
