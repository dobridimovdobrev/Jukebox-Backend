using Jukebox_Backend.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jukebox_Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Artist> Artists { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<UserQuizHistory> UserQuizHistories { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }

        // pivot tables
        public DbSet<PlaylistArtist> PlaylistArtists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }

        // soft delete and relationships management
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Playlist>()
                .HasQueryFilter(p => !p.IsDeleted);

            builder.Entity<Playlist>()
                .HasMany(p => p.PlaylistArtists)
                .WithOne(pa => pa.Playlist)
                .HasForeignKey(pa => pa.PlaylistId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Playlist>()
                .HasMany(p => p.PlaylistSongs)
                .WithOne(ps => ps.Playlist)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Artist>()
                .HasMany(a => a.Songs)
                .WithOne(s => s.Artist)
                .HasForeignKey(s => s.ArtistId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Artist>()
                .HasMany(a => a.Albums)
                .WithOne(al => al.Artist)
                .HasForeignKey(al => al.ArtistId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Album>()
                .HasMany(al => al.Songs)
                .WithOne(s => s.Album)
                .HasForeignKey(s => s.AlbumId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TicketHistory>()
                .HasOne(th => th.Ticket)
                .WithMany()
                .HasForeignKey(th => th.TicketId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TicketHistory>()
                .HasOne(th => th.User)
                .WithMany()
                .HasForeignKey(th => th.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
