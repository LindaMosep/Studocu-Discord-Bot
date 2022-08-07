using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class UserWithChannel
{
    public SocketUser user;
    public SocketTextChannel channel;
    public float second;
    public SocketMessage message;
    public bool isProcessFinished;

    public UserWithChannel(SocketUser user, SocketTextChannel channel, float second, SocketMessage message, bool isProcessFinished)
    {
        this.user = user;
        this.channel = channel;
        this.second = second;
        this.message = message;
        this.isProcessFinished=isProcessFinished;
    }

}


namespace Studocu_Discord_Bot
{
    internal static class Program
    {
        #region Values
        public static DiscordSocketClient _client;
        public static List<UserWithChannel> users = new List<UserWithChannel>();
        public static EmbedFooterBuilder SignFooter;
        public static int FileCount;
        
        #endregion


        public static CookieContainer cookie = new CookieContainer();

        public static void Main(string[] Args)
        {

            MainAsync().Wait();


        }

        public static async Task SlowMode(SocketGuildUser user, SocketTextChannel channel, SocketMessage e, bool isProcessFinished)
        {

            var uch = new UserWithChannel(user, channel, 300, e, false);
            users.Add(uch);

            while (!users.Find(m => m.user == uch.user && m.channel == uch.channel).isProcessFinished)
            {

                await Task.Delay(1);
            }


            for (int i = 300; i > 1; i--)
            {

                await Task.Delay(1);
                users.Find(m => m.user == uch.user && m.channel == uch.channel).second = i;


            }

            SlowModeFinished(users.Find(m => m.user == uch.user && m.channel == uch.channel).message);
            users.Remove(uch);

        }

        public static async Task MessageError(SocketMessage e)
        {
            if ((e.Channel as IGuildChannel).Guild.GetRole(922634093471092737) != null)
            {
                var embed = new EmbedBuilder().WithTitle("```It's seems there is an error!```").WithColor(new Color(255, 0, 0)).WithDescription(e.Author.Mention+ $" **Please report to {(e.Channel as IGuildChannel).Guild.GetRole(922634093471092737).Mention}**")
                        .WithThumbnailUrl("https://images-ext-2.discordapp.net/external/wOqzpmK--O8wLGpd1AHc4Bv2lBFebnihQSV0JpkcD7k/https/cdn.dribbble.com/users/2182116/screenshots/13933572/media/cc32730b1eb3a657a48a6ceacefc72fb.gif?width=745&height=559").WithFooter(SignFooter);
                await e.Channel.SendMessageAsync("", false, embed.Build(), null, null, new MessageReference(e.Id));
            }
            else
            {
                var embed = new EmbedBuilder().WithTitle("```It's seems there is an error!```").WithColor(new Color(255, 0, 0)).WithDescription(e.Author.Mention+ $" **Please report to Admin!**")
                       .WithThumbnailUrl("https://images-ext-2.discordapp.net/external/wOqzpmK--O8wLGpd1AHc4Bv2lBFebnihQSV0JpkcD7k/https/cdn.dribbble.com/users/2182116/screenshots/13933572/media/cc32730b1eb3a657a48a6ceacefc72fb.gif?width=745&height=559").WithFooter(SignFooter);
                await e.Channel.SendMessageAsync("", false, embed.Build(), null, null, new MessageReference(e.Id));
            }

          
        }

        public static async Task ProcessStarted(SocketMessage e)
        {
            var embed = new EmbedBuilder().WithTitle("Process started succesfully!").WithColor(new Color(255, 255, 0)).WithDescription(e.Author.Mention+ $" Please wait until process completed.")
                          .WithThumbnailUrl("https://media.discordapp.net/attachments/917813714923683860/926237778168148028/loader-backinout-1.gif").WithFooter(SignFooter);
            await e.Channel.SendMessageAsync("", false, embed.Build(), null, null, new MessageReference(e.Id));
        }

        public static async Task ProcessCompleted(SocketMessage e)
        {
            var embed = new EmbedBuilder().WithTitle("Process completed succesfully!").WithColor(new Color(0, 255, 0)).WithDescription(e.Author.Mention+ $" Please check your DM!.")
                          .WithThumbnailUrl("https://media.discordapp.net/attachments/917813714923683860/925800837639446590/ezgif-2-a888610124.gif").WithFooter(SignFooter);
            await e.Channel.SendMessageAsync("", false, embed.Build(), null, null, new MessageReference(e.Id));
        }

