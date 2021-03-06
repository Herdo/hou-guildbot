﻿using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IUserInfoProvider
    {
        Task<string[]> GetLastSeenInfo();

        EmbedData WhoIs(DiscordUserID userID);

        EmbedData WhoIs(string username, string remainderContent);
    }
}