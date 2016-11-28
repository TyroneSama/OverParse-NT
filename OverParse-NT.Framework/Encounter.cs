using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverParse_NT.Framework
{

    public struct EncounterPlayer
    {
        public long ID;
        public string Name;
    }
    public enum EncounterAbilityType
    {
        Damage,
        Heal
    }

    public struct EncounterAbility
    {
        public EncounterAbilityType AbilityType;
        public long Value;

        public long ID;
        public string Name;
        public bool IsJustAttack;
        public bool IsCriticalAttack;
    }

    public class EncounterStep
    {
        public TimeSpan Offset;

        public EncounterPlayer Source;
        public EncounterAbility Ability;
    }

    public class PartialEncounter
    {
        public DateTime Start;
        public IReadOnlyCollection<EncounterStep> Steps;
    }

    public class Encounter : PartialEncounter
    {
        public TimeSpan Length;
    }
}
