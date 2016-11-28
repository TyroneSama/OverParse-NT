using OverParse_NT.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverParse_NT.Client
{
    public class EncounterDisplayInfo
    {
        public class Entity
        {
            public string Name;
            public long TotalDamage;
            public EncounterAbility StrongestAttack;
        }

        public IReadOnlyCollection<Entity> Entities;
    }
}
