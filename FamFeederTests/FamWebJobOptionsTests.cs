using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FamFeederTests;

[TestClass]
public class FamWebJobOptionsTests
{
    [TestMethod]
    public void PlantFilter_Should_Trim_Spaces()
    {
        var options = new FamFeederOptions()
        {
            FamFeederPlantFilter = "  PCS$BLAH ,  PCS$BLAHDEBLAH  "
        };

        Assert.AreEqual("PCS$BLAH", options.FamFeederPlantFilterList[0]);
        Assert.AreEqual("PCS$BLAHDEBLAH", options.FamFeederPlantFilterList[1]);
    }
}