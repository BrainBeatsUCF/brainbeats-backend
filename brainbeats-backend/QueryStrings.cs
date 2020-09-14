using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.Utility;

namespace brainbeats_backend {
  public static class QueryStrings {
    // Creates a new vertex
    public static string CreateVertexQuery(string vertexType, string vertexId, JObject body) {
      HashSet<string> schema = GetSchema(vertexType);
      StringBuilder queryString = new StringBuilder(CreateVertex(vertexType, vertexId));

      foreach (string field in schema) {
        // All fields in the schema are required
        if (body.ContainsKey(field)) {
          queryString.Append(AddProperty(field, body.GetValue(field).ToString()));
        } else {
          throw new ArgumentException();
        }
      }

      queryString.Append(AddProperty("createdDate", GetCurrentTime()));
      queryString.Append(AddProperty("modifiedDate", GetCurrentTime()));

      if (body.ContainsKey("seed")) {
        queryString.Append(AddProperty("seed", body.GetValue("seed").ToString()));
      }

      return queryString.ToString();
    }

    // Creates a new vertex with outgoing edges
    // Each pair in the edges list has the schema <edge type, destination>
    public static string CreateVertexQuery(string vertexType, string vertexId, JObject body, List<KeyValuePair<string, string>> edges) {
      StringBuilder queryString = new StringBuilder(CreateVertexQuery(vertexType, vertexId, body));

      foreach (KeyValuePair<string, string> pair in edges) {
        queryString.Append(CreateEdge(pair.Key, pair.Value) + EdgeSourceReference());
      }

      return queryString.ToString();
    }

    // Returns the specified vertex
    public static string ReadVertexQuery(string vertexId) {
      return GetVertex(vertexId);
    }

    // Updates the specified vertex
    public static string UpdateVertexQuery(string vertexType, string vertexId, JObject body) {
      HashSet<string> schema = GetSchema(vertexType);
      StringBuilder queryString = new StringBuilder(GetVertex(vertexId));

      foreach (string field in schema) {
        if (body.ContainsKey(field)) {
          queryString.Append(AddProperty(field, body.GetValue(field).ToString()));
        }
      }

      return queryString.ToString();
    }

    // Deletes the specified vertex
    public static string DeleteVertexQuery(string vertexId) {
      return GetVertex(vertexId) + Delete();
    }

    public static string DeleteOutNeighborQuery(string edgeType, string sourceVertexId, string destVertexId) {
      return GetVertex(sourceVertexId) + AddProperty("modifiedDate", GetCurrentTime()) + GetOutEdge(edgeType, destVertexId) + Delete();
    }

    public static string CreateOutNeighborQuery(string edgeType, string sourceVertexId, string destVertexId) {
      return GetVertex(sourceVertexId) + AddProperty("modifiedDate", GetCurrentTime()) + CreateEdge(edgeType, destVertexId);
    }

    // Gets all owned vertices of a specific type owned by a given email
    public static string GetAllOwnedVerticesQuery(string vertexType, string email) {
      return GetVertex(email) + GetInNeighbors("OWNED_BY") + EdgeSourceReference() + HasLabel(vertexType);
    }

    // Gets all privately owned vertices of a specific type owned by a given email
    public static string GetAllPrivateVerticesQuery(string vertexType, string email) {
      return GetAllOwnedVerticesQuery(vertexType, email) + HasProperty("isPrivate", "true");
    }

    // Gets all public vertices of a specific type
    public static string GetAllPublicVerticesQuery(string vertexType) {
      return GetAllVertices(vertexType) + HasProperty("isPrivate", "false");
    }

    // Gets neighbors that have an incoming edge coming from the vertexId
    public static string GetOutNeighborsQuery(string vertexType, string edgeType, string vertexId) {
      return GetVertex(vertexId) + GetOutNeighbors(edgeType) + HasLabel(vertexType);
    }

    // Gets neighbors that have an incoming edge directed towards the vertexId
    public static string GetInNeighborsQuery(string vertexType, string edgeType, string vertexId) {
      return GetVertex(vertexId) + GetInNeighbors(edgeType) + EdgeSourceReference() + HasLabel(vertexType);
    }
  }
}
