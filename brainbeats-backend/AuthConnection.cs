using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brainbeats_backend {
  public class AuthConnection {
    public static AuthConnection Instance { get; set; }

    private GraphServiceClient graphClient { get; set; }

    public static void Init(IConfiguration configuration) {
      Instance = new AuthConnection(configuration);
    }

    private AuthConnection(IConfiguration configuration) {
      string appId = configuration["Auth:AppId"];
      string tenantId = configuration["Auth:TenantId"];
      string clientSecret = configuration["Auth:ClientSecret"];

      // Initialize new Microsoft Graph client
      IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
          .Create(appId)
          .WithTenantId(tenantId)
          .WithClientSecret(clientSecret)
          .Build();
      ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

      graphClient = new GraphServiceClient(authProvider);
    }

    public async Task ListUsers() {
      Console.WriteLine("Getting list of users...");

      // Get all users (one page)
      var result = await this.graphClient.Users
          .Request()
          .Select(e => new {
            e.DisplayName,
            e.Id,
            e.Identities
          })
          .GetAsync();

      foreach (var user in result.CurrentPage) {
        Console.WriteLine(JsonConvert.SerializeObject(user));
      }
    }

    public async Task CreateUser(string name, string email, string password) {
      var user = new User {
        AccountEnabled = true,
        DisplayName = name,
        MailNickname = email,
        UserPrincipalName = email,
        PasswordProfile = new PasswordProfile {
          ForceChangePasswordNextSignIn = false,
          ForceChangePasswordNextSignInWithMfa = false,
          Password = password
        }
      };

      await graphClient.Users
        .Request()
        .AddAsync(user);
    }

    public async Task LoginUser(string name, string email, string password) {
      var user = new User {
        AccountEnabled = true,
        DisplayName = name,
        MailNickname = email,
        UserPrincipalName = email,
        PasswordProfile = new PasswordProfile {
          ForceChangePasswordNextSignIn = false,
          ForceChangePasswordNextSignInWithMfa = false,
          Password = password
        }
      };

      await graphClient.Users
        .Request()
        .AddAsync(user);
    }
  }
}
