using dSample.Models;
using System.Threading.Tasks;

namespace dSample.Services.Interfaces
{
	public interface ILdapSevice
	{
		void Dispose();
		Task EstablishConnectionAsync(string domainUrl, int domainPort, string ldapUserDomain, string ldapUserName, string ldapUserPassword, bool disableSecurity = false);
		Task<DomainUser> FindUserByNameAsync(string userName, string searchStartPosition);
	}
}