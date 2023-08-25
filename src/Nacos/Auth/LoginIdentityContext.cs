namespace Nacos.Auth
{
    using System.Collections.Generic;
    using System.Linq;

    public class LoginIdentityContext
    {
        private readonly Dictionary<string, string> _map = new();

        public string GetParameter(string key) => _map.TryGetValue(key, out var value) ? value : null;

        public void SetParameter(string key, string value) => _map[key] = value;

        public void SetParameter(Dictionary<string, string> parameters)
        {
            foreach (var item in parameters)
            {
                _map[item.Key] = item.Value;
            }
        }

        public List<string> GetAllKey() => _map.Keys.ToList();
    }
}
