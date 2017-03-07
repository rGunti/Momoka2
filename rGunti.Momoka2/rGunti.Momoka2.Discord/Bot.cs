﻿using Discord;
using Discord.Commands;
using NLog;
using rGunti.Momoka2.Discord.InfoBot;
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

        public Bot(string apiKey) {
            client = new DiscordClient();
            client.Log.Message += Log_Message;
            this.apiKey = apiKey;
            InitCommands();
        }

        private void InitCommands() {
            client.MessageReceived += Client_MessageReceived;
            client.UsingCommands(x => {
                x.PrefixChar = '!';
                x.HelpMode = HelpMode.Private;
            });
            client.GetService<CommandService>().CreateCommand("ping")
                .Alias("test", "hi")
                .Description("Answers with \"Pong\"!")
                .Do(async e => {
                    await e.Channel.SendMessage($"{e.User.Mention}, Pong!");
                })
            ;
            client.GetService<CommandService>().CreateCommand("clear")
                .Description("Clears the current text channel")
                .Do(async e => {
                    log.Trace($"Message Listing from Channel {e.Channel.Name} on {e.Server.Name}:");
                    Message[] messages = await e.Channel.DownloadMessages(100);

                    foreach (var message in messages) {
                        log.Trace($"{message.User.Name} wrote at {message.Timestamp}: {message.Text}");
                    }

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
        }

        private void Client_MessageReceived(object sender, MessageEventArgs e) {
            if (!e.Message.IsAuthor) {
                log.Trace($"{e.Message.User.Name} from {e.Server.Name} said: {e.Message.Text}");
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
