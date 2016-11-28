using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverParse_NT.Framework
{
    public class GeneratorEncounterDataChangedEventArgs : EventArgs
    {
        public PartialEncounter Current;
        //public IReadOnlyCollection<Encounter> Completed;
    }

    public interface IGenerator
    {
        event EventHandler<GeneratorEncounterDataChangedEventArgs> EncounterDataChanged;
    }
}
