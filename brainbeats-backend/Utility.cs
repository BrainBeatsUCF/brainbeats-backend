using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace brainbeats_backend {
  public class Utility {
    // Ensures the incoming controller request is of type String
    public static string GetRequest(dynamic req) {
      return req.GetType().Equals(typeof(string)) ? req : req.ToString();
    }

    // Gets the current UNIX time
    public static string GetCurrentTime() {
      int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      return unixTimestamp.ToString();
    }

    public static HashSet<string> GetSchema(string vertexType) {
      switch (vertexType.ToLower().Trim()) {
        case "user":
          return new HashSet<string> { "firstName", "lastName" };
        case "beat":
          return new HashSet<string> { "name", "duration", "image", "isPrivate", "instrumentList", "attributes", "audio" };
        case "sample":
          return new HashSet<string> { "name", "isPrivate", "attributes", "audio" };
        case "playlist":
          return new HashSet<string> { "name", "image", "isPrivate" };
        default:
          return null;
      }
    }

    public static JObject DeserializeRequest(dynamic req) {
      string request = GetRequest(req);
      JObject body = JsonConvert.DeserializeObject<dynamic>(request);

      return body;
    }
  }
}
