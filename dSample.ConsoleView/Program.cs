using dSample.Services.Implementations;
using dSample.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace dSample.ConsoleView
{
	static class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine(new string('-', 25));
			Console.WriteLine("We are looking for domain user with name: 'user' in domain controller (192.168.0.102:389) of 'MYDOMAIN.LOCAL' domain.");
			Console.WriteLine("Domain admin cred`s: admin - 1234567890Abc");
			Console.WriteLine(new string('-',25));
			Console.WriteLine();
			Task.WaitAll(new Task[] { GetResult() });
		}

		private static async Task GetResult()
		{
			ILdapSevice ldapService = new LdapSevice();
			await ldapService.EstablishConnectionAsync("192.168.0.102", 389, "mydomain", "admin", "1234567890Abc");
			var user = await ldapService.FindUserByNameAsync("user2", "dc=mydomain,dc=local");

			Console.WriteLine(new string('-',25));
			Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(user));
			Console.WriteLine(new string('-', 25));
		}
	}
}
