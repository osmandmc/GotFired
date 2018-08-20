using GotFired.Model.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace GotFired.Api.Helper
{
    //public static class EnumHelper
    //{
    //    public static string GetDisplayName(this Enum enumValue)
    //    {
    //        return enumValue.GetType()
    //                        .GetMember(enumValue.ToString())
    //                        .First()
    //                        .GetCustomAttribute<DisplayAttribute>()
    //                        .GetName();
    //    }
    //    public static IList<LookupEntity> GetEnumLookups<TEnum>() where TEnum : struct
    //    {
    //        var enumerationType = typeof(TEnum);

    //        if (!enumerationType.IsEnum)
    //            throw new ArgumentException("Enumeration type is expected.");

    //        var dictionary = new Dictionary<int, string>();
    //        var lookups = new List<LookupEntity>();
    //        foreach (var value in Enum.GetValues(enumerationType))
    //        {
    //            Enum test = Enum.Parse(typeof(TEnum), value.ToString()) as Enum;
    //            int x = Convert.ToInt32(test); // x is the integer value of enum

    //            var name = Enum.GetName(enumerationType, value);
    //            lookups.Add(new LookupEntity { ID = x, Name = test.GetDisplayName() });
    //        }

    //        return lookups;
    //    }
        
    //}
}