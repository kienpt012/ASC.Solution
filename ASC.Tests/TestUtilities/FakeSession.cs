using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASC.Tests.TestUtilities
{
    public class FakeSession : ISession
    {
        private Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask; 
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask; 
        }

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value)
        {
            if (!_sessionStorage.ContainsKey(key))
                _sessionStorage.Add(key, value);
            else
                _sessionStorage[key] = value;
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_sessionStorage.ContainsKey(key) && _sessionStorage[key] != null)
            {
                value = _sessionStorage[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

    }
}
