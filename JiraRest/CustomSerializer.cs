using System;

namespace JiraRest
{
    class CustomSerializer : ISerializer
    {
        public T Deserialize<T>(string input)
        {
            throw new NotImplementedException();
            //return new JavaScriptSerializer {MaxJsonLength = int.MaxValue}.Deserialize<T>(input);
        }
    }
}