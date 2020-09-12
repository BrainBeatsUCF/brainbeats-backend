using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.Utility;

namespace brainbeats_backend {
  public class QueryStrings {
    public static string GetAllOwnedVerticesQuery(string vertexType, string email) {
      return GetVertex(email) + GetInNeighbors("OWNED_BY") + EdgeSourceReference() + HasLabel(vertexType);
    }
    public static string GetAllPrivatelyOwnedVerticesQuery(string vertexType, string email) {
      return GetAllOwnedVerticesQuery(vertexType, email) + HasProperty("isPrivate", "true");
    }

    public static string GetAllPublicVerticesQuery(string vertexType) {
      return GetAllVertices(vertexType) + HasProperty("isPrivate", "false");
    }

    public static string GetLikedVerticesQuery(string vertexType, string email) {
      return GetVertex(email) + GetOutNeighbors("LIKES") + HasLabel(vertexType);
    }

    public static string GetOwnedVerticesQuery(string vertexType, string email) {
      return GetVertex(email) + GetInNeighbors("OWNED_BY") + EdgeSourceReference() + HasLabel(vertexType);
    }

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

      return queryString.ToString();
    }

    public static string ReadVertexQuery(string vertexId) {
      return GetVertex(vertexId);
    }

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

    public static string DeleteVertexQuery(string vertexId) {
      return GetVertex(vertexId) + Delete();
    }
  }
}
