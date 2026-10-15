using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public static partial class MySettings
{
    private sealed class Store
    {
        private readonly string _path;
        private readonly object _sync = new();
        private readonly Dictionary<string, string> _values;
        private readonly SemaphoreSlim _writeLock = new(1, 1);

        private long _savedVersion;
        private long _version;

        public Store(string path)
        {
            _path = Path.GetFullPath(path);
            _values = File.Read(_path);
        }

        private Dictionary<string, string> CreateSnapshot(out long version)
        {
            lock (_sync)
            {
                version = _version;
                return (_savedVersion == version) ? null : new(_values, StringComparer.Ordinal);
            }
        }

        public void DeleteAll()
        {
            lock (_sync)
            {
                if (_values.Count == 0)
                    return;

                _values.Clear();
                ++_version;
            }
        }

        public void DeleteKey(string key)
        {
            lock (_sync)
            {
                if (_values.Remove(key))
                    ++_version;
            }
        }

        public T Get<T>(string key, T defValue, Func<string, T> parse)
        {
            lock (_sync)
                return _values.TryGetValue(key, out var value) ? parse(value) : defValue;
        }

        public bool HasKey(string key)
        {
            lock (_sync)
                return _values.ContainsKey(key);
        }

        private void MarkSaved(long version)
        {
            lock (_sync)
                _savedVersion = version;
        }

        public void Save()
        {
            _writeLock.Wait();
            try
            {
                var values = CreateSnapshot(out var version);
                if (values == null)
                    return;

                File.Write(_path, values);
                MarkSaved(version);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task SaveAsync()
        {
            await _writeLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var values = CreateSnapshot(out var version);
                if (values == null)
                    return;

                await File.WriteAsync(_path, values).ConfigureAwait(false);
                MarkSaved(version);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public void Set(string key, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            lock (_sync)
            {
                if (_values.TryGetValue(key, out var previousValue) && (previousValue == value))
                    return;

                _values[key] = value;
                ++_version;
            }
        }
    }
}
