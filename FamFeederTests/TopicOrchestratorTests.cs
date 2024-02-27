using Core.Models;
using FamFeederFunction.Functions.FamFeeder;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace FamFeederTests;

[TestClass]
public class TopicOrchestratorTests
{
    [TestMethod]
    public async Task RunOrchestrator_Accepts_MultiPlant_Constant()
    {
        var durableOrchestrationContextMock = Substitute.For<IDurableOrchestrationContext>();
        durableOrchestrationContextMock.GetInput<QueryParameters>().Returns(new QueryParameters{Plants = {"ALL"}, PcsTopics = { "Tag" }});
        durableOrchestrationContextMock.CallActivityAsync<List<string>>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(new List<string> { "PCS$JOHAN_SVERDRUP", "PCS$HEIMDAL"}));

        var result = await TopicOrchestrator.RunOrchestrator(durableOrchestrationContextMock);

        Assert.IsTrue(result.Count > 10);
    }

    [TestMethod]
    public async Task RunOrchestrator_Accepts_Single_Plant()
    {
        var durableOrchestrationContextMock = Substitute.For<IDurableOrchestrationContext>();
        durableOrchestrationContextMock.GetInput<QueryParameters>().Returns(new QueryParameters { Plants = { "PCS$HEIMDAL" }, PcsTopics = { "Tag" } });
        durableOrchestrationContextMock.CallActivityAsync<List<string>>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(new List<string> { "PCS$JOHAN_SVERDRUP", "PCS$HEIMDAL" }));

        var result = await TopicOrchestrator.RunOrchestrator(durableOrchestrationContextMock);

        Assert.AreEqual(1, result.Count);
        Assert.IsFalse(result[0].Contains("plant", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task RunOrchestrator_Accepts_Three_Plants()
    {
        var durableOrchestrationContextMock = Substitute.For<IDurableOrchestrationContext>();
        durableOrchestrationContextMock.GetInput<QueryParameters>()
            .Returns(new QueryParameters { Plants = { "PCS$JOHAN_SVERDRUP", "PCS$HEIMDAL", "PCS$SLEIPNER" }, PcsTopics = { "Tag" } });
        durableOrchestrationContextMock.CallActivityAsync<List<string>>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(new List<string> { "PCS$JOHAN_SVERDRUP", "PCS$HEIMDAL", "PCS$SLEIPNER", "PCS$SNORRE" }));

        var result = await TopicOrchestrator.RunOrchestrator(durableOrchestrationContextMock);

        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public async Task RunOrchestrator_Should_Fail_When_Invalid_Multiplant()
    {
        var durableOrchestrationContextMock = Substitute.For<IDurableOrchestrationContext>();
        durableOrchestrationContextMock.GetInput<QueryParameters>().Returns(new QueryParameters { Plants = { "INVALIDMULTIPLANT" }, PcsTopics = { "Tag" } });
        durableOrchestrationContextMock.CallActivityAsync<List<string>>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(new List<string> { "PCS$JOHAN_SVERDRUP", "PCS$HEIMDAL" }));

        var result = await TopicOrchestrator.RunOrchestrator(durableOrchestrationContextMock);

        Assert.AreEqual("Please provide one or more valid plants", result[0]);
    }

    [TestMethod]
    public async Task RunOrchestrator_Should_Fail_When_Invalid_Plant()
    {
        var durableOrchestrationContextMock = Substitute.For<IDurableOrchestrationContext>();
        durableOrchestrationContextMock.GetInput<QueryParameters>().Returns(new QueryParameters { Plants = { "PCS$INVALIDPLANT" }, PcsTopics = { "Tag" } });
        durableOrchestrationContextMock.CallActivityAsync<List<string>>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(new List<string> { "PCS$JOHAN_SVERDRUP", "PCS$HEIMDAL" }));

        var result = await TopicOrchestrator.RunOrchestrator(durableOrchestrationContextMock);

        Assert.AreEqual("Please provide one or more valid plants", result[0]);
    }
}
