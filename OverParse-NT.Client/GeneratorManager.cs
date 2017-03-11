using OverParse_NT.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverParse_NT.Client
{
    class GeneratorManager
    {
        private PartialEncounter _CurrentEncounter;
        private Dictionary<long, EncounterDisplayInfo.Entity> _CurrentEntities;

        public event EventHandler<EncounterDisplayInfo> EncounterInfoChanged;

        public GeneratorManager(IGenerator generator)
        {
            _CurrentEntities = new Dictionary<long, EncounterDisplayInfo.Entity>();

            generator.EncounterDataChanged += (sender, args) =>
            {
                var newStepCount = args.Current.Steps.Count - (_CurrentEncounter?.Steps?.Count ?? 0);
                var newSteps = args.Current.Steps.Skip(args.Current.Steps.Count - newStepCount);

                foreach (var s in newSteps)
                    _HandleStep(s);

                _CurrentEncounter = args.Current;

                // TODO: extra encounter display info data will require caching
                EncounterInfoChanged?.Invoke(this, new EncounterDisplayInfo { Entities = _CurrentEntities.Values });
            };
        }

        private void _HandleStep(EncounterStep step)
        {
            // if the entity is new add to dictionary
            if (!_CurrentEntities.ContainsKey(step.Source.Account))
                _CurrentEntities[step.Source.Account] = new EncounterDisplayInfo.Entity { Name = step.Source.Name };

            // TODO: handle healing steps
            if (step.Ability.AbilityType == EncounterAbilityType.Damage)
            {
                _CurrentEntities[step.Source.Account].TotalDamage += step.Ability.Value;
                if (step.Ability.Value > _CurrentEntities[step.Source.Account].StrongestAttack.Value)
                    _CurrentEntities[step.Source.Account].StrongestAttack = step.Ability;
            }
        }
    }
}
