using System.Collections.Generic;

namespace dSample.Models
{
	public record DomainUser
	{
		public string UserIdentity { get; init; }
		public string UserEmail { get; init; }
		public string DisplayName { get; init; }
		public IEnumerable<DomainGroup> UserDomainGroups { get; init; }
	}
}
