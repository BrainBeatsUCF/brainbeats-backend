using brainbeats_backend.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace brainbeats_backend {
  public static class Utility {
    // Ensures the incoming controller request is of type String
    public static string GetRequest(dynamic req) {
      return req.GetType().Equals(typeof(string)) ? req : req.ToString();
    }

    // Gets the current UNIX time
    public static string GetCurrentTime() {
      int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      return unixTimestamp.ToString();
    }

    public static PropertyInfo [] GetSchema(string vertexType) {
      string type = vertexType.ToLowerInvariant().Trim();

      return type switch
      {
        "user" => new User().GetType().GetProperties(),
        "beat" => new Beat().GetType().GetProperties(),
        "sample" => new Sample().GetType().GetProperties(),
        "playlist" => new Playlist().GetType().GetProperties(),
        _ => null,
      };
    }

    public static JObject DeserializeRequest(dynamic req) {
      string request = GetRequest(req);
      JObject body = JsonConvert.DeserializeObject<dynamic>(request);

      return body;
    }
  }
}
