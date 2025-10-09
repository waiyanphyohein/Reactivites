using System;
using AutoMapper;
using Domain;

namespace Application.Core;

public class MappingProfiles : Profile
{
    // Constructor
    public MappingProfiles() => CreateMap<Activity, Activity>()
        // don't overwrite destination values with null source values
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
            srcMember != null &&
            !(srcMember is string str && string.IsNullOrWhiteSpace(str)) &&
            !(srcMember is double d && d == 0) && // also ignore zero for double types (like Latitude and Longitude)
            !(srcMember is float f && f == 0f) && // also ignore zero for float types
            !(srcMember is int i && i == 0) && // also ignore zero for int types
            !(srcMember is long l && l == 0) &&// also ignore zero for long types
            !(srcMember is decimal m && m == 0m) &&// also ignore zero for decimal types
            !(srcMember is DateTime dt && dt == default) // also ignore default DateTime values
        )); // also ignore empty strings
}
