using BuildingBlocks.Domain.Exceptions;

namespace ClassManagement.Application.Utils;

public static class ParseAgeRange
{
    public static (int? MinAge, int? MaxAge) FromString(string? ageRange)
    {
        if (string.IsNullOrWhiteSpace(ageRange))
            return (null, null);
            
        var parts = ageRange.Split('-')
                            .Select(p => p.Trim())
                            .ToArray();
        int? ageFrom = null;
        int? ageTo = null;

        if (parts.Length == 1)
        {
            if (int.TryParse(parts[0], out var from))
                return (from, null); 
        }
        else if (parts.Length == 2)
        {
            if (int.TryParse(parts[0], out var from))
                ageFrom = from;
            if (int.TryParse(parts[1], out var to))
                ageTo = to;
                
            if (ageFrom.HasValue && ageTo.HasValue && ageFrom > ageTo)
                throw new BadRequestException("Age from must be less than or equal to age to.");
                
            return (ageFrom, ageTo); 
        }
        return (null, null);
    }
}