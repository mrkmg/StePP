// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
using System.Collections.Generic;

namespace StePP.Config
{
    public class Step
    {
        public List<string> Prerequisites { get; set; } = new List<string>();
        public List<string> Actions { get; set; } = new List<string>();
        public string LogPath { get; set; } = "-";
        public bool CanBeKilled { get; set; } = true;
        public bool IgnoreFailure { get; set; } = false;
    }
}
