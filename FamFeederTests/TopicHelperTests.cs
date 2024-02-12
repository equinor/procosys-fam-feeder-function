using Core.Misc;
using Equinor.ProCoSys.PcsServiceBus;
using FamFeederFunction.Functions.FamFeeder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FamFeederTests;

[TestClass]
public class TopicHelperTests
{
    [TestMethod]
    public void Contains_Topic()
    {
        var topics = TopicHelper.GetAllTopicsAsEnumerable();
        Assert.IsTrue(topics.Contains(PcsTopicConstants.Action));
    }
}