        public static async Task SlowModeMessage(SocketMessage e, float second, SocketGuildUser user, SocketTextChannel channel)
        {
            var embed = new EmbedBuilder().WithTitle("Your in cooldown!").WithColor(new Color(255, 255, 0)).WithDescription(e.Author.Mention+ $" Your currently in cooldown, wait a **{second / 100:F2}** second.")
                          .WithThumbnailUrl("https://images-ext-1.discordapp.net/external/8pBdRwZ4cXwRdZPAoBhesOdgZa2EQMSznqHFGYt7iyg/https/cdn.dribbble.com/users/2015153/screenshots/6592242/progess-bar2.gif?width=745&height=559").WithFooter(SignFooter);
            users.Find(m => m.user == user && m.channel == channel).message = e;
            await e.Channel.SendMessageAsync("", false, embed.Build(), null, null, new MessageReference(e.Id));
        }

        public static async Task SlowModeFinished(SocketMessage e)
        {
            var embed = new EmbedBuilder().WithTitle("Your cooldown finished!").WithColor(new Color(0, 0, 255)).WithDescription(e.Author.Mention+ $" You can use me now.")
                          .WithThumbnailUrl("https://images-ext-2.discordapp.net/external/-xCsuG6EsrPfO15glJR33j2U57ztvS432HaW0D0oRV0/https/cdn.dribbble.com/users/1162077/screenshots/5427775/media/612968fb2a4690f4959deb23a00eb2d0.gif?width=745&height=559").WithFooter(SignFooter);
            try
            {
                await e.Author.SendMessageAsync("", false, embed.Build(), null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            
        }

        
       
        public static void GetCookies()
        {

            cookie = new CookieContainer();
            var msgs = _client.GetGuild(922634093445918751).GetTextChannel(926900351884476436).GetMessagesAsync(1).ToListAsync();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(msgs.Result.ToList()[0].ToList()[0].Attachments.ToList()[0].Url);


            WebClient myWebClient = new WebClient();



            // Download the resource and load the bytes into a buffer.
            byte[] buffer = myWebClient.DownloadData(msgs.Result.ToList()[0].ToList()[0].Attachments.ToList()[0].Url);

            // Encode the buffer into UTF-8
            string download = Encoding.UTF8.GetString(buffer);
            var CookiesString = download;

            var CookieLines = Regex.Split(CookiesString, "},");
            foreach (var text in CookieLines)
            {
                var name = text.Substring(text.IndexOf("\"name\": \"") + "\"name\": \"".Length);
                name = name.Remove(name.IndexOf("\""));


                var value = text.Substring(text.IndexOf("\"value\": \"") + "\"value\": \"".Length);
                value = value.Remove(value.IndexOf("\""));

                var domain = text.Substring(text.IndexOf("\"domain\": \"") + "\"domain\": \"".Length);
                domain = domain.Remove(domain.IndexOf("\""));


                cookie.Add(new Cookie(name, value, "/", domain));
            }
        }

        public static async Task MainAsync()
        {


            Console.WriteLine(0);
            SignFooter = new EmbedFooterBuilder().WithText("Powered by Meowdemia!").WithIconUrl("https://media.discordapp.net/attachments/917813714923683860/923807194640687134/Grey_Cute_Illustrated_Cat_and_Fish_Circle_Laptop_Sticker_1.png?width=559&height=559");

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildMessageReactions |
                GatewayIntents.GuildMessages |
                GatewayIntents.GuildVoiceStates | GatewayIntents.All

            });




            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, "");
            await _client.StartAsync();


            _client.MessageReceived += MessageStart;


            await Task.Delay(-1);




        }

        public static string SubString2(this string str, string search)
        {
            string ret = str;
            if (str.IndexOf(search) != -1)
            {
                ret = str.Substring(str.IndexOf(search) + search.Length);
            }

            return ret;
        }

        public static string Remove2(this string str, string search)
        {
            string ret = str;


            if (str.IndexOf(search) != -1)
            {
                ret = str.Remove(str.IndexOf(search) + search.Length);
                Console.WriteLine("Removed");
            }

            return ret;
        }

        public static Task MessageStart(SocketMessage e)
        {
            if (FileCount == 1000)
            {
                FileCount = 0;
                MessageRecieve(e);
            }
            else
            {
                MessageRecieve(e);
            }

            return Task.CompletedTask;
        }



