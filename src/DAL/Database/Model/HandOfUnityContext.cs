﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class HandOfUnityContext : DbContext
    {
        public HandOfUnityContext()
        {
        }

        public HandOfUnityContext(DbContextOptions<HandOfUnityContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DesiredTimeZone> DesiredTimeZone { get; set; }
        public virtual DbSet<DiscordMapping> DiscordMapping { get; set; }
        public virtual DbSet<Game> Game { get; set; }
        public virtual DbSet<GameRole> GameRole { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<PersonalReminder> PersonalReminder { get; set; }
        public virtual DbSet<SpamProtectedChannel> SpamProtectedChannel { get; set; }
        public virtual DbSet<UnitsEndpoint> UnitsEndpoint { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<Vacation> Vacation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Scaffolding:ConnectionString", "Data Source=(local);Initial Catalog=Database;Integrated Security=true");

            modelBuilder.Entity<DesiredTimeZone>(entity =>
            {
                entity.HasKey(e => e.DesiredTimeZoneKey);

                entity.ToTable("DesiredTimeZone", "config");

                entity.Property(e => e.DesiredTimeZoneKey)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.InvariantDisplayName)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DiscordMapping>(entity =>
            {
                entity.HasKey(e => e.DiscordMappingKey);

                entity.ToTable("DiscordMapping", "config");

                entity.Property(e => e.DiscordMappingKey)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.DiscordID).HasColumnType("decimal(20, 0)");
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Game", "config");

                entity.HasIndex(e => e.LongName, "UQ_Game_LongName")
                    .IsUnique();

                entity.HasIndex(e => e.ShortName, "UQ_Game_ShortName")
                    .IsUnique();

                entity.Property(e => e.GameInterestEmojiName)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.GameInterestRoleId).HasColumnType("decimal(20, 0)");

                entity.Property(e => e.LongName)
                    .IsRequired()
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryGameDiscordRoleID).HasColumnType("decimal(20, 0)");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.HasOne(d => d.ModifiedByUser)
                    .WithMany(p => p.Game)
                    .HasForeignKey(d => d.ModifiedByUserID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<GameRole>(entity =>
            {
                entity.ToTable("GameRole", "config");

                entity.HasIndex(e => e.DiscordRoleID, "UQ_GameRole_DiscordRoleID")
                    .IsUnique();

                entity.HasIndex(e => new { e.GameID, e.RoleName }, "UQ_GameRole_GameID_RoleName")
                    .IsUnique();

                entity.Property(e => e.DiscordRoleID).HasColumnType("decimal(20, 0)");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GameRole)
                    .HasForeignKey(d => d.GameID)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifiedByUser)
                    .WithMany(p => p.GameRole)
                    .HasForeignKey(d => d.ModifiedByUserID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message", "config");

                entity.HasIndex(e => e.Name, "IDX_Message_Name_Inc_Content")
                    .IsUnique();

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PersonalReminder>(entity =>
            {
                entity.ToTable("PersonalReminder", "config");

                entity.Property(e => e.CronSchedule)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.DiscordChannelID).HasColumnType("decimal(20, 0)");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.UserToRemind).HasColumnType("decimal(20, 0)");
            });

            modelBuilder.Entity<SpamProtectedChannel>(entity =>
            {
                entity.ToTable("SpamProtectedChannel", "config");

                entity.Property(e => e.SpamProtectedChannelID).HasColumnType("decimal(20, 0)");
            });

            modelBuilder.Entity<UnitsEndpoint>(entity =>
            {
                entity.ToTable("UnitsEndpoint", "config");

                entity.HasIndex(e => e.BaseAddress, "UQ_BaseAddress")
                    .IsUnique();

                entity.Property(e => e.BaseAddress)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Secret)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User", "hou");

                entity.HasIndex(e => e.DiscordUserID, "IDX_User_DiscordUserID_Inc_UserID")
                    .IsUnique();

                entity.Property(e => e.DiscordUserID).HasColumnType("decimal(20, 0)");
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasKey(e => e.UserID)
                    .HasName("PK_UserInfo_UserID");

                entity.ToTable("UserInfo", "hou");

                entity.Property(e => e.UserID).ValueGeneratedNever();

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserInfo)
                    .HasForeignKey<UserInfo>(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserInfo_User");
            });

            modelBuilder.Entity<Vacation>(entity =>
            {
                entity.ToTable("Vacation", "hou");

                entity.Property(e => e.End).HasColumnType("date");

                entity.Property(e => e.Note)
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Start).HasColumnType("date");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Vacation)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}