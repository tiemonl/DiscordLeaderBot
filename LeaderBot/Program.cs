using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace LeaderBot
{
    class Program
    {
        static void Main(string[] args) => new LeaderBot()
            .MainAsync(token: args[0], prefix: args[1], mongoUrl: args[2])
            .GetAwaiter()
            .GetResult();
    }
}
