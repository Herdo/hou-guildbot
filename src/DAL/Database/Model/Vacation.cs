﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class Vacation
    {
        public int VacationID { get; set; }
        public int UserID { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Note { get; set; }

        public virtual User User { get; set; }
    }
}