        public static async Task MessageRecieve(SocketMessage e)
        {


            string url = "";
            var chnl = (e.Channel as ITextChannel);
            if (chnl != null)
            {
                if (chnl.Guild != null)
                {
                    var guild = chnl.Guild;
                    if (!users.Exists(m => m.user == e.Author as SocketGuildUser && m.channel == e.Channel as SocketTextChannel))
                    {
                        if (e.Content.StartsWith("https://www.studocu.com/"))
                        {

                            List<string> linksinmessage = new List<string>();
                            foreach (Match item in Regex.Matches(e.Content, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                            {
                                linksinmessage.Add(item.Value);
                            }

                            if (linksinmessage.Count > 0)
                            {
                                try
                                {
                                    if (!linksinmessage[0].EndsWith("/"))
                                    {
                                        linksinmessage[0] += "/";

                                    }

                                    linksinmessage[0] += "download";
                                    await ProcessStarted(e);
                                    if (!(e.Author as IGuildUser).RoleIds.ToList().Contains(925007529572962405))
                                    {
                                        SlowMode(e.Author as SocketGuildUser, e.Channel as SocketTextChannel, e, false);
                                    }
                                    GetCookies();
                                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(linksinmessage[0]);
                                    request.CookieContainer = cookie;
                                    request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";


                                    WebResponse wbb = request.GetResponse();






                                    using (Stream responsestream = wbb.GetResponseStream())
                                    {


                                        int count = FileCount +1;
                                        FileCount++;
                                        File.WriteAllBytes(Environment.CurrentDirectory +$@"\Studocu({count}).pdf", ReadFully(responsestream));


                                        while (!File.Exists(Environment.CurrentDirectory  +$@"\Studocu({count}).pdf"))
                                        {
                                            int m = 0;
                                        }


                                        FileInfo bla = new FileInfo(Environment.CurrentDirectory +$@"\Studocu({count}).pdf");

                                        if (bla.Length == 0)
                                        {
                                            MessageError(e);
                                            File.Delete(Environment.CurrentDirectory +$@"\Studocu({count}).pdf");
                                            users.Find(m => m.user.Id == e.Author.Id && m.channel.Id == e.Channel.Id).isProcessFinished = true;
                                        }
                                        else
                                        {

                                            var msg = await _client.GetUser(411997699647340545).SendFileAsync(Environment.CurrentDirectory + $@"\Studocu({count}).pdf");

                                            string urlOfFile = "";
                                            foreach (var m in msg.Attachments)
                                            {
                                                urlOfFile = m.Url;
                                            }
                                            File.Delete(Environment.CurrentDirectory  +$@"\Studocu({count}).pdf");

                                            EmbedBuilder TakeThis = new EmbedBuilder().WithTitle("Here it is, best postman in the whole World!").WithColor(Color.Green).AddField("Could you open the door please? I got some documentation for you. Study well!", $"[Download!]({urlOfFile})")
                                                                                                                            .WithThumbnailUrl("https://images-ext-1.discordapp.net/external/LUCi0IWmG8Jd-2yZlGW2Z1IHes2_A5KOuOfnV1KecwQ/https/cdn.dribbble.com/users/1616371/screenshots/6042217/media/844e55475e98ada8d64e1166e4d5a1b1.gif").WithFooter(SignFooter);
                                            await e.Author.SendMessageAsync(null, false, TakeThis.Build());
                                            ProcessCompleted(e);
                                            users.Find(m => m.user.Id == e.Author.Id && m.channel.Id == e.Channel.Id).isProcessFinished = true;
                                        }



                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    MessageError(e);
                                    users.Find(m => m.user.Id == e.Author.Id && m.channel.Id == e.Channel.Id).isProcessFinished = true;
                                }



                            }
                            else
                            {
                                await e.Channel.SendMessageAsync("Please send a valid URL!", false, null, null, null, new MessageReference(e.Id));
                            }



                        }
                    }
                    else
                    {
                        if (users.Find(m => m.user == e.Author as SocketGuildUser && m.channel == e.Channel as SocketTextChannel).isProcessFinished)
                        {
                            await SlowModeMessage(e, users.Find(m => m.user == e.Author as SocketGuildUser && m.channel == e.Channel as SocketTextChannel).second, e.Author as SocketGuildUser, e.Channel as SocketTextChannel);
                        }

                    }
                }
            }









        }
        public static void Copy(Stream input, Stream output)
        {
            byte[] buffer = new byte[50000000];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream tempStream = new MemoryStream())
            {
                Copy(input, tempStream);
                if (tempStream.Length == tempStream.GetBuffer().Length)
                {
                    return tempStream.GetBuffer();
                }
                return tempStream.ToArray();
            }
        }
        static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }


    }




}
