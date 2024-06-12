using FamFeederFunction.Functions.FamFeeder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FamFeederTests;

[TestClass]
public class TopicHttpTriggerTests
{
    [TestMethod]
    public void Has_Valid_Topic()
    {
        var topics = "Action";
        Assert.IsTrue(TopicHttpTrigger.HasValidTopic(topics));
    }

    [TestMethod]
    public void No_Valid_Topic()
    {
        var topics = "NO_TOPIC";
        Assert.IsFalse(TopicHttpTrigger.HasValidTopic(topics));
    }

    [TestMethod]
    public void HasValidTopics_Fails_IfOneOfTheTopicsAreInvalid()
    {
        var topics = "NOT_VALID,Action";
        Assert.IsFalse(TopicHttpTrigger.HasValidTopic(topics));
    }

    [TestMethod]
    public void Splits_List()
    {
        var topics = " Action, Task ,Tag , ,";
        var splitList = TopicHttpTrigger.SplitList(topics);

        Assert.IsTrue(splitList.Contains("Action"));
        Assert.IsTrue(splitList.Contains("Task"));
        Assert.IsTrue(splitList.Contains("Tag"));
    }
}