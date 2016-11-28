using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverParse_NT.DamageDump
{
    /// <summary>
    /// Watches for changes in damage dump file
    /// </summary>
    public class DamageDumpWatcher
    {
        private string _FilePath;
        private FileInfo _PreviousFileInfo;

        public event EventHandler<IReadOnlyCollection<DamageDumpUtilities.Entry>> NewEntries;

        public DamageDumpWatcher(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new ArgumentException("File path does not exist", nameof(filePath));

            _FilePath = filePath;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            using (var stream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                reader.ReadLineAsync().Wait(ct); // skip header

                while (!ct.IsCancellationRequested)
                {
                    var fileInfo = new FileInfo(_FilePath);
                    if (fileInfo.Length != _PreviousFileInfo?.Length)
                    {
                        _PreviousFileInfo = fileInfo;

                        var entries = _ReadNewEntries(reader, ct);
                        if (entries.Any())
                            NewEntries?.Invoke(this, entries);
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
                    }
                }
            }
        }

        // TODO: Possible IEnumerable?
        private IReadOnlyCollection<DamageDumpUtilities.Entry> _ReadNewEntries(TextReader reader, CancellationToken ct)
        {
            var entries = new List<DamageDumpUtilities.Entry>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                ct.ThrowIfCancellationRequested();

                var e = DamageDumpUtilities.ParseDumpLine(line);
                if (e.HasValue)
                    entries.Add(e.Value);
            }

            return entries;
        }
    }
}
