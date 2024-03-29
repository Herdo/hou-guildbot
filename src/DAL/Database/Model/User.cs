﻿using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class User
    {
        public User()
        {
            Game = new HashSet<Game>();
            GameRole = new HashSet<GameRole>();
            Vacation = new HashSet<Vacation>();
        }

        public int UserId { get; set; }
        public decimal DiscordUserId { get; set; }

        public virtual UserBirthday? UserBirthday { get; set; } = null!;
        public virtual UserInfo? UserInfo { get; set; } = null!;
        public virtual ICollection<Game> Game { get; set; }
        public virtual ICollection<GameRole> GameRole { get; set; }
        public virtual ICollection<Vacation> Vacation { get; set; }
    }
}
