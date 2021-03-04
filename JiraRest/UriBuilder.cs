using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraRest
{
    public class UriBuilder
    {
        private Uri? _uri;
        private readonly IList<KeyValuePair<string, string>> _parameters = new List<KeyValuePair<string, string>>();

        public UriBuilder SetRoot(string uri)
        {
            _uri = new Uri(uri);
            return this;
        }

        public UriBuilder SetRoot(Uri uri)
        {
            _uri = uri;
            return this;
        }

        public UriBuilder AddRelativePath(string part)
        {
            _uri = new Uri(_uri!, part);
            return this;
        }

        public UriBuilder AddParam<T>(string name, T value, bool escape = false) where T: notnull
        {
            _parameters.Add(
                new KeyValuePair<string, string>(
                    name,
                    escape ? Uri.EscapeDataString(value.ToString()!) : value.ToString()!));

            return this;
        }

        public UriBuilder AddParamIf<T>(bool condition, string name, T value, bool escape = false) where T: notnull
        {
            return condition ? AddParam(name, value, escape) : this;
        }

        public Uri Build()
        {
            return new Uri(
                _uri!,
                _parameters.Any()
                    ? string.Format(
                        "?{0}",
                        string.Join("&", _parameters.Select(p => string.Format("{0}={1}", p.Key, p.Value))))
                    : "");
        }
    }
}