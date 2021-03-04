using Bender.Configuration.Action;

namespace Bender.Configuration
{
    public abstract class Rule
    {
        public string? AdditionalPredicateName { get; set; }
        public Notify? HowToNotify { get; set; }
        public Update? HowToUpdate { get; set; }
   }
}