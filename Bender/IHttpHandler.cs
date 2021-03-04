using System.Collections.Generic;
using JiraRest;

namespace Bender
{
    public interface IHttpHandler
    {
        void HandleAll(IEnumerable<HttpRequest> requests);
    }
}