using OverParse_NT.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverParse_NT.DamageDump
{
    public class DamageDumpEncounterGenerator : IGenerator
    {
        private DamageDumpWatcher _Watcher;

        private PartialEncounter _CurrentEncounter;
        //private IList<Encounter> _CompletedEncounters;

        public event EventHandler<GeneratorEncounterDataChangedEventArgs> EncounterDataChanged;

        public DamageDumpEncounterGenerator(string filePath)
        {
            _Watcher = new DamageDumpWatcher(filePath);
            _Watcher.NewEntries += (sender, entries) => _HandleNewEntries(entries);
        }

        private void _HandleNewEntries(IReadOnlyCollection<DamageDumpUtilities.Entry> entries)
        {
            // filter out unwanted entries
            var filtered = from e in entries
                           where e.Timestamp != 0 // TODO: user info
                           where e.SourceID > 10_000_000 // id range of players
                           //where e.AttackID != 2106601422 // skip zanverse damage
                           select e;

            if (!filtered.Any())
                return;

            // if there is no current encounter make a new one with the start time being the time of the first new encounter
            if (_CurrentEncounter == null)
                _CurrentEncounter = new PartialEncounter { Start = DateTimeOffset.FromUnixTimeSeconds(filtered.First().Timestamp).UtcDateTime, Steps = new List<EncounterStep>() };

            // all current plus new converted encounter steps
            var encounterSteps = new List<EncounterStep>(_CurrentEncounter.Steps);
            encounterSteps.AddRange(filtered.Select(x => _EntryToEncounterStep(_CurrentEncounter.Start, x)));

            // set current encounter to new updated encounter
            _CurrentEncounter = new PartialEncounter
            {
                Start = _CurrentEncounter.Start,
                Steps = encounterSteps
            };

            EncounterDataChanged?.Invoke(this, new GeneratorEncounterDataChangedEventArgs { Current = _CurrentEncounter });
        }

        private EncounterStep _EntryToEncounterStep(DateTime start, DamageDumpUtilities.Entry entry)
        {
            return new EncounterStep
            {
                Offset = DateTimeOffset.FromUnixTimeSeconds(entry.Timestamp).UtcDateTime - start, // TODO: Unsure if damage dump is UTC or local

                Source = new EncounterPlayer { ID = entry.SourceID, Name = entry.SourceName },
                Ability = new EncounterAbility
                {
                    AbilityType = entry.Damage < 0 ? EncounterAbilityType.Heal : EncounterAbilityType.Damage,
                    Value = Math.Abs(entry.Damage),

                    ID = entry.AttackID,
                    Name = "No name database",
                    IsJustAttack = entry.IsJustAttack,
                    IsCriticalAttack = entry.IsCritical
                }
            };
        }

        public async Task RunAsync(CancellationToken ct)
        {
            await _Watcher.RunAsync(ct);
        }

    }
}
