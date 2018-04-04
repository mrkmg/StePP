// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using System.Collections.Generic;

namespace StePP.Config
{
    public class Root
    {
        public string Name { get; set; }
        public int Version { get; set; }
        public string LogPath { get; set; } = "-";
        public Dictionary<string, Step> Steps { get; set; } = new Dictionary<string, Step>();
        public Dictionary<string, Action> Actions { get; set; } = new Dictionary<string, Action>();
    }
}