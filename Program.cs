   using System;
   using System.Text;
   using System.Threading.Tasks;
   using  Microsoft.Azure.Cosmos;
   using Newtonsoft.Json;

class Program 
{
    private static readonly string endpointUri = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT");
    private static readonly string primaryKey = Environment.GetEnvironmentVariable("COSMOS_KEY");
    private static CosmosClient cosmosClient;
    private static Database database;
    private static Container container;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Time to load our jekyll site with some Cosmos data!");
        cosmosClient = new CosmosClient(endpointUri, primaryKey);
        database = await cosmosClient.CreateDatabaseIfNotExistsAsync("DevPortal");
        container = await database.CreateContainerIfNotExistsAsync("Author", "/partitionkey");

        await QueryItemsAsync();
    }

    static async Task QueryItemsAsync()
    {
        var sqlQueryText = "SELECT * FROM c"; // WHERE c.id = 'your-item-id'";
        QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        FeedIterator<Item> queryResultSetIterator = container.GetItemQueryIterator<Item>(queryDefinition);
    
        var authorsText = "";

        while (queryResultSetIterator.HasMoreResults)
        {
            FeedResponse<Item> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            foreach (Item item in currentResultSet)
            {
                Console.WriteLine($"\tRead {item}");
                authorsText += item.Id + "\n";
                authorsText += $"{item.Id}:\n";
                authorsText += string.IsNullOrEmpty(item.UserId) ? string.Empty : $"  user-id: {item.UserId}\n";
                authorsText += string.IsNullOrEmpty(item.GhUser) ? string.Empty : $"  gh-user: {item.GhUser}\n";
                authorsText += string.IsNullOrEmpty(item.Name) ? string.Empty : $"  name: {item.Name}\n";
                authorsText += string.IsNullOrEmpty(item.Abbrev) ? string.Empty : $"  abbrev: {item.Abbrev}\n";
                authorsText += string.IsNullOrEmpty(item.Picture) ? string.Empty : $"  picture: {item.Picture}\n";
                authorsText += string.IsNullOrEmpty(item.Bio) ? string.Empty : $"  bio: {item.Bio}\n";
                authorsText += string.IsNullOrEmpty(item.Joined) ? string.Empty : $"  joined: {item.Joined}\n";
                authorsText += string.IsNullOrEmpty(item.Twitter) ? string.Empty : $"  twitter: '{item.Twitter}'\n";
            }
        }
        Console.WriteLine("AUTHORS GENERATED");
        Console.WriteLine(authorsText);
        Console.WriteLine($"PWD: {Directory.GetCurrentDirectory()}");
        var files = Directory.GetFiles(Directory.GetCurrentDirectory());
        foreach (var file in files)
            Console.WriteLine(file);
    }
}

public class Item
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("partitionkey")]
    public string PartitionKey { get; set; }
    
    [JsonProperty("user-id")]
    public string UserId { get; set; }

    [JsonProperty("gh-user")]
    public string GhUser { get; set; }

    [JsonProperty("badges-permission")]
    public string BadgesPermission { get; set; }

    [JsonProperty("bio")]
    public string Bio { get; set; }

    [JsonProperty("joined")]
    public string Joined { get; set; }

    [JsonProperty("twitter")]
    public string Twitter { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("abbrev")]
    public string Abbrev { get; set; }

    [JsonProperty("picture")]
    public string Picture { get; set; }

    [JsonProperty("subscribed-tags")]
    public string SubscribedTags { get; set; }

    [JsonProperty("achievements")]
    public string Achievements { get; set; }
}
