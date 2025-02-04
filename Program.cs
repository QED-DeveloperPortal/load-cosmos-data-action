    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using  Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;

    class Program 
    {
        private static readonly string? endpointUri = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT");
        private static readonly string? primaryKey = Environment.GetEnvironmentVariable("COSMOS_KEY");
     
        private const string authorsPath = "/home/runner/work/qed-developer-portal/qed-developer-portal/_data/authors.yml";
            
        private static CosmosClient? cosmosClient;
        private static Database? database;
        private static Container? container;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Time to load our jekyll site with some Cosmos data!");
            cosmosClient = new CosmosClient(endpointUri, primaryKey);
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync("DevPortal");
            container = await database.CreateContainerIfNotExistsAsync("Author", "/partitionkey");

            await QueryItemsAsync();
        }

        static async Task QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Item> queryResultSetIterator = container.GetItemQueryIterator<Item>(queryDefinition);
        
            var authorsText = "";

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Item> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Item item in currentResultSet)
                {
                    authorsText += $"{item.Id}:\n";
                    authorsText += string.IsNullOrEmpty(item.UserId) ? string.Empty : $"  user-id: {item.UserId}\n";
                    authorsText += string.IsNullOrEmpty(item.GhUser) ? string.Empty : $"  gh-user: {item.GhUser}\n";
                    authorsText += string.IsNullOrEmpty(item.Name) ? string.Empty : $"  name: {item.Name}\n";
                    authorsText += string.IsNullOrEmpty(item.Abbrev) ? string.Empty : $"  abbrev: {item.Abbrev}\n";
                    authorsText += string.IsNullOrEmpty(item.Picture) ? string.Empty : $"  picture: {item.Picture}\n";
                    authorsText += string.IsNullOrEmpty(item.Bio) ? string.Empty : $"  bio: {item.Bio}\n";
                    authorsText += string.IsNullOrEmpty(item.Joined) ? string.Empty : $"  joined: {item.Joined}\n";
                    authorsText += string.IsNullOrEmpty(item.Twitter) ? string.Empty : $"  twitter: '{item.Twitter}'\n";
                    authorsText += AppendList("badges-permission", item.BadgesPermission);
                    authorsText += AppendList("badges-award", item.BadgesAward);
                    authorsText += AppendList("badges-role", item.BadgesRole);
                    authorsText += AppendListOfLists("links", new List<string> {"title", "url", "icon"}, item.Links);
                    authorsText += AppendList("subscribed-tags", item.SubscribedTags);
                }
            }
            Console.WriteLine("AUTHORS GENERATED");
            //Console.WriteLine(authorsText);
            File.Delete(authorsPath);
            using (var sw = new StreamWriter(authorsPath))
            {
                sw.Write(authorsText);
            }
        }

        public static string AppendList(string title, string input)
        {
            var result = "";

            if (!string.IsNullOrEmpty(input))
            {
                result += $"  {title}:\n";
                var items = ParseStringToList(input);
                foreach (var item in items)
                {
                    result += $"  - {item}\n";
                }
            }

            return result;
        }

        static List<string> ParseStringToList(string input)
        {
            // Remove the curly braces and whitespace
            input = input.Trim('{', '}', ' ');

            // Split the string into an array using the comma as a delimiter
            string[] items = input.Split(new[] { ", " }, StringSplitOptions.None);

            // Convert the array into a list and return it
            return new List<string>(items);
        }

        static string AppendListOfLists(string title, List<string> subTitles, string input)
        {
            var result = "";

            if (!string.IsNullOrEmpty(input))
            {
                result += $"  {title}:\n";

                var itemLists = ParseStringToListOfLists(input);

                foreach (var itemList in itemLists)
                {
                    var index = 0;
                    foreach (var item in itemList)
                    {
                        if (index == 0)
                        {
                            result = result += $"  - {subTitles[index]}: {item}\n";
                        }
                        else
                        {
                            result += $"    {subTitles[index]}: {item}\n";
                        }

                        index++;
                    }
                }
            }

            return result;
        }

        public static List<List<string>> ParseStringToListOfLists(string input)
        {
            // Remove the outer curly braces
            input = input.Trim('{', '}');

            // Split the string into individual items based on the closing brace followed by a comma and an opening brace
            string[] items = input.Split(new[] { "},{" }, StringSplitOptions.None);

            // Create a list of lists to hold the result
            List<List<string>> result = new List<List<string>>();

            // Process each item
            foreach (var item in items)
            {
                // Remove any remaining braces and split the item into its components
                string[] components = item.Trim('{', '}').Split(new[] { ", " }, StringSplitOptions.None);

                // Create a list from the components and add it to the result
                result.Add(new List<string>(components));
            }

            return result;
        }
    }

    public class Item
    {
        [JsonProperty("id")]
        public required string Id { get; set; }
        
        [JsonProperty("partitionkey")]
        public string? PartitionKey { get; set; }
        
        [JsonProperty("user-id")]
        public required string UserId { get; set; }

        [JsonProperty("gh-user")]
        public string? GhUser { get; set; }

        [JsonProperty("badges-permission")]
        public string? BadgesPermission { get; set; }

        [JsonProperty("badges-award")]
        public string? BadgesAward { get; set; }

        [JsonProperty("badges-role")]
        public string? BadgesRole { get; set; }

        [JsonProperty("bio")]
        public string? Bio { get; set; }

        [JsonProperty("joined")]
        public string? Joined { get; set; }

        [JsonProperty("twitter")]
        public string? Twitter { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("abbrev")]
        public string? Abbrev { get; set; }

        [JsonProperty("picture")]
        public string? Picture { get; set; }

        [JsonProperty("links")]
        public string? Links { get; set; }

        [JsonProperty("subscribed-tags")]
        public string? SubscribedTags { get; set; }

        [JsonProperty("achievements")]
        public string? Achievements { get; set; }
    }
