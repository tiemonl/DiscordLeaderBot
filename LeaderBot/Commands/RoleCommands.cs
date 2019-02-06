using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Text;
using MongoDB.Bson;
using LeaderBot.Utils;

namespace LeaderBot {
    [Group("admin"), RequireUserPermission(GuildPermission.Administrator)]
    public class RoleCommands : ModuleBase {
        public Random rand = new Random();

        public RoleCommands() {
        }

        //https://docs.stillu.cc/
        [Command("createRoles"), Summary("Creates a role in the guild")]
        public async Task CreateRoles() {
            List<string> currentGuildRoles = new List<string>();
            foreach (SocketRole guildRoles in ((SocketGuild)Context.Guild).Roles) {
                currentGuildRoles.Add(guildRoles.Name);
            }

            foreach (var role in Util.LoadAllRolesFromServer()) {
                if (!currentGuildRoles.Contains(role.Name)) {
                    var randColor = new Color(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));
                    await Context.Guild.CreateRoleAsync(role.Name, GuildPermissions.None, randColor);
                    await Logger.Log(new LogMessage(LogSeverity.Verbose, GetType().Name + ".createRoles", "Added role to server: " + role.Name));
                    await ReplyAsync($"Added role: {role.Name}\nHow to get: {role.Description}");
                }
            }
        }

        [Command("updateUsers"), Summary("edit's user info in database")]
        public async Task UpdateUsers() {
            StringBuilder sb = new StringBuilder();
            foreach (var user in (await Context.Guild.GetUsersAsync())) {
                if (!user.IsBot) {
                    var userInfo = ObjectUtils.GetUserInformation(user.Id);

                    foreach (SocketRole userRole in ((SocketGuildUser)user).Roles) {
                        if (!userInfo.roles.Contains(userRole.ToString()))
                            Util.UpdateArray("name", user.ToString(), "roles", userRole.ToString());
                    }
                    sb.Append($"{user} updated.\n");
                }
            }
            await ReplyAsync(sb.ToString());
        }

        [Command("giveRole"), Summary("Adds role to specified user"), RequireUserPermission(GuildPermission.Administrator)]
        public async Task GiveRole([Summary("The user to add role to")] SocketGuildUser user, [Summary("The role to add"), Remainder] string roleName) {

            var userInfo = user as SocketUser;
            var currentGuild = user.Guild as SocketGuild;
            var role = currentGuild.Roles.FirstOrDefault(x => x.Name.ToLower() == roleName.ToLower());
            Util.UpdateArray("name", user.ToString(), "roles", role.ToString());
            await Logger.Log(new LogMessage(LogSeverity.Info, GetType().Name + ".addRole", userInfo.ToString() + " added role " + role.ToString()));
            await (userInfo as IGuildUser).AddRoleAsync(role);
            await ReplyAsync($"{userInfo} now has {role}");
        }

        [Command("reorderRoles"), Summary("Reorders roles based on difficulty"), RequireUserPermission(GuildPermission.Administrator)]
        public async Task ReorderRoles() {
            var allRoles = Util.LoadAllRolesFromServer().OrderBy(x => x.Difficulty).Select(x => x.Name).ToList();
            var allGuildRoles = Context.Guild.Roles.OrderBy(y => allRoles.IndexOf(y.Name)).ToList();

            //reorder leaderbot roles to be at the top for hierarchy purposes
            var leaderbotRoles = allGuildRoles.Where(x => x.Name.Contains("leaderbot")).Reverse().ToList();
            allGuildRoles.RemoveAll(x => x.Name.Contains("leaderbot"));
            allGuildRoles.AddRange(leaderbotRoles);

            //sort the list based on the difficulty
            var sorting = allGuildRoles.Select((role, pos) => {
                var res = new ReorderRoleProperties(role.Id, pos);
                return res;
            });
            await Context.Guild.ReorderRolesAsync(sorting);
            await ReplyAsync($"Roles have been reordered");
        }

        [Command("makeRole"), Summary("Creates a new role in the server"), RequireUserPermission(GuildPermission.Administrator)]
        public async Task MakeRoles(string name, string description, int difficulty) {
			CreateObjectUtils.CreateRoleInDatabase(name, description, difficulty);
            await ReplyAsync($"Role has been inserted into the database");
            await CreateRoles();
        }

        [Command("rolesList"), Summary("prints role list"), RequireUserPermission(GuildPermission.Administrator)]
        public async Task PrintRolesList() {
            //delete command and previous role list
            var items = await Context.Channel.GetMessagesAsync(2).FlattenAsync();
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(items);

            var allRoles = Util.LoadAllRolesFromServer().OrderBy(x => x.Difficulty).Reverse();
            StringBuilder sb = new StringBuilder();
            foreach (var role in allRoles) {
                string s = $"**{role.Name}** | {role.Description} | {role.Difficulty}\n";
                if (sb.ToString().Length + s.Length > 2000) {
                    await ReplyAsync(sb.ToString());
                    sb.Clear();
                } else {
                    sb.Append(s);
                }
            }
            await ReplyAsync(sb.ToString());
        }

        [Command("updateUserFields"), Summary("Returns user experience")]
        public async Task UpdateUSersField(string field) {
            try {
                foreach (var user in await Context.Guild.GetUsersAsync()) {
                    if (!user.IsBot) {
                        Util.UpdateDocumentField(user.Id, field, new BsonArray { });
                    }
                }
                await ReplyAsync($"fields updated");
            } catch (Exception ex) {
                await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".updateUserFields", "Unexpected Exception", ex));
            }
        }

        [Command("givePoints"), Summary("give specificed user points")]
        public async Task GivePoints([Summary("The user to get point total from")] SocketGuildUser userName, int points) {
            var user = userName as SocketUser;
            DatabaseUtils.IncrementDocument(user.Id, "points", points);
            UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
            if (userInfo != null) {
                var currentPoints = userInfo.points;
                await ReplyAsync($"{user} has {currentPoints} points!");

            }
        }

        [Command("moveroles")]
        public async Task MoveRoles() {
            var rol = Util.LoadAllRolesFromServer();
            foreach (var r in rol) {
                DatabaseUtils.ChangeCollection("rolesNew");
                CreateObjectUtils.CreateRoleInDatabase(r.Name, r.Description, r.Difficulty);
            }
        }
    }
}