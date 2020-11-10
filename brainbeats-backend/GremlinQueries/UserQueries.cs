using brainbeats_backend.Controllers;
using Gremlin.Net.Driver;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.GremlinQueries {
  public static class UserQueries {
    private static readonly string defaultProfilePicture = $"{StorageConnection.Instance.StorageEndpoint}/static/profile_picture.png";
    private static readonly HashSet<string> relationships = new HashSet<string>() { "LIKES", "RECOMMENDED", "OWNED_BY" };
    private static readonly HashSet<string> types = new HashSet<string>() { "beat", "playlist", "sample" };

    public static async Task<ResultSet<dynamic>> CreateUserVertexQuery(string email, UserVertex u) {
      email = email.ToLowerInvariant();
      StringBuilder queryString = new StringBuilder(CreateVertex("user", email));

      try {
        foreach (PropertyInfo prop in u.GetType().GetProperties()) {
          // Skip "image" field for creating new users
          if (prop.Name.ToLowerInvariant().Equals("image")) {
            continue;
          }

          // Skip null or empty fields
          if (prop.GetValue(u) == null || string.IsNullOrWhiteSpace(prop.GetValue(u).ToString())) {
            throw new ArgumentException($"{prop.Name} must not be empty");
          }

          queryString.Append(await SetField(prop, "user", email, u).ConfigureAwait(false));
        }

        // Append the name field
        string name = $"{u.firstName} {u.lastName}";
        queryString.Append(AddProperty("name", name));

        // Append the lowercase searchName field
        queryString.Append(AddProperty("searchName", name.ToLowerInvariant()));

        // Attach the default image to new users
        queryString.Append(AddProperty("image", defaultProfilePicture));
      } catch {
        throw;
      }

      try {
        return await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
      } catch {
        throw;
      }
    }

    public static async Task<dynamic> GetUserVertexNeighborsQuery(string email, string relationship, string type) {
      email = email.ToLowerInvariant();
      relationship = relationship.ToUpperInvariant();
      if (!relationships.Contains(relationship)) {
        throw new ArgumentException("Relationship must be one of: " + string.Join(" ", relationships));
      }

      string queryString;

      // If type is unspecified, then get all vertices with a given relationship
      if (string.IsNullOrWhiteSpace(type)) {
        queryString = GetVertex(email) + GetOutNeighbors(relationship);
      } else {
        type = type.ToLowerInvariant();
        if (!types.Contains(type)) {
          throw new ArgumentException("Type must be one of: " + string.Join(" ", types));
        }

        queryString = GetVertex(email) + GetOutNeighbors(relationship) + HasLabel(type);
      }

      ResultSet<dynamic> result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

      List<dynamic> resultList = await PopulatePlaylistLength(result);
      resultList = await PopulateVertexOwners(resultList);

      return resultList;
    }

    public static async Task<ResultSet<dynamic>> UpdateUserVertexQuery(string email, UserVertex u) {
      email = email.ToLowerInvariant();
      StringBuilder queryString = new StringBuilder(GetVertex(email));

      try {
        foreach (PropertyInfo prop in u.GetType().GetProperties()) {
          // Skip null or empty fields
          if (prop.GetValue(u) == null || string.IsNullOrWhiteSpace(prop.GetValue(u).ToString())) {
            continue;
          }

          queryString.Append(await SetField(prop, "user", email, u).ConfigureAwait(false));
        }

        // First name and last name must either be:
        // 1. Both included, or 
        // 2. Both excluded
        // in a valid request
        if ((string.IsNullOrWhiteSpace(u.firstName) && !string.IsNullOrWhiteSpace(u.lastName)) ||
          (!string.IsNullOrWhiteSpace(u.firstName) && string.IsNullOrWhiteSpace(u.lastName))) {
          throw new ArgumentException("Both firstName and lastName must be included for name updates");
        }

        // Append the name field
        string name = $"{u.firstName} {u.lastName}";
        queryString.Append(AddProperty("name", name));

        // Append the lowercase searchName field
        queryString.Append(AddProperty("searchName", name.ToLowerInvariant()));
      } catch {
        throw;
      }

      try {
        return await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
      } catch {
        throw;
      }
    }

    public static async Task<ResultSet<dynamic>> ReadUserVertexQuery(string email) {
      email = email.ToLowerInvariant();
      string queryString = GetVertex(email);

      try {
        return await DatabaseConnection.Instance.ExecuteQuery(queryString);
      } catch {
        throw;
      }
    }

    public static async Task<ResultSet<dynamic>> DeleteUserVertexQuery(string email) {
      email = email.ToLowerInvariant();
      string queryString = GetVertex(email) + Delete();

      try {
        // Delete the png or jpg picture associated with this User
        await StorageConnection.Instance.DeleteFileAsync("user", $"{email}_image.jpg");
        await StorageConnection.Instance.DeleteFileAsync("user", $"{email}_image.png");

        return await DatabaseConnection.Instance.ExecuteQuery(queryString);
      } catch {
        throw;
      }
    }
  }
}
