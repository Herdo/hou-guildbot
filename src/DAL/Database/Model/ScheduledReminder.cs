﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class ScheduledReminder
    {
        public ScheduledReminder()
        {
            ScheduledReminderMention = new HashSet<ScheduledReminderMention>();
        }

        public int ScheduledReminderID { get; set; }
        public string CronSchedule { get; set; }
        public decimal DiscordChannelID { get; set; }
        public string Text { get; set; }

        public virtual ICollection<ScheduledReminderMention> ScheduledReminderMention { get; set; }
    }
}