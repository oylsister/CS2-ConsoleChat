﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.RegularExpressions;

namespace ConsoleChat
{
    public class ConsoleChat : BasePlugin
    {
        public override string ModuleName => "Console Chat";
        public override string ModuleVersion => "1.0";
        public override string ModuleAuthor => "Oylsister";

        public List<string> BlackList = new List<string> { "recharge", "recast", "cooldown", "cool" };

        int Countdown = 0;

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

                    if((Countdown > 5) && (timeleft > Countdown))
                    {
                        int triggerTime = (int)Math.Ceiling(timeleft - Countdown);

                        if((int)timeleft - 0.5f == (int)timeleft)
                        {
                            triggerTime++;
                        }

                        var min = triggerTime / 60;
                        var secs = triggerTime % 60;

                        Server.PrintToChatAll($" {ChatColors.Red}CONSOLE:{ChatColors.Green} {info.ArgString} {ChatColors.Orange}[ {min}:{secs} ]");
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

            string[] exploded = message.Split(" ");
            int number;

            for(int i = 0; i < exploded.Length; i ++)
            {
                var isnumber = int.TryParse(exploded[i], out number);

                if (isnumber)
                {
                    Countdown = number;
                    var array = exploded[i + 1].ToCharArray();

                    if (array[0] == 's')
                        return true;
                }
            }

            return false;
        }
    }
}
