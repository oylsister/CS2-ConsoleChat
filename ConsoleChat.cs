using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace ConsoleChat
{
    public class ConsoleChat : BasePlugin
    {
        public override string ModuleName => "Console Chat";
        public override string ModuleVersion => "1.1";
        public override string ModuleAuthor => "Oylsister";

        public List<string> BlackList = new List<string> { "recharge", "recast", "cooldown", "cool" };

        public override void Load(bool hotReload)
        {
            AddCommandListener("say", OnSayTest, HookMode.Pre);
        }

        public HookResult OnSayTest(CBaseEntity? client, CommandInfo info)
        {
            if (client == null)
            {
                if (!IsCountable(info.ArgString))
                {
                    Server.PrintToChatAll($" {ChatColors.Red}CONSOLE:{ChatColors.Green} {info.ArgString}");
                    return HookResult.Handled;
                }
                else
                {
                    var gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;

                    var timeleft = gameRules.RoundTime - (Server.CurrentTime - gameRules.RoundStartTime);

                    var Countdown = GetCountNumber(info.ArgString);

                    if ((Countdown > 5) && (timeleft > Countdown))
                    {
                        int triggerTime = (int)Math.Ceiling(timeleft - Countdown);

                        if ((int)timeleft - 0.5f == (int)timeleft)
                        {
                            triggerTime++;
                        }

                        var min = triggerTime / 60;
                        var secs = triggerTime % 60;

                        Server.PrintToChatAll($" {ChatColors.Red}CONSOLE:{ChatColors.Green} {info.ArgString} {ChatColors.Orange}[ {min}:{secs:D2} ]");
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

            string pattern = @"\b(\d+)(\s*)(s|sec|second|seconds)\b";

            return Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase);
        }

        public int GetCountNumber(string message)
        {
            if (!IsCountable(message))
                return 0;

            string pattern = @"\b(\d+)(\s*)(s|sec|second|seconds)\b";

            var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
                return int.Parse(match.Groups[1].Value);

            return 0;
        }
    }
}
