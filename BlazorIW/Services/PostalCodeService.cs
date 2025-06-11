using BlazorIW.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorIW.Services;

public class PostalCodeService(ApplicationDbContext db)
{
    private readonly ApplicationDbContext _db = db;

    public async Task<(double Latitude, double Longitude)> GetLatLongAsync(string zipcode, CancellationToken ct = default)
    {
        var code = await _db.PostalCodes.FirstOrDefaultAsync(p => p.Zipcode == zipcode, ct);
        if (code is null)
        {
            code = await _db.PostalCodes.FirstOrDefaultAsync(p => p.Zipcode == "231-0045", ct);
        }

        return code is null
            ? (35.4465, 139.6325)
            : (code.Latitude, code.Longitude);
    }
}
