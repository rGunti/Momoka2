using Discord;
using Discord.Commands;
using NLog;
using rGunti.Momoka2.Discord.InfoBot;
using rGunti.Momoka2.Discord.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rGunti.Momoka2.Discord {
    class Bot {
        private Logger log = LogManager.GetCurrentClassLogger();
        private DiscordClient client;
        private string apiKey;
        private Random random;

        public Bot(string apiKey) {
            client = new DiscordClient();
            client.Log.Message += Log_Message;
            this.apiKey = apiKey;
            InitCommands();
        }

        private void InitCommands() {
            random = new Random(DateTime.Now.Millisecond);

            client.MessageReceived += Client_MessageReceived;
            client.UsingCommands(x => {
                x.PrefixChar = '!';
                x.HelpMode = HelpMode.Private;
            });
            client.GetService<CommandService>().CreateCommand("ping")
                .Alias("test", "hi")
                .Description("Answers with \"Pong\"!")
                .Do(async e => {
                    if (!ServerPermissionManager.GetPermissionManager(e.Server).HasPermission(e.Server, e.User)) {
                        await e.Channel.SendMessage($":x: {e.User.Mention}: You don't have permission to do that.");
                        return;
                    }
                    await e.Channel.SendMessage($"{e.User.Mention}, Pong!");
                })
            ;
            client.GetService<CommandService>().CreateCommand("clear")
                .Description("Clears the current text channel")
                .Do(async e => {
                    log.Trace($"Message Listing from Channel {e.Channel.Name} on {e.Server.Name}:");
                    Message[] messages = await e.Channel.DownloadMessages(100);

                    //foreach (var message in messages) {
                    //    log.Trace($"{message.User.Name} wrote at {message.Timestamp}: {message.Text}");
                    //}

                    log.Info($"Deleting {messages.Length} messages in Channel {e.Channel.Name} on {e.Server.Name}");
                    await e.Channel.DeleteMessages(messages);
                })
            ;
            client.GetService<CommandService>().CreateCommand("dictionary")
                .Description("Displays information about a specific topic from the server dictionary")
                .Alias("dict", "d")
                .Parameter("Term", ParameterType.Required)
                .Parameter("Content", ParameterType.Optional)
                .Do(async e => {
                    await e.Channel.SendIsTyping();

                    string term = e.GetArg("Term");
                    string content = e.GetArg("Content");

                    InfoBotContentManager cm = new InfoBotContentManager(e.Server);
                    if (string.IsNullOrWhiteSpace(content)) {
                        string termContent = cm.GetContent(term);
                        if (termContent == null) {
                            await e.Channel.SendMessage($":x: {e.User.Mention}: Sorry, there is currently no information available for **{term}**.");
                        } else {
                            await e.Channel.SendMessage($"**About {term}**\n{termContent}");
                        }
                    } else {
                        cm.SetContent(term, content);
                        await e.Channel.SendMessage($":white_check_mark: {e.User.Mention} has stored new info about **{term}**.");
                    }
                })
            ;
            client.GetService<CommandService>().CreateCommand("dice")
                .Description("Rolls a six-sided dice and returns its result.")
                .Do(e => {
                    int randomNumber = random.Next(1, 7);
                    e.Channel.SendMessage($"{e.User.Mention}, your dice showed you a **{randomNumber}**.");
                })
            ;
            client.GetService<CommandService>().CreateCommand("ndice")
                .Description("Rolls a n-sided dice and returns its result.")
                .Parameter("Sides", ParameterType.Required)
                .Do(e => {
                    string sidesString = e.GetArg("Sides");
                    int sides;

                    if (!int.TryParse(sidesString, out sides)) {
                        e.Channel.SendMessage($":x: {e.User.Mention}: You have to give me a **number** of sides for the n-sided dice to work.");
                        return;
                    }

                    int randomNumber = random.Next(1, sides + 1);
                    e.Channel.SendMessage($"{e.User.Mention}, your {sides}-sided dice showed you a **{randomNumber}**.");
                })
            ;


            client.GetService<CommandService>().CreateCommand("perm")
                //.Description("")
                .Parameter("Role ID / Name", ParameterType.Required)
                .Parameter("Allow", ParameterType.Required)
                .Do(e => {
                    var permManager = ServerPermissionManager.GetPermissionManager(e.Server);
                    if (!permManager.HasPermission(e.Server, e.User)) {
                        e.Channel.SendMessage($":x: {e.User.Mention}: You don't have permission to do that.");
                        return;
                    }

                    string roleString = e.GetArg("Role ID / Name");
                    bool allowCommandUsage = "1" == e.GetArg("Allow");
                    ulong roleID;

                    Role role;
                    if (ulong.TryParse(roleString, out roleID)) {
                        role = e.Server.GetRole(roleID);
                    } else {
                        IEnumerable<Role> roles = e.Server.FindRoles(roleString);
                        if (roles == null || roles.Count() == 0) {
                            e.Channel.SendMessage($":x: {e.User.Mention}: Can't find a role with the name \"{roleString}\".");
                            return;
                        }
                        role = roles.First();
                    }

                    if (role == null) {
                        e.Channel.SendMessage($":x: {e.User.Mention}: Given Role ID is invalid!");
                    } else {
                        permManager.SetHasPermission(role.Id, allowCommandUsage);
                        e.Channel.SendMessage($":white_check_mark: {e.User.Mention}: Permission for Role @{role.Name} ({role.Id}) is set to " +
                            $"{(allowCommandUsage ? ":white_check_mark:" : ":no_entry:")}.");
                    }
                })
            ;
            client.GetService<CommandService>().CreateCommand("listperm")
                .Description("Lists all set permissions on this server.")
                .Do(e => {
                    var permManager = ServerPermissionManager.GetPermissionManager(e.Server);
                    if (!permManager.HasPermission(e.Server, e.User)) {
                        e.Channel.SendMessage($":x: {e.User.Mention}: You don't have permission to do that.");
                        return;
                    }

                    string responseString = $"**Permission List for {e.Server.Name}**";
                    foreach (KeyValuePair<ulong, bool> kvp in permManager.Dictionary) {
                        Role role = e.Server.GetRole(kvp.Key);
                        responseString += $"\n{(kvp.Value ? ":white_check_mark:" : ":no_entry:")} [{kvp.Key}] {role?.Name ?? "<does not exist>"}";
                    }

                    e.Channel.SendMessage(responseString);
                })
            ;
#if DEBUG
                    client.GetService<CommandService>().CreateCommand("debugrole")
                .Do(e => {
                    log.Trace($"Roles of Server [{e.Server?.Id}] {e.Server?.Name}");
                    foreach (Role role in e.Server.Roles) {
                        log.Trace($" - [{role.Id}] {role.Name} ({role.Color})");
                    }
                })
            ;
#endif
        }

        private void Client_MessageReceived(object sender, MessageEventArgs e) {
            if (!e.Message.IsAuthor) {
                log.Trace($"{e.Message.User.Name} from {e.Server?.Name} said: {e.Message.Text}");
            }
        }

        private void Log_Message(object sender, LogMessageEventArgs e) {
            log.Trace($"DiscordClient reported: [{e.Severity}] {e.Message}");
        }

        public void Run() {
            client.ExecuteAndWait(async () => {
                await client.Connect(apiKey, TokenType.Bot);
            });
        }
    }
}
