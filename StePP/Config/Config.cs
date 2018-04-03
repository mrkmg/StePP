// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StePP.Config
{
    public class Config
    {
        [Required]
        public string Name { get; set; }
        public int Version { get; set; }
        public Dictionary<string, Step> Steps { get; set; }
        public Dictionary<string, Action> Actions { get; set; }
    }

    public class Step
    {
        public List<string> Prerequisites { get; set; }
        public List<string> Actions { get; set; }
        public string LogPath { get; set; }
    }

    public class Action
    {
        public Dictionary<string, string> Environment { get; set; }
        public string Executable { get; set; }
        public List<string> Arguments { get; set; }
    }
}
