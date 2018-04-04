// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
using System.Collections.Generic;

namespace StePP.Config
{
    public class Action
    {
        public string Executable { get; set; }
        public List<string> Arguments { get; set; } = new List<string>();
        public Dictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();
        public bool CanBeKilled { get; set; } = true;
        public bool IgnoreFailure { get; set; } = false;
    }
}
