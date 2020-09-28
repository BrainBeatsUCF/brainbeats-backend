using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace brainbeats_backend {
  public class DatabaseConnection {
    public static DatabaseConnection Instance { get; set; }

    private GremlinClient gremlinClient { get; set; }

    public static void Init(IConfiguration configuration) {
      Instance = new DatabaseConnection(configuration);
    }

    private DatabaseConnection(IConfiguration configuration) {
      string EndpointUrl = configuration["Database:EndpointUrl"];
      string PrimaryKey = configuration["Database:PrimaryKey"];
      int port = Int32.Parse(configuration["Database:port"]);
      string database = configuration["Database:database"];
      string container = configuration["Database:container"];

      GremlinServer gremlinServer = new GremlinServer(EndpointUrl, port, enableSsl: true,
                                        username: "/dbs/" + database + "/colls/" + container,
                                        password: PrimaryKey);

      gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer(), "application/json");
    }

    public Task<ResultSet<dynamic>> ExecuteQuery(string query) {
      try {
        return gremlinClient.SubmitAsync<dynamic>(query);
      } catch (ResponseException e) {
        Console.WriteLine("\tRequest Error!");

        // Print the Gremlin status code.
        Console.WriteLine($"\tStatusCode: {e.StatusCode}");

        // On error, ResponseException.StatusAttributes will include the common StatusAttributes for successful requests, as well as
        // additional attributes for retry handling and diagnostics.
        // These include:
        //  x-ms-retry-after-ms         : The number of milliseconds to wait to retry the operation after an initial operation was throttled. This will be populated when
        //                              : attribute 'x-ms-status-code' returns 429.
        //  x-ms-activity-id            : Represents a unique identifier for the operation. Commonly used for troubleshooting purposes.
        PrintStatusAttributes(e.StatusAttributes);
        Console.WriteLine($"\t[\"x-ms-retry-after-ms\"] : { GetValueAsString(e.StatusAttributes, "x-ms-retry-after-ms")}");
        Console.WriteLine($"\t[\"x-ms-activity-id\"] : { GetValueAsString(e.StatusAttributes, "x-ms-activity-id")}");

        throw;
      }
    }

    private void PrintStatusAttributes(IReadOnlyDictionary<string, object> attributes) {
      Console.WriteLine($"\tStatusAttributes:");
      Console.WriteLine($"\t[\"x-ms-status-code\"] : { GetValueAsString(attributes, "x-ms-status-code")}");
      Console.WriteLine($"\t[\"x-ms-total-request-charge\"] : { GetValueAsString(attributes, "x-ms-total-request-charge")}");
    }

    public string GetValueAsString(IReadOnlyDictionary<string, object> dictionary, string key) {
      return JsonConvert.SerializeObject(GetValueOrDefault(dictionary, key));
    }

    public object GetValueOrDefault(IReadOnlyDictionary<string, object> dictionary, string key) {
      if (dictionary.ContainsKey(key)) {
        return dictionary[key];
      }

      return null;
    }
  }
}
