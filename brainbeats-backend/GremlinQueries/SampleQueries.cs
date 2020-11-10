using brainbeats_backend.Controllers;
using Gremlin.Net.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static brainbeats_backend.QueryBuilder;
using static brainbeats_backend.QueryStrings;
using static brainbeats_backend.Utility;

namespace brainbeats_backend.GremlinQueries {
  public static class SampleQueries {
    public static async Task<ResultSet<dynamic>> CreateSampleVertexQuery(string email, SampleVertex p, string seed = null) {
      string sampleId = Guid.NewGuid().ToString();
      email = email.ToLowerInvariant();

      StringBuilder queryString = new StringBuilder(CreateVertex("sample", sampleId));

      try {
        foreach (PropertyInfo prop in p.GetType().GetProperties()) {
          // Skip null or empty fields
          if (prop.GetValue(p) == null || string.IsNullOrWhiteSpace(prop.GetValue(p).ToString())) {
            throw new ArgumentException($"{prop.Name} must not be empty");
          }

          queryString.Append(await SetField(prop, "sample", sampleId, p).ConfigureAwait(false));
        }

        if (!string.IsNullOrWhiteSpace(seed)) {
          queryString.Append(AddProperty("seed", seed));
        }

        queryString.Append(CreateEdge("OWNED_BY", email) + EdgeSourceReference());
      } catch {
        throw;
      }

      try {
        return await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
      } catch {
        throw;
      }
    }

    public static async Task<ResultSet<dynamic>> UpdateSampleVertexQuery(string sampleId, SampleVertex p) {
      StringBuilder queryString = new StringBuilder(GetVertex(sampleId));

      try {
        foreach (PropertyInfo prop in p.GetType().GetProperties()) {
          // Skip null or empty fields
          if (prop.GetValue(p) == null || string.IsNullOrWhiteSpace(prop.GetValue(p).ToString())) {
            continue;
          }

          queryString.Append(await SetField(prop, "sample", sampleId, p).ConfigureAwait(false));
        }
      } catch {
        throw;
      }

      try {
        return await DatabaseConnection.Instance.ExecuteQuery(queryString.ToString());
      } catch {
        throw;
      }
    }

    public static async Task<List<dynamic>> SearchSampleVertexQuery(string name = null, string email = null) {
      try {
        // If name and email is null, get public Beats
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email)) {
          string queryString = GetAllPublicVerticesQuery("sample");

          ResultSet<dynamic> result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

          List<dynamic> resultList = await PopulatePlaylistLength(result);
          resultList = await PopulateVertexOwners(resultList);

          return resultList;
        }
        // If name is not null and email is null, search public Beats
        else if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email)) {
          string queryString = SearchPublicVertexQuery("sample", name);

          ResultSet<dynamic> result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

          List<dynamic> resultList = await PopulatePlaylistLength(result);
          resultList = await PopulateVertexOwners(resultList);

          return resultList;
        }
        // If name is null and email is not null, get public and owned Beats
        else if (string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email)) {
          string queryStringPublic = GetAllPublicVerticesQuery("sample");
          string queryStringPrivate = GetAllOwnedVerticesQuery("sample", email);

          ResultSet<dynamic> resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
          ResultSet<dynamic> resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);

          List<dynamic> resultList = await PopulateVertexOwners(resultsPublic.Concat(resultsPrivate));
          return resultList;
        }
        // If name and email is not null, search public and owned Beats
        else {
          string queryStringPublic = SearchPublicVertexQuery("sample", name);
          string queryStringPrivate = SearchOwnedVertexQuery("sample", email, name);

          ResultSet<dynamic> resultsPublic = await DatabaseConnection.Instance.ExecuteQuery(queryStringPublic);
          ResultSet<dynamic> resultsPrivate = await DatabaseConnection.Instance.ExecuteQuery(queryStringPrivate);

          List<dynamic> resultList = await PopulateVertexOwners(resultsPublic.Concat(resultsPrivate));
          return resultList;
        }
      } catch {
        throw;
      }
    }

    public static async Task<List<dynamic>> ReadSampleVertexQuery(string sampleId) {
      string queryString = GetVertex(sampleId);

      try {
        ResultSet<dynamic> result = await DatabaseConnection.Instance.ExecuteQuery(queryString);

        List<dynamic> resultList = await PopulatePlaylistLength(result);
        resultList = await PopulateVertexOwners(resultList);

        return resultList;
      } catch {
        throw;
      }
    }

    public static async Task<ResultSet<dynamic>> DeleteSampleVertexQuery(string sampleId) {
      string queryString = GetVertex(sampleId) + Delete();

      try {
        // IMPORTANT: Don't delete Sample audio files when deleting Sample vertices

        return await DatabaseConnection.Instance.ExecuteQuery(queryString);
      } catch {
        throw;
      }
    }
  }
}