using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GotFired.Model.Entities.Enums
{
    public static class EnumHelper
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            if(enumValue!=null)
            {
                return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            .GetName();
            }
            return string.Empty;
            
        }
        public static IList<LookupEntity> GetEnumLookups<TEnum>() where TEnum : struct
        {
            var enumerationType = typeof(TEnum);

            if (!enumerationType.IsEnum)
                throw new ArgumentException("Enumeration type is expected.");

            var dictionary = new Dictionary<int, string>();
            var lookups = new List<LookupEntity>();
            foreach (var value in Enum.GetValues(enumerationType))
            {
                Enum test = Enum.Parse(typeof(TEnum), value.ToString()) as Enum;
                int x = Convert.ToInt32(test); // x is the integer value of enum

                var name = Enum.GetName(enumerationType, value);
                lookups.Add(new LookupEntity { ID = x, Name = test.GetDisplayName() });
            }

            return lookups;
        }
    }
   
    public enum AgeInterval : byte
    {
        /// <summary>
        /// 18-24
        /// </summary>
        [Display(Name = "18 - 24")]
        EighteenToTwentyFour = 1,
        /// <summary>
        /// 25-34
        /// </summary>
        [Display(Name = "25 - 34")]
        TwentyFiveToThirtyFour = 2,
        /// <summary>
        /// 35-45
        /// </summary>
        [Display(Name = "35 - 44")]
        ThirtyFiveToFortyFour = 3,
        /// <summary>
        /// 45-54
        /// </summary>
        [Display(Name = "45 - 54")]
        FortyFiveToFiftyFour = 4,
        /// <summary>
        /// 55+
        /// </summary>
        [Display(Name = "55 +")]
        FiftyFivePlus = 5
    }
    public enum DismissalState : byte
    {
        [Display(Name = "İşten çıkartıldım")]
        Dissmised = 1,
        [Display(Name = "İstifaya zorlandım / istifa ettirildim")]
        ForcedToResign = 2,
        [Display(Name = "Şu anda işten çıkarılma tehdidi altındayım")]
        AboutToBeDismissed = 3
    }
    public enum AppealState : byte
    {
        [Display(Name = "Bekliyor")]
        Pending = 1,
        [Display(Name = "Cevaplandı")]
        Answered = 2,
    }
    public enum EducationState : byte
    {
        [Display(Name = "İlköğretim")]
        PrimarySchool = 1,
        [Display(Name = "Orta Öğretim")]
        SecondarySchool = 2,
        [Display(Name = "Lise")]
        HighSchool = 3,
        [Display(Name = "Üniversite")]
        University = 4,
        [Display(Name = "Yüksek Lisans")]
        Graduate = 5
    }
    public enum EmployeeCount : byte
    {
        [Display(Name = "30 kişiden az")]
        LessThenThirty = 1,
        [Display(Name = "30 kişi ve üstü")]
        MoreThenThirty = 2
    }
    public enum EmploymentDurationSince : byte
    {
        [Display(Name = "6 aydan az")]
        SixMonthAndLess = 1,
        [Display(Name = "6 ay - 1 yıl arası")]
        FromSixMonthsToOneYear = 2,
        [Display(Name = "1 - 1.5 yıl arası")]
        FromOneYearToOneAndHalfYear = 3,
        [Display(Name = "1.5 yıl - 3 yıl arası")]
        FromOneAndHalfYearToThreeYears = 4,
        [Display(Name = "3 yıldan fazla")]
        MoreThanThreeYears = 5
    }
    public enum Gender : byte
    {
        [Display(Name = "Erkek")]
        Male = 1,
        [Display(Name = "Kadın")]
        Female = 2
    }
}
