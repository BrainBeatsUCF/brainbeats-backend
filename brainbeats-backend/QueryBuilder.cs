using System;

namespace brainbeats_backend {
  public static class QueryBuilder {
    // Creates a new vertex with the partitioning key "type"
    public static string CreateVertex(string vertexType, string vertexId) {
      return $"g.addV('{vertexType}')" + 
        AddProperty("type", vertexType) + 
        AddProperty("id", vertexId);
    }

    public static string CreateVertex(string vertexType) {
      return $"g.addV('{vertexType}')" + AddProperty("type", vertexType);
    }

    // Deletes the current vertex or edge
    public static string Delete() {
      return $".drop()";
    }

    // Returns the specified vertex by key
    public static string GetVertex(string vertexId) {
      return $"g.V('{vertexId}')";
    }

    // Returns all vertices of a given type
    public static string GetAllVertices(string vertexType) {
      return $"g.V().hasLabel('{vertexType}')";
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

    // Get the outgoing neighbors of the current vertex by specified edge type
    public static string GetOutNeighbors(string edgeType) {
      return $".out('{edgeType}')";
    }

    // Get the incoming neighbors of the current vertex by specified edge type
    public static string GetInNeighbors(string edgeType) {
      return $".inE('{edgeType}')";
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

    // Require a vertex or edge property type to match a specified input
    public static string HasProperty(string propertyType, string propertyValue) {
      return $".has('{propertyType}', '{propertyValue}')";
    }

    // Require a vertex or edge label type to match a specified input
    public static string HasLabel(string labelType) {
      return $".hasLabel('{labelType}')";
    }
  }
}
