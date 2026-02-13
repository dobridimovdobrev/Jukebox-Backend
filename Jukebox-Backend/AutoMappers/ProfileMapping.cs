using AutoMapper;
using Jukebox_Backend.Models.Dto.CombinedApiData;
using Jukebox_Backend.Models.Dto.Responses;
using Jukebox_Backend.Models.Entities;

namespace Jukebox_Backend.Automapers
{
    public class ProfileMapping : Profile
    {
        public ProfileMapping()
        {
            CreateMap<CombinedArtistData, Artist>()
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo ?? "default.jpg"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.ArtistId, opt => opt.Ignore());

            CreateMap<CombinedSongData, Song>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.SongId, opt => opt.Ignore());

            CreateMap<Artist, ArtistResponse>()
                .ForMember(dest => dest.CareerStart, opt => opt.MapFrom(src =>
                    src.CareerStart.HasValue ? src.CareerStart.Value.Year : (int?)null))
                .ForMember(dest => dest.CareerEnd, opt => opt.MapFrom(src =>
                    src.CareerEnd.HasValue ? src.CareerEnd.Value.Year : (int?)null));

            CreateMap<Song, SongResponse>()
                .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src =>
                    src.Artist != null ? src.Artist.Name : string.Empty))
                .ForMember(dest => dest.AlbumId, opt => opt.MapFrom(src => src.AlbumId))
                .ForMember(dest => dest.AlbumTitle, opt => opt.MapFrom(src =>
                    src.Album != null ? src.Album.Title : null));

            CreateMap<Album, AlbumResponse>()
                .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src =>
                    src.Artist != null ? src.Artist.Name : string.Empty))
                .ForMember(dest => dest.SongsCount, opt => opt.MapFrom(src =>
                    src.Songs != null ? src.Songs.Count : 0));
        }
    }
}