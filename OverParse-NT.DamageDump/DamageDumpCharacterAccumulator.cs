using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverParse_NT.DamageDump
{
    public class DamageDumpCharacterAccumulator
    {
        public struct Info
        {
            public string Name;
            public long TotalDamage;
            public long MaxHitDamage;
            public uint MaxHitAction;
        }

        private Dictionary<uint, Info> _Infos = new Dictionary<uint, Info>();

        public event EventHandler<IReadOnlyCollection<Info>> InfosChanged;

        public void Reset()
        {
            _Infos.Clear();
            InfosChanged?.Invoke(this, _Infos.Values.ToArray());
        }

        // TODO: everything that isnt here
        public void ProcessEntries(params LogFileEntry[] entries)
        {
            bool changed = false;

            foreach (var e in entries)
            {
                // this is used to get the current player id
                if (e.Timestamp == 0)
                    continue;

                // only care about players
                if (e.Source.Id < 10000000)
                    continue;

                // discard zanverse for now
                if (e.Action == 2106601422)
                    continue;

                if (Math.Sign(e.Damage) > 0)
                {
                    if (_Infos.ContainsKey(e.Source.Id))
                    {
                        var current = _Infos[e.Source.Id];
                        var largerMaxHit = e.Damage > current.MaxHitDamage;

                        _Infos[e.Source.Id] = new Info
                        {
                            Name = e.Source.Name,
                            TotalDamage = current.TotalDamage + e.Damage,
                            MaxHitDamage = largerMaxHit ? e.Damage : current.MaxHitDamage,
                            MaxHitAction = largerMaxHit ? e.Action : current.MaxHitAction
                        };

                    }
                    else
                    {
                        _Infos[e.Source.Id] = new Info
                        {
                            Name = e.Source.Name,
                            TotalDamage = e.Damage,
                            MaxHitDamage = e.Damage,
                            MaxHitAction = e.Action
                        };
                    }

                    changed = true;
                }
            }

            if (changed)
                InfosChanged?.Invoke(this, _Infos.Values.ToArray());
        }
    }
}
