using Core.Interfaces;
using Core.Models.Search;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using Infrastructure.Data;
using Infrastructure.Repositories.SearchQueries;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infrastructure.Repositories;

public class SearchItemRepository : ISearchItemRepository
{
    private readonly AppDbContext _context;

    public SearchItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<IndexDocument>> GetMcPackages(string plant) => await ExecuteQuery(McPkgQuery.GetQueryWithProjectNames(plant), "McPkg");
    public async Task<List<IndexDocument>> GetCommPackages(string plant) => await ExecuteQuery(CommPkgQuery.GetQueryWithProjectNames(plant), "CommPkg");
    public async Task<List<IndexDocument>> GetPunchItems(string plant) => await ExecuteQuery(PunchListItemQuery.GetQueryWithProjectNames(plant), "PunchItem");
    public async Task<List<IndexDocument>> GetTags(string plant) => await ExecuteQuery(TagQuery.GetQueryWithProjectNames(plant),"Tag");

    internal async Task<List<IndexDocument>> ExecuteQuery(string query, string topic)
    {
        await using var context = _context;
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        await context.Database.OpenConnectionAsync();
        await using var result = await command.ExecuteReaderAsync();
        var entities = new List<IndexDocument>();

        string key;
        IndexDocument doc;

        while (await result.ReadAsync())
        {
            var cleanString = WashString((string)result[0]);
            switch (topic)
            {
                case "CommPkg":
                    var msgCommPkg = JsonSerializer.Deserialize<CommPkgTopic>(cleanString);
                    if (msgCommPkg != null)
                    {
                        key = GenerateKey($"commpkg:{msgCommPkg.Plant}:{msgCommPkg.ProjectName}:{ msgCommPkg.CommPkgNo}");
                        doc = new IndexDocument
                        {
                            Key = key,
                            LastUpdated = DateTime.ParseExact(msgCommPkg.LastUpdated, @"yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                            Plant = msgCommPkg.Plant,
                            PlantName = msgCommPkg.PlantName,
                            Project = msgCommPkg.ProjectName,
                            ProjectNames = msgCommPkg.ProjectNames ?? new List<string>(),
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
                        entities.Add((doc));
                    }
                    break;
                case "McPkg":
                    var msgMcPkg = JsonSerializer.Deserialize<McPkgTopic>(cleanString);
                    if(msgMcPkg != null)
                    {
                        key = GenerateKey($"mcpkg:{msgMcPkg.Plant}:{msgMcPkg.ProjectName}:{msgMcPkg.CommPkgNo}:{msgMcPkg.McPkgNo}");
                        doc = new IndexDocument
                        {
                            Key = key,
                            LastUpdated = DateTime.ParseExact(msgMcPkg.LastUpdated, @"yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                            Plant = msgMcPkg.Plant,
                            PlantName = msgMcPkg.PlantName,
                            Project = msgMcPkg.ProjectName,
                            ProjectNames = msgMcPkg.ProjectNames ?? new List<string>(),
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
                        entities.Add((doc));
                    }

                    break;
                case "PunchItem":
                    var msgPunchItem = JsonSerializer.Deserialize<PunchListItemTopic>(cleanString);
                    if (msgPunchItem != null)
                    {
                        key = GenerateKey($"punchitem:{msgPunchItem.Plant}:{msgPunchItem.ProjectName}:{msgPunchItem.PunchItemNo}");
                        doc = new IndexDocument
                        {
                            Key = key,
                            LastUpdated = DateTime.ParseExact(msgPunchItem.LastUpdated, @"yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                            Plant = msgPunchItem.Plant,
                            PlantName = msgPunchItem.PlantName,
                            Project = msgPunchItem.ProjectName,
                            ProjectNames = msgPunchItem.ProjectNames ?? new List<string>(),

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
                        entities.Add((doc));
                    }
                    
                    break;
                case "Tag":
                    var msgTag = JsonSerializer.Deserialize<TagTopic>(cleanString);
                    if (msgTag != null)
                    {
                        key = GenerateKey($"tag:{msgTag.Plant}:{msgTag.ProjectName}:{ Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(msgTag.TagNo))}");
                        doc = new IndexDocument
                        {
                            Key = key,
                            LastUpdated = DateTime.ParseExact(msgTag.LastUpdated, @"yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                            Plant = msgTag.Plant,
                            PlantName = msgTag.PlantName,
                            Project = msgTag.ProjectName,
                            ProjectNames = msgTag.ProjectNames ?? new List<string>(),

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
                        entities.Add((doc));
                    }
                    break;

                default:
                    // code block
                    break;
            }
        }
        return entities;
    }

    public static string GenerateKey(string keyString)
    {
        var keyBytes = Encoding.UTF8.GetBytes(keyString);
        return Convert.ToBase64String(keyBytes).Replace("/", "_").Replace("+", "-"); // URL safe base64
    }


    public string WashString(string busEventMessage)
    {
        if (string.IsNullOrEmpty(busEventMessage))
        {
            return busEventMessage;
        }
        Regex _rx = new(@"[\a\e\f\n\r\t\v]", RegexOptions.Compiled);
        busEventMessage = busEventMessage.Replace("\r", "");
        busEventMessage = busEventMessage.Replace("\n", "");
        busEventMessage = busEventMessage.Replace("\t", "");
        busEventMessage = busEventMessage.Replace("\f", "");
        busEventMessage = _rx.Replace(busEventMessage, m => Regex.Escape(m.Value));

        busEventMessage = busEventMessage.Replace((char)31, ' ');
        busEventMessage = busEventMessage.Replace((char)11, ' ');
        busEventMessage = busEventMessage.Replace((char)4, ' ');


        ////Removes non printable characters 
        /// THIS ALSO REMOVES NORWEGIAN CHARACTERS LIKE ø !!!
        //const string Pattern = "[^ -~]+";
        //var regExp = new Regex(Pattern);
        //busEventMessage = regExp.Replace(busEventMessage, "");

        return busEventMessage;
    }
}