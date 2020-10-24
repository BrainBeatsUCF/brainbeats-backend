using brainbeats_backend.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.Utility;

namespace brainbeats_backend {
  public static class QueryStrings {
    // Create new Vertex
    public static async Task<string> CreateVertexQueryAsync(object obj) {
      return obj switch
      {
        Beat _ => await CreateVertexQueryAsync("beat", Guid.NewGuid().ToString(), obj, null).ConfigureAwait(false),
        Playlist _ => await CreateVertexQueryAsync("playlist", Guid.NewGuid().ToString(), obj, null).ConfigureAwait(false),
        Sample _ => await CreateVertexQueryAsync("sample", Guid.NewGuid().ToString(), obj, null).ConfigureAwait(false),
        User u => await CreateVertexQueryAsync("user", u.email, obj, null).ConfigureAwait(false),
        _ => "",
      };
    }

    public static async Task<string> CreateVertexQueryAsync(object obj, List<KeyValuePair<string, string>> edges) {
      return obj switch
      {
        Beat _ => await CreateVertexQueryAsync("beat", Guid.NewGuid().ToString(), obj, edges).ConfigureAwait(false),
        Playlist _ => await CreateVertexQueryAsync("playlist", Guid.NewGuid().ToString(), obj, edges).ConfigureAwait(false),
        Sample _ => await CreateVertexQueryAsync("sample", Guid.NewGuid().ToString(), obj, edges).ConfigureAwait(false),
        User u => await CreateVertexQueryAsync("user", u.email, obj, edges).ConfigureAwait(false),
        _ => "",
      };
    }

    public static async Task<string> CreateVertexQueryAsync(string vertexType, string vertexId, object obj, List<KeyValuePair<string, string>> edges) {
      StringBuilder queryString = new StringBuilder(CreateVertex(vertexType, vertexId));
      string seed = obj.GetType().GetProperty("seed").GetValue(obj) != null ?
        obj.GetType().GetProperty("seed").GetValue(obj).ToString() : null;

      foreach (PropertyInfo prop in obj.GetType().GetProperties()) {
        // Don't append seed, email, or id fields
        if (prop.Name.Equals("seed") || prop.Name.Equals("email") || prop.Name.Equals("id")) {
          continue;
        }

        if (prop.GetValue(obj) == null || string.IsNullOrWhiteSpace(prop.GetValue(obj).ToString())) {
          throw new ArgumentException($"{prop.Name} field is missing");
        }

        Type type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        if (type == typeof(IFormFile)) {
          IFormFile file = (IFormFile) prop.GetValue(obj);
          string extension = Path.GetExtension(file.FileName);

          // Reject improper file extensions
          if (prop.Name.Equals("audio") && (!extension.ToLowerInvariant().Equals(".wav") && !extension.ToLowerInvariant().Equals(".mp3"))) {
            throw new ArgumentException($"{prop.Name} file extension must be wav or mp3");
          }

          if (prop.Name.Equals("image") && (!extension.ToLowerInvariant().Equals(".jpg") && !extension.ToLowerInvariant().Equals(".png"))) {
            throw new ArgumentException($"{prop.Name} file extension must be jpg or png");
          }

          string fileName = $"{vertexId}_{prop.Name}";

          string url = await StorageConnection.Instance.UploadFileAsync(file, vertexType, fileName);
          queryString.Append(AddProperty(prop.Name, url));
        } else {
          string value = prop.GetValue(obj).ToString();
          queryString.Append(AddProperty(prop.Name, value));

          // If this is the "name" field, make a lowercased field for type insensitive searches
          if (prop.Name.Equals("name")) {
            queryString.Append(AddProperty("searchName", value.ToLowerInvariant()));
          }
        }
      }

      // Append the seed field if it is present
      if (seed != null && !string.IsNullOrWhiteSpace(seed)) {
        queryString.Append(AddProperty("seed", seed));
      }

      string currentTime = GetCurrentTime();
      queryString.Append(AddProperty("createdDate", currentTime));
      queryString.Append(AddProperty("modifiedDate", currentTime));

      if (edges != null) {
        foreach (KeyValuePair<string, string> pair in edges) {
          queryString.Append(CreateEdge(pair.Key, pair.Value) + EdgeSourceReference());
        }
      }

      return queryString.ToString();
    }

