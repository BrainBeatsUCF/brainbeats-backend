using System;

namespace brainbeats_backend {
  public static class Utility {
    public static string GetRequest(dynamic req) {
      return req.GetType().Equals(typeof(string)) ? req : req.ToString();
    }

    public static string GetCurrentTime() {
      int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
      return unixTimestamp.ToString();
    }

    public static string CreateVertex(string vertexType, string vertexId) {
      return $"g.addV('{vertexType}')" + 
        AddProperty("type", vertexType) + 
        AddProperty("id", vertexId);
    }

    // g.V().hasLabel('beat').has("name", "test_beat_name_1").values('id')
    // g.V('test_email_1_56789@email.com').addE('LIKES').to(g.V().hasLabel('beat').has("name", "test_beat_name_1").values('id')).outV()

    public static string Delete() {
      return $".drop()";
    }

    public static string GetVertex(string vertexId) {
      return $"g.V('{vertexId}')";
    }

    public static string CreateEdge(string edgeType, string dest) {
      return $".addE('{edgeType}').to(g.V('{dest}'))" + AddProperty("date", GetCurrentTime()) + ".outV()";
    }

    public static string GetEdge(string edgeType, string dest) {
      return $".outE('{edgeType}').where(inV().has('id', '{dest}'))";
    }

    public static string GetNeighbors(string edgeType) {
      return $".out('{edgeType}')";
    }

    public static string AddProperty(string propertyType, string propertyValue, bool required = true) {
      if (propertyValue == null) {
        if (required) {
          throw new ArgumentException($"Missing PropertyValue {propertyValue}");
        } else {
          return "";
        }
      }

      return $".property('{propertyType}', '{propertyValue}')";
    }
  }
}
