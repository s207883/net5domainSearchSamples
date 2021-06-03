using dSample.Models;
using System;
using System.Threading.Tasks;

namespace dSample.Services.Interfaces
{
	/// <summary>
	/// LDAP Search service.
	/// </summary>
	public interface ILdapSevice: IDisposable
	{
		/// <summary>
		/// Establish connection with ldap server.
		/// </summary>
		/// <param name="domainUrl">LDAP server host.</param>
		/// <param name="domainPort">LDAP server port.</param>
		/// <param name="ldapUserDomain">LDAP user domain.</param>
		/// <param name="ldapUserName">User name.</param>
		/// <param name="ldapUserPassword">User password.</param>
		/// <param name="disableSecurity">Disable certificate and SSL checking.</param>
		/// <returns></returns>
		Task EstablishConnectionAsync(string domainUrl, int domainPort, string ldapUserDomain, string ldapUserName, string ldapUserPassword, bool disableSecurity = false);

		/// <summary>
		/// Find domain user by user login.
		/// </summary>
		/// <param name="userName">User login.</param>
		/// <param name="searchStartPosition">Specify domain directory. Search scope - subtree. </param>
		/// <returns></returns>
		Task<DomainUser> FindUserByNameAsync(string userName, string searchStartPosition);
	}
}