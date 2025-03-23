using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json;
using static CounterStrikeSharp.API.Core.Listeners;

namespace ConsoleChat
{
    public class ConsoleChat : BasePlugin
    {
        public override string ModuleName => "Console Chat";
        public override string ModuleVersion => "1.1";
        public override string ModuleAuthor => "Oylsister";

        public List<string> BlackList = new List<string> { "recharge", "recast", "cooldown", "cool" };

        public static Dictionary<string, ConsoleMessage> MessageList = [];
        public static string ConfigPath = Path.Combine(Application.RootDirectory, "configs/console_message/");
        public static string LastMap = string.Empty;

        public override void Load(bool hotReload)
        {
            AddCommandListener("say", OnSayTest, HookMode.Pre);

            if (!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);

            RegisterListener<OnMapStart>(OnMapStart);

            if(hotReload)
            {
                LoadConfig(Server.MapName);
                LastMap = Server.MapName;
            }
        }

        public override void Unload(bool hotReload)
        {
            RemoveCommandListener("say", OnSayTest, HookMode.Pre);
            RemoveListener<OnMapStart>(OnMapStart);
        }

        public void OnMapStart(string mapname)
        {
            // if we just end the map, we need to save the config first.
            if (LastMap != mapname)
                SaveConfig(LastMap);
                
            LoadConfig(mapname);
            LastMap = mapname;
        }

        private static void SaveConfig(string mapname)
        {
            var filePath = Path.Combine(ConfigPath, $"{mapname}.jsonc");
            File.WriteAllText(filePath, JsonConvert.SerializeObject(MessageList, Formatting.Indented));
        }

        private static void LoadConfig(string mapname)
        {
            MessageList.Clear();

            var filePath = Path.Combine(ConfigPath, $"{mapname}.jsonc");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(MessageList, Formatting.Indented));
            }

            else
            {
                MessageList = JsonConvert.DeserializeObject<Dictionary<string, ConsoleMessage>>(File.ReadAllText(filePath)) ?? [];
            }
        }

        public HookResult OnSayTest(CBaseEntity? client, CommandInfo info)
        {
            if (client == null)
            {
                var message = info.ArgString.ToLower();

                if (MessageList.TryGetValue(info.ArgString, out ConsoleMessage? msg))
                {
                    if (msg != null && !string.IsNullOrEmpty(msg.English))
                    {
                        if (!IsCountable(msg.English.ToLower()))
                        {
                            Server.PrintToChatAll($" {ChatColors.Red}CONSOLE:{ChatColors.Green} {msg.English}");
                            return HookResult.Handled;
                        }
                        else
                        {
                            var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;

                            var timeleft = gameRules.RoundTime - (Server.CurrentTime - gameRules.RoundStartTime);

                            var Countdown = GetCountNumber(msg.English);

                            if ((Countdown >= 5) && (timeleft > Countdown))
                            {
                                int triggerTime = (int)Math.Ceiling(timeleft - Countdown);

                                if ((int)timeleft - 0.5f == (int)timeleft)
                                {
                                    triggerTime++;
                                }

                                var min = triggerTime / 60;
                                var secs = triggerTime % 60;

                                Server.PrintToChatAll($" {ChatColors.Red}CONSOLE:{ChatColors.Green} {msg.English} {ChatColors.Orange} - @{min}:{secs:D2}");
                                return HookResult.Handled;
                            }
                            else
                            {
                                Server.PrintToChatAll($" {ChatColors.Red}CONSOLE:{ChatColors.Green} {msg.English}");
                                return HookResult.Handled;
                            }
                        }
                    }
                }

                // if so far they reach here, it means the message is not in the list.
                // whatever happened we will need to add message to list first.
                MessageList.TryAdd(info.ArgString, new ConsoleMessage { English = info.ArgString });

                if (!IsCountable(message))
                {
                    Server.PrintToChatAll($" {ChatColors.Red}CONSOLE:{ChatColors.Green} {info.ArgString}");
                    return HookResult.Handled;
                }
                else
                {
                    //Server.PrintToChatAll("Countable message");
                    var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;

                    var timeleft = gameRules.RoundTime - (Server.CurrentTime - gameRules.RoundStartTime);

                    var Countdown = GetCountNumber(info.ArgString);

                    if ((Countdown >= 5) && (timeleft > Countdown))
                    {
                        int triggerTime = (int)Math.Ceiling(timeleft - Countdown);

                        if ((int)timeleft - 0.5f == (int)timeleft)
                        {
                            triggerTime++;
                        }

                        var min = triggerTime / 60;
                        var secs = triggerTime % 60;

                        Server.PrintToChatAll($" {ChatColors.Red}CONSOLE:{ChatColors.Green} {info.ArgString} {ChatColors.Orange} - @{min}:{secs:D2}");
                        return HookResult.Handled;
                    }
                    else
                    {
                        Server.PrintToChatAll($" {ChatColors.Red}CONSOLE:{ChatColors.Green} {info.ArgString}");
                        return HookResult.Handled;
                    }
                }
            }

            return HookResult.Continue;
        }

        bool CheckString(string str)
        {
            foreach (var blackword in BlackList)
            {
                if (str.Contains(blackword, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public bool IsCountable(string message)
        {
            if (CheckString(message))
                return false;

            string pattern = @"\b(\d+)(\s*)(s|sec|secs|second|seconds)\b";

            return Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase);
        }

        public int GetCountNumber(string message)
        {
            if (!IsCountable(message))
                return 0;

            string pattern = @"\b(\d+)(\s*)(s|sec|secs|second|seconds)\b";

            var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
                return int.Parse(match.Groups[1].Value);

            return 0;
        }
    }

    public class ConsoleMessage
    {
        [JsonProperty(PropertyName = "en")]
        public string English { get; set; } = string.Empty;
    }
}
