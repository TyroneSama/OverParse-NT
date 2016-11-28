using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverParse_NT.Framework
{
    [Obsolete]
    public class DamageDumpManager
    {
        private class DamageDumpEntry
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

        public class DataChangedEventArgs : EventArgs
        {
            public class EntityData
            {
                public long ID;
                public string Name;
                public long TotalDamage;
                public long ZanverseDamage;
                public string MaxHitName;
                public long MaxHitDamage;
            }

            public ICollection<EntityData> Entities;
            public long PlayerID;
            public long TotalDamage;
        }

        public event EventHandler<DataChangedEventArgs> DataChanged;

        private string _FilePath;
        private long _PreviousFileLength;
        private long _PlayerID;
        private Dictionary<long, DataChangedEventArgs.EntityData> _CurrentEntities;

        public DamageDumpManager(string filePath)
        {
            _FilePath = filePath;
            _CurrentEntities = new Dictionary<long, DataChangedEventArgs.EntityData>();
        }

        public async Task RunAsync(CancellationToken ct)
        {
            using (var stream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                reader.ReadLine(); // skip header

                while (!ct.IsCancellationRequested)
                {
                    var fileInfo = new FileInfo(_FilePath);
                    if (fileInfo.Length != _PreviousFileLength)
                    {
                        _PreviousFileLength = fileInfo.Length;

                        _ParseNewEntries(ct, reader);
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
                    }
                }
            }
        }

        private void _ParseNewEntries(CancellationToken ct, TextReader reader)
        {
            var newEntities = new Dictionary<long, DataChangedEventArgs.EntityData>(_CurrentEntities);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                ct.ThrowIfCancellationRequested();

                var entry = _ParseDumpFileLine(line);
                if (entry == null)
                    continue; // malformed entry

                if (entry.Timestamp == 0)
                {
                    _PlayerID = entry.SourceID; // nothing could end up zero here or anything
                    continue;
                }

                // only id range of players
                if (entry.SourceID < 10000000)
                    continue;

                if (!newEntities.ContainsKey(entry.SourceID))
                    newEntities[entry.SourceID] = new DataChangedEventArgs.EntityData { ID = entry.SourceID, Name = entry.SourceName };

                newEntities[entry.SourceID].TotalDamage += entry.Damage;

                if (entry.Damage > newEntities[entry.SourceID].MaxHitDamage)
                {
                    newEntities[entry.SourceID].MaxHitDamage = entry.Damage;
                    newEntities[entry.SourceID].MaxHitName = entry.AttackID.ToString(); // TODO: attack names
                }

                // Zanverse
                if (entry.AttackID == 2106601422)
                    newEntities[entry.SourceID].ZanverseDamage += entry.Damage;
            }

            long totalDamage = 0;
            foreach (var e in newEntities.Values)
                totalDamage += e.TotalDamage;

            _CurrentEntities = newEntities;

            DataChanged?.Invoke(this, new DataChangedEventArgs
            {
                Entities = _CurrentEntities.Values,
                PlayerID = _PlayerID,
                TotalDamage = totalDamage
            });
        }

        private DamageDumpEntry _ParseDumpFileLine(string line)
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
                return new DamageDumpEntry
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
