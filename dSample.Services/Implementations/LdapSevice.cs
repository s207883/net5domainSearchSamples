using dSample.Models;
using dSample.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace dSample.Services.Implementations
{
	public class LdapSevice : IDisposable, ILdapSevice
	{
		private LdapConnection _ldapConnection;

		public Task EstablishConnectionAsync(string domainUrl, int domainPort, string ldapUserDomain, string ldapUserName, string ldapUserPassword, bool disableSecurity = false)
		{
			#region Param check.
			if (string.IsNullOrWhiteSpace(domainUrl))
			{
				throw new ArgumentException($"{nameof(domainUrl)} cannot be null or whitespace.", nameof(domainUrl));
			}

			if (domainPort <= 0)
			{
				throw new ArgumentException($"'{nameof(domainPort)}' cannot be less or equal zero.", nameof(domainPort));
			}

			if (string.IsNullOrWhiteSpace(ldapUserDomain))
			{
				throw new ArgumentException($"'{nameof(ldapUserDomain)}' cannot be null or whitespace.", nameof(ldapUserDomain));
			}

			if (string.IsNullOrWhiteSpace(ldapUserName))
			{
				throw new ArgumentException($"{nameof(ldapUserName)} cannot be null or whitespace.", nameof(ldapUserName));
			}

			if (string.IsNullOrWhiteSpace(ldapUserPassword))
			{
				throw new ArgumentException($"{nameof(ldapUserPassword)} cannot be null or whitespace.", nameof(ldapUserPassword));
			}
			#endregion

			var networkCredentials = new NetworkCredential(ldapUserName, ldapUserPassword, ldapUserDomain);
			if (_ldapConnection == default)
			{
				var ldapIdentifier = new LdapDirectoryIdentifier(domainUrl, domainPort);
				_ldapConnection = new LdapConnection(ldapIdentifier, networkCredentials);
			}

			if (disableSecurity)
			{
				_ldapConnection.SessionOptions.SecureSocketLayer = false;
				_ldapConnection.SessionOptions.VerifyServerCertificate = (LdapConnection _, X509Certificate _) => true;
			}

			_ldapConnection.Bind(networkCredentials);

			return Task.CompletedTask;
		}

		public Task<DomainUser> FindUserByNameAsync(string userName, string searchStartPosition)
		{
			#region Param check
			if (string.IsNullOrWhiteSpace(userName))
			{
				throw new ArgumentException($"'{nameof(userName)}' cannot be null or whitespace.", nameof(userName));
			}
			#endregion

			var request = new SearchRequest(searchStartPosition, $"samaccountname={userName}", SearchScope.Subtree);
			request.Attributes.AddRange(
				new[]
				{
					"sAMAccountName",
					"displayName",
					"mail",
					"memberof",
				}
				);
			var result = (SearchResponse)_ldapConnection.SendRequest(request);
			if (result.Entries.Count == 0)
			{
				return default;
			}

			var entry = result.Entries[0];
			var user = new DomainUser()
			{
				UserIdentity = GetDecodedString(entry, "sAMAccountName"),
				UserEmail = GetDecodedString(entry, "mail"),
				DisplayName = GetDecodedString(entry, "sAMAccountName"),
				UserDomainGroups = GetUserGroups(entry, "memberof"),
			};

			return Task.FromResult(user);
		}

		private static string GetDecodedString(SearchResultEntry searchResult, string attributeName)
		{
			#region Param check
			if (searchResult is null)
			{
				throw new ArgumentNullException(nameof(searchResult));
			}

			if (string.IsNullOrWhiteSpace(attributeName))
			{
				throw new ArgumentException($"'{nameof(attributeName)}' cannot be null or whitespace.", nameof(attributeName));
			}
			#endregion

			var attributes = searchResult.Attributes[attributeName];
			if (attributes.Count == 0)
			{
				return default;
			}
			var attributeString = attributes[0] as string;

			return attributeString;
		}

		private static IEnumerable<DomainGroup> GetUserGroups(SearchResultEntry searchResult, string attributeName)
		{
			#region Param ckeck
			if (searchResult is null)
			{
				throw new ArgumentNullException(nameof(searchResult));
			}

			if (string.IsNullOrEmpty(attributeName))
			{
				throw new ArgumentException($"'{nameof(attributeName)}' cannot be null or empty.", nameof(attributeName));
			}
			#endregion

			var rawUserGroups = searchResult.Attributes[attributeName];
			var userGroups = new List<DomainGroup>();
			foreach (var userGroup in rawUserGroups)
			{
				var rawValue = Encoding.UTF8.GetString(userGroup as byte[]);
				if (string.IsNullOrWhiteSpace(rawValue))
				{
					continue;
				}

				var groupName = rawValue.Split(',').FirstOrDefault().Replace("CN=", "");
				userGroups.Add(new() { DomainGroupRawName = rawValue, DomainGroupClearName = groupName });
			}
			return userGroups;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);

			_ldapConnection?.Dispose();
		}
	}
}
