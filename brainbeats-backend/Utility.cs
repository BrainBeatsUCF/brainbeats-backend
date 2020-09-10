using System;

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

    // Creates a new vertex with the partitioning key "type"
    public static string CreateVertex(string vertexType, string vertexId) {
      return $"g.addV('{vertexType}')" + 
        AddProperty("type", vertexType) + 
        AddProperty("id", vertexId);
    }

    // Deletes the current vertex or edge
    public static string Delete() {
      return $".drop()";
    }

    // Returns the specified vertex by key
    public static string GetVertex(string vertexId) {
      return $"g.V('{vertexId}')";
    }

    // Creates a new edge and adds the current time as a property
    public static string CreateEdge(string edgeType, string dest) {
      return $".addE('{edgeType}').to(g.V('{dest}'))" + AddProperty("date", GetCurrentTime());
    }

    // Returns the source of the selected edge
    public static string EdgeSourceReference() {
      return ".outV()";
    }

    // Returns the edge that joins the current vertex and matches the specified edge type 
    // and destination
    public static string GetEdge(string edgeType, string dest) {
      return $".outE('{edgeType}').where(inV().has('id', '{dest}'))";
    }

    // Get the neighbors of the current vertex by specified edge type
    public static string GetNeighbors(string edgeType) {
      return $".out('{edgeType}')";
    }

    public static string AddProperty(string propertyType, string propertyValue) {
      return AddProperty(propertyType, propertyValue, true);
    }

    // Adds a new property to the specified vertex or edge
    public static string AddProperty(string propertyType, string propertyValue, bool required) {
      // If the property value is null and required, throw an argument exception
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
