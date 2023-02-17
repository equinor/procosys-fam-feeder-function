using Core.Interfaces;
using Core.Models.Search;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using Equinor.TI.Common.Messaging;
using Infrastructure.Data;
using Infrastructure.Repositories.SearchQueries;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infrastructure.Repositories;

public class SearchItemRepository : ISearchItemRepository
{
    private readonly AppDbContext _context;

    public SearchItemRepository(AppDbContext context) => _context = context;
    public async Task<List<IndexDocument>> GetMcPackages(string plant) => await ExecuteQuery(McPkgQuery.GetQueryWithProjectNames(plant), "McPkg");
    public async Task<List<IndexDocument>> GetCommPackages(string plant) => await ExecuteQuery(CommPkgQuery.GetQueryWithProjectNames(plant), "CommPkg");
    public async Task<List<IndexDocument>> GetPunchItems(string plant) => await ExecuteQuery(PunchListItemQuery.GetQueryWithProjectNames(plant), "PunchItem");
    public async Task<List<IndexDocument>> GetTags(string plant) => await ExecuteQuery(TagQuery.GetQueryWithProjectNames(plant), "Tag");

    internal async Task<List<IndexDocument>> ExecuteQuery(string query, string topic)
    {
        var dbConnection = _context.Database.GetDbConnection();
        var connectionWasClosed = dbConnection.State != ConnectionState.Open;
        if (connectionWasClosed)
        {
            await _context.Database.OpenConnectionAsync();
        }

        try
        {
            await using var command = dbConnection.CreateCommand();
            command.CommandText = query;
            await using var result = await command.ExecuteReaderAsync();
            var entities = new List<IndexDocument>();

            while (await result.ReadAsync())
            {
                var cleanString = WashString((string)result[0]);
                var messageWithFormattedGuid = FormatProCoSysGuid(cleanString);
                switch (topic)
                {
                    case "CommPkg":
                    {
                        var deserializedCommPkg = JsonSerializer.Deserialize<CommPkgTopic>(messageWithFormattedGuid);
                        if (deserializedCommPkg != null)
                        {
                            entities.Add(CreateCommPkgIndexDocument(deserializedCommPkg));
                        }
                        break;
                    }
                    case "McPkg":
                    {
                        var deserializedMcPkg = JsonSerializer.Deserialize<McPkgTopic>(messageWithFormattedGuid);
                        if (deserializedMcPkg != null)
                        {
                            entities.Add(CreateMcPkgTopicIndexDocument(deserializedMcPkg));
                        }
                        break;
                    }
                    case "PunchItem":
                    {
                        var msgPunchItem = JsonSerializer.Deserialize<PunchListItemTopic>(messageWithFormattedGuid);
                        if (msgPunchItem != null)
                        {
                            entities.Add(CreatePunchItemTopicIndexDocument(msgPunchItem));
                        }
                        break;
                    }
                    case "Tag":
                    {
                        var msgTag = JsonSerializer.Deserialize<TagTopic>(messageWithFormattedGuid);
                        if (msgTag != null)
                        {
                            entities.Add(CreateTagTopicIndexDocument(msgTag));
                        }
                        break;
                    }
                }
            }
            return entities;
        }
        finally
        {
            //If we open it, we have to close it.
            if (connectionWasClosed)
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }

    private static IndexDocument CreateTagTopicIndexDocument(TagTopic msgTag)
    {
        var proCoSysGuidString = msgTag.ProCoSysGuid.Replace("-", "").ToUpper();
        var key = $"tag_{proCoSysGuidString}";
        var doc = new IndexDocument
        {
            Key = key,
            LastUpdated = DateTime.ParseExact(msgTag.LastUpdated, @"yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture),
            Plant = msgTag.Plant,
            PlantName = msgTag.PlantName,
            Project = msgTag.ProjectName,
            ProjectNames = msgTag.ProjectNames ?? new List<string>(),
            ProCoSysGuid = proCoSysGuidString,
            Tag = new Tag
            {
                TagNo = msgTag.TagNo,
                McPkgNo = msgTag.McPkgNo,
                CommPkgNo = msgTag.CommPkgNo,
                Description = msgTag.Description,
                Area = msgTag.AreaCode + " " + msgTag.AreaDescription,
                DisciplineCode = msgTag.DisciplineCode,
                DisciplineDescription = msgTag.DisciplineDescription,
                CallOffNo = msgTag.CallOffNo,
                PurchaseOrderNo = msgTag.PurchaseOrderNo,
                TagFunctionCode = msgTag.TagFunctionCode
            }
        };
        return doc;
    }

    private static IndexDocument CreatePunchItemTopicIndexDocument(PunchListItemTopic msgPunchItem)
    {
        var proCoSysGuidString = msgPunchItem.ProCoSysGuid.Replace("-", "").ToUpper();
            var key = $"punchlistitem_{proCoSysGuidString}";
            var doc = new IndexDocument
            {
                Key = key,
                LastUpdated = DateTime.ParseExact(msgPunchItem.LastUpdated, @"yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture),
                Plant = msgPunchItem.Plant,
                PlantName = msgPunchItem.PlantName,
                Project = msgPunchItem.ProjectName,
                ProjectNames = msgPunchItem.ProjectNames ?? new List<string>(),
                ProCoSysGuid = msgPunchItem.ProCoSysGuid,
                PunchItem = new PunchItem
                {
                    PunchItemNo = msgPunchItem.PunchItemNo,
                    Description = msgPunchItem.Description,
                    TagNo = msgPunchItem.TagNo,
                    Responsible = msgPunchItem.ResponsibleCode + " " + msgPunchItem.ResponsibleDescription,
                    FormType = msgPunchItem.FormType,
                    Category = msgPunchItem.Category
                }
            };
            return doc;
    }

    private static IndexDocument CreateMcPkgTopicIndexDocument(McPkgTopic msgMcPkg)
    {
        var proCoSysGuidString = msgMcPkg.ProCoSysGuid.ToString().Replace("-", "").ToUpper();
            var key = $"commpkg_{proCoSysGuidString}";
            var doc = new IndexDocument
            {
                Key = key,
                LastUpdated = DateTime.ParseExact(msgMcPkg.LastUpdated, @"yyyy-MM-dd HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture),
                Plant = msgMcPkg.Plant,
                PlantName = msgMcPkg.PlantName,
                Project = msgMcPkg.ProjectName,
                ProjectNames = msgMcPkg.ProjectNames ?? new List<string>(),
                ProCoSysGuid = proCoSysGuidString,
                McPkg = new McPkg
                {
                    McPkgNo = msgMcPkg.McPkgNo,
                    CommPkgNo = msgMcPkg.CommPkgNo,
                    Description = msgMcPkg.Description,
                    Remark = msgMcPkg.Remark,
                    Responsible = msgMcPkg.ResponsibleCode + " " + msgMcPkg.ResponsibleDescription,
                    Area = msgMcPkg.AreaCode + " " + msgMcPkg.AreaDescription,
                    Discipline = msgMcPkg.Discipline,
                }
            };
            return doc;
    }

    private static IndexDocument CreateCommPkgIndexDocument(CommPkgTopic msgCommPkg)
    {
        var proCoSysGuidString = msgCommPkg.ProCoSysGuid.ToString().Replace("-", "").ToUpper();
        var key = $"commpkg_{proCoSysGuidString}";
        var doc = new IndexDocument
        {
            Key = key,
            LastUpdated = DateTime.ParseExact(msgCommPkg.LastUpdated, @"yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture),
            Plant = msgCommPkg.Plant,
            PlantName = msgCommPkg.PlantName,
            Project = msgCommPkg.ProjectName,
            ProjectNames = msgCommPkg.ProjectNames ?? new List<string>(),
            ProCoSysGuid = proCoSysGuidString,
            CommPkg = new CommPkg
            {
                CommPkgNo = msgCommPkg.CommPkgNo,
                Description = msgCommPkg.Description,
                DescriptionOfWork = msgCommPkg.DescriptionOfWork,
                Remark = msgCommPkg.Remark,
                Responsible = msgCommPkg.ResponsibleCode + " " + msgCommPkg.ResponsibleDescription,
                Area = msgCommPkg.AreaCode + " " + msgCommPkg.AreaDescription
            }
        };
        return doc;
    }

    private static string FormatProCoSysGuid(string cleanString)
    {
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(cleanString);
        if (Guid.TryParse(dictionary?["ProCoSysGuid"].ToString(), out var guid))
        {
            dictionary["ProCoSysGuid"] = guid;
        }
        var messageWithFormattedGuid = JsonSerializer.Serialize(dictionary);
        return messageWithFormattedGuid;
    }


    private static string WashString(string busEventMessage)
    {
        if (string.IsNullOrEmpty(busEventMessage))
        {
            return busEventMessage;
        }
        Regex rx = new(@"[\a\e\f\n\r\t\v]", RegexOptions.Compiled);
        busEventMessage = busEventMessage.Replace("\r", "");
        busEventMessage = busEventMessage.Replace("\n", "");
        busEventMessage = busEventMessage.Replace("\t", "");
        busEventMessage = busEventMessage.Replace("\f", "");
        busEventMessage = rx.Replace(busEventMessage, m => Regex.Escape(m.Value));

        busEventMessage = busEventMessage.Replace((char)31, ' ');
        busEventMessage = busEventMessage.Replace((char)11, ' ');
        busEventMessage = busEventMessage.Replace((char)4, ' ');

        return busEventMessage;
    }
}