﻿using System.IO;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IImageProvider
    {
        Stream CreateAocClassDistributionImage();

        Stream CreateAocPlayStyleDistributionImage();

        Stream CreateAocRaceDistributionImage();

        Stream CreateNewWorldClassDistributionImage();

        Stream CreateNewWorldProfessionDistributionImage();

        Task<Stream> CreateProfileImage(DiscordUserID userID,
                                        string avatarUrl);

        Stream LoadClassListImage();
    }
}