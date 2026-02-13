using Jukebox_Backend.Data;
using Jukebox_Backend.Models.Dto.Requests;
using Jukebox_Backend.Models.Dto.Responses;
using Jukebox_Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jukebox_Backend.Services
{
    public class CountryService : ServiceBase
    {
        public CountryService(ApplicationDbContext context) : base(context) { }

        // get all countries
        public async Task<List<CountryResponse>> GetAllAsync()
        {
            try
            {
                return await _context.Countries
                    .AsNoTracking()
                    .Select(c => new CountryResponse
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Name = c.Name
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<CountryResponse>();
            }
        }

        // get country by id
        public async Task<CountryResponse?> GetByIdAsync(int id)
        {
            try
            {
                var country = await _context.Countries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (country is null) return null;

                return new CountryResponse
                {
                    Id = country.Id,
                    Code = country.Code,
                    Name = country.Name
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // create country
        public async Task<CountryResponse?> CreateAsync(CreateCountryRequest request)
        {
            try
            {
                var exists = await _context.Countries
                    .AnyAsync(c => c.Code == request.Code);

                if (exists) return null;

                var country = new Country
                {
                    Code = request.Code.ToUpper(),
                    Name = request.Name
                };

                _context.Countries.Add(country);
                bool saved = await SaveAsync();

                if (!saved) return null;

                return new CountryResponse
                {
                    Id = country.Id,
                    Code = country.Code,
                    Name = country.Name
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // delete country
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var country = await _context.Countries.FindAsync(id);

                if (country is null) return false;

                _context.Countries.Remove(country);
                return await SaveAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}