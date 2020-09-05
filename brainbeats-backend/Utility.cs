using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public static string DeleteVertex(string vertexId) {
      return $"g.V('{vertexId}').drop()";
    }

    public static string GetVertex(string vertexId) {
      return $"g.V('{vertexId}')";
    }

    public static string CreateEdge(string edgeType, string dest) {
      return $".addE('{edgeType}').to(g.V('{dest}'))";
    }

    public static string GetNeighbors(string edgeType) {
      return $".out('{edgeType}')";
    }

    public static string AddProperty(string propertyType, string propertyValue) {
      if (propertyValue == null) {
        throw new ArgumentException($"Missing PropertyValue {propertyValue}");
      }

      return $".property('{propertyType}', '{propertyValue}')";
    }
  }
}
