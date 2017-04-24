using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverParse_NT.DamageDump
{
    public class LogFileWatcher
    {
        private FileInfo _LastFileInfo;

        public event EventHandler<LogFileEntry[]> OnNewEntries;

        public Task RunAsync(string filePath) => RunAsync(filePath, CancellationToken.None);
        public async Task RunAsync(string filePath, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                await reader.ReadLineAsync(); // skip header

                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    // TODO: find a way to event based update aside from polling
                    var info = new FileInfo(filePath);
                    if (_LastFileInfo == null || info.Length > _LastFileInfo.Length)
                    {
                        _LastFileInfo = info;

                        var newEntries = new List<LogFileEntry>();

                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            ct.ThrowIfCancellationRequested();

                            var entry = _ParseLogLine(line);
                            if (entry.HasValue)
                                newEntries.Add(entry.Value);
                        }

                        if (newEntries.Count > 0)
                            OnNewEntries?.Invoke(this, newEntries.ToArray());
                    }
                    else
                    {
                        await Task.Delay(100, ct);
                    }
                }
            }
        }

        private LogFileEntry? _ParseLogLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var parts = line.Split(',');
            // number of segments expected in line
            if (parts.Length != 13)
                return null;

            // check for any conversion errors
            try
            {
                return new LogFileEntry
                {
                    Timestamp = ulong.Parse(parts[0]),
                    Instance = ushort.Parse(parts[1]),

                    Source = new LogFileEntry.Entity
                    {
                        Id = uint.Parse(parts[2]),
                        Name = parts[3]
                    },

                    Target = new LogFileEntry.Entity
                    {
                        Id = uint.Parse(parts[4]),
                        Name = parts[5]
                    },

                    Action = uint.Parse(parts[6]),
                    Damage = int.Parse(parts[7]),

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
