using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Unity.Interception.InterceptionBehaviors;
using Unity.Interception.PolicyInjection.Pipeline;

namespace Bender.DI
{
    public class SeriloggingBehavior : IInterceptionBehavior
    {
        private readonly ILogger _logger;

        public SeriloggingBehavior(ILogger logger)
        {
            _logger = logger;
        }
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            const string logFormat = "Target: {Target}, Parameters: {@Inputs}";
            var result = getNext()(input, getNext);
            if (result.Exception != null)
            {
                _logger.Error(result.Exception, logFormat, $"{input.Target}.{input.MethodBase.Name}", input.Inputs);
            }
            else
            {
                _logger.Information(logFormat, $"{input.Target}.{input.MethodBase.Name}", input.Inputs);
            }

            return result;
        }

        public IEnumerable<Type> GetRequiredInterfaces() => Enumerable.Empty<Type>();

        public bool WillExecute => true;
    }
}