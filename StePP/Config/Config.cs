// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using System.Collections.Generic;

namespace StePP.Config
{
    public class Config
    {
        public string Name { get; set; }
        public int Version { get; set; }
        public string LogPath { get; set; } = "-";
        public Dictionary<string, Step> Steps { get; set; } = new Dictionary<string, Step>();
        public Dictionary<string, Action> Actions { get; set; } = new Dictionary<string, Action>();
    }

    public class Step
    {
        public List<string> Prerequisites { get; set; } = new List<string>();
        public List<string> Actions { get; set; } = new List<string>();
        public string LogPath { get; set; } = "-";
    }

    public class Action
    {
        public string Executable { get; set; }
        public List<string> Arguments { get; set; } = new List<string>();
        public Dictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();
        public bool CanBeKilled { get; set; } = true;
    }
}
