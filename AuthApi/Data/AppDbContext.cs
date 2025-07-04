﻿using Microsoft.EntityFrameworkCore;
using AuthApi.Models;

namespace AuthApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Friendships> Friendships { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<Messages> Messagess { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Friendships>().ToTable("friendships");
            modelBuilder.Entity<Chat>().ToTable("chats");
            modelBuilder.Entity<ChatParticipant>().ToTable("chatparticipants");
            modelBuilder.Entity<Messages>().ToTable("messages");

        }
    }
}