    // Updates existing Vertex; supports partial updates
    public static async Task<string> UpdateVertexQueryAsync(object obj) {
      return obj switch
      {
        Beat b => await UpdateVertexQueryAsync("beat", b.id, obj).ConfigureAwait(false),
        Playlist p => await UpdateVertexQueryAsync("playlist", p.id, obj).ConfigureAwait(false),
        Sample s => await UpdateVertexQueryAsync("sample", s.id, obj).ConfigureAwait(false),
        User u => await UpdateVertexQueryAsync("user", u.email, obj).ConfigureAwait(false),
        _ => "",
      };
    }

    public static async Task<string> UpdateVertexQueryAsync(string vertexType, string vertexId, object obj) {
      StringBuilder queryString = new StringBuilder(GetVertex(vertexId));

      foreach (PropertyInfo prop in obj.GetType().GetProperties()) {
        // Don't append seed, email, or id fields
        if (prop.Name.Equals("seed") || prop.Name.Equals("email") || prop.Name.Equals("id")) {
          continue;
        }

        // Skip null or empty fields
        if (prop.GetValue(obj) == null || string.IsNullOrWhiteSpace(prop.GetValue(obj).ToString())) {
          continue;
        }

        Type type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        if (type == typeof(IFormFile)) {
          IFormFile file = (IFormFile) prop.GetValue(obj);
          string extension = Path.GetExtension(file.FileName);

          // Reject improper file extensions
          if (prop.Name.Equals("audio") && (!extension.ToLowerInvariant().Equals(".wav") && !extension.ToLowerInvariant().Equals(".mp3"))) {
            throw new ArgumentException($"{prop.Name} file extension must be wav or mp3");
          }

          if (prop.Name.Equals("image") && (!extension.ToLowerInvariant().Equals(".jpg") && !extension.ToLowerInvariant().Equals(".png"))) {
            throw new ArgumentException($"{prop.Name} file extension must be jpg or png");
          }

          string fileName = $"{vertexId}_{prop.Name}";

          string url = await StorageConnection.Instance.UploadFileAsync(file, vertexType, fileName);
          queryString.Append(AddProperty(prop.Name, url));
        } else {
          string value = prop.GetValue(obj).ToString();
          queryString.Append(AddProperty(prop.Name, value));

          // If this is the "name" field, make a lowercased field for type insensitive searches
          if (prop.Name.Equals("name")) {
            queryString.Append(AddProperty("searchName", value.ToLowerInvariant()));
          }
        }
      }

      queryString.Append(AddProperty("modifiedDate", GetCurrentTime()));

      return queryString.ToString();
    }

    public static string ValidateVertexOwnershipQuery(string email, string vertexId) {
      return GetVertex(vertexId) + GetOutNeighbors("OWNED_BY");
    }

    // Returns the specified vertex
    public static string ReadVertexQuery(string vertexId) {
      return GetVertex(vertexId);
    }

    // Searches the specified vertex
    public static string SearchVertexQuery(string vertexType, string searchWord) {
      return GetAllVertices(vertexType) + HasProperty("searchName", searchWord);
    }

    // Deletes the specified vertex
    public static string DeleteVertexQuery(string vertexId) {
      return GetVertex(vertexId) + Delete();
    }

    // Deletes an outgoing edge with a certain type to the specified neighbor
    public static string DeleteOutNeighborQuery(string edgeType, string sourceVertexId, string destVertexId) {
      return GetVertex(sourceVertexId) + AddProperty("modifiedDate", GetCurrentTime()) + GetOutEdge(edgeType, destVertexId) + Delete();
    }

    // Creates an outgoing edge with a certain type to the specified neighbor
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
