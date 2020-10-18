using brainbeats_backend.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.Utility;

namespace brainbeats_backend {
  public static class QueryStrings {
    // Creates a new vertex with an input JObject
    public static string CreateVertexQuery(string vertexType, string vertexId, JObject body) {
      StringBuilder queryString = new StringBuilder(CreateVertex(vertexType, vertexId));

      foreach (PropertyInfo prop in GetSchema(vertexType)) {
        // All fields in the schema are required
        if (body.ContainsKey(prop.Name)) {
          queryString.Append(AddProperty(prop.Name, body.GetValue(prop.Name).ToString()));
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

    // Creates a new vertex with outgoing edges asynchronously with file uploads with an input Object
    public static async Task<string> CreateVertexQueryAsync(string vertexType, Object obj) {
      StringBuilder queryString = new StringBuilder(CreateVertex(vertexType, Guid.NewGuid().ToString()));

      foreach (PropertyInfo prop in obj.GetType().GetProperties()) {
        var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        if (type == typeof(IFormFile)) {
          string url = await StorageConnection.Instance.UploadFileAsync((IFormFile) prop.GetValue(obj, null), vertexType);
          queryString.Append(AddProperty(prop.Name, url));
        } else {
          // Append if prop is not null and prop is not seed or email
          if (prop.GetValue(obj) != null && !prop.Name.Equals("seed") && !prop.Name.Equals("email")) {
            queryString.Append(AddProperty(prop.Name, prop.GetValue(obj).ToString()));

            if (prop.Name.Equals("name")) {
              queryString.Append(AddProperty("searchName", prop.GetValue(obj).ToString().ToLower()));
            }
          }
        }
      }

      queryString.Append(AddProperty("createdDate", GetCurrentTime()));
      queryString.Append(AddProperty("modifiedDate", GetCurrentTime()));

      return queryString.ToString();
    }

    // Creates a new vertex with outgoing edges asynchronously with file uploads with an input Object,
    // and creates corresponding edges; ach pair in the edges list has the schema <edge type, destination>
    public static async Task<string> CreateVertexQueryAsync(string vertexType, Object obj, List<KeyValuePair<string, string>> edges) {
      StringBuilder queryString = new StringBuilder(await CreateVertexQueryAsync(vertexType, obj).ConfigureAwait(false));

      foreach (KeyValuePair<string, string> pair in edges) {
        queryString.Append(CreateEdge(pair.Key, pair.Value) + EdgeSourceReference());
      }

      return queryString.ToString();
    }

    // Creates a new vertex with outgoing edges with an input JObject,
    // and creates corresponding edges; ach pair in the edges list has the schema <edge type, destination>
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

    // Searches the specified vertex
    public static string SearchVertexQuery(string vertexType, string searchWord) {
      return GetAllVertices(vertexType) + HasProperty("searchName", searchWord);
    }

    // Updates the specified vertex
    public static string UpdateVertexQuery(string vertexType, string vertexId, JObject body) {
      StringBuilder queryString = new StringBuilder(GetVertex(vertexId));

      foreach (PropertyInfo prop in GetSchema(vertexType)) {
        if (body.ContainsKey(prop.Name)) {
          queryString.Append(AddProperty(prop.Name, body.GetValue(prop.Name).ToString()));
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
