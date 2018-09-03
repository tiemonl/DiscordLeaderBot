using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace LeaderBot
{
	class Program
	{
		static void Main(string[] args) => new LeaderBot().MainAsync().GetAwaiter().GetResult();
	}  
}
