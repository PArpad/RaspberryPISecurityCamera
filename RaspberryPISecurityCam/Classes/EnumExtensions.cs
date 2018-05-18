using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RaspberryPISecurityCam.Classes
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());
            var DisplayNameAttribute = fi.CustomAttributes.FirstOrDefault(c => c.AttributeType == typeof(DisplayNameAttribute));
            if (DisplayNameAttribute == null) return enumValue.ToString();
            return DisplayNameAttribute.ConstructorArguments.FirstOrDefault().Value.ToString();
        }
    }
}
