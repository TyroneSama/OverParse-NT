using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverParse_NT.DamageDump
{
    public struct LogFileEntry
    {
        public struct Entity
        {
            public uint Id;
            public string Name;
        }

        public ulong Timestamp;
        public ushort Instance;
        public Entity Source;
        public Entity Target;
        public uint Action;
        public int Damage;
        public bool IsJustAttack;
        public bool IsCritical;
        public bool IsMultiHit;
        public bool IsMisc;
        public bool IsMisc2;
    }
}
