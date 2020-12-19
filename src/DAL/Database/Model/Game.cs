﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

#nullable disable

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class Game
    {
        public Game()
        {
            GameRole = new HashSet<GameRole>();
        }

        public short GameID { get; set; }
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public int ModifiedByUserID { get; set; }
        public DateTime ModifiedAtTimestamp { get; set; }
        public decimal? PrimaryGameDiscordRoleID { get; set; }
        public bool IncludeInGuildMembersStatistic { get; set; }
        public bool IncludeInGamesMenu { get; set; }
        public string GameInterestEmojiName { get; set; }
        public decimal? GameInterestRoleId { get; set; }

        public virtual User ModifiedByUser { get; set; }
        public virtual ICollection<GameRole> GameRole { get; set; }
    }
}