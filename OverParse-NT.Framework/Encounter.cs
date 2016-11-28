using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverParse_NT.Framework
{
    public enum EncounterStepType
    {
        Damage,
        Heal
    }

    public struct EncounterPlayer
    {
        public long ID;
        public string Name;
    }

    public struct EncounterAbility
    {
        public EncounterStepType Type;
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
