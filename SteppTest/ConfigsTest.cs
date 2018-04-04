using System;
using StePP.Config;
using Xunit;

namespace SteppTest
{
    public class ConfigsTest
    {
        [Fact]
        public void AllParamsSimple()
        {
            var reader = new Reader("../../../Configs/all-params-simple.yaml");
            var config = reader.ParseConfig();

            Assert.Equal( "All Params Simple", config.Name);
            Assert.Collection(config.Steps);
        }
    }
}
