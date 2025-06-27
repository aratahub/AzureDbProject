using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common
{
    public static class ReflectionHelper
    {
        public static void SetProperty(object entity, PropertyInfo property, object value)
        {
            var type = entity.GetType();
            Set(entity, type, property, value);
        }

        private static void Set(object entity, Type type, PropertyInfo property, object value)
        {
            var propertyInfo = type.GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            while (type != null && propertyInfo != null && propertyInfo.SetMethod == null)
            {
                if (type.BaseType == null)
                {
                    break;
                }
                type = type.BaseType;
                propertyInfo = type.GetProperty(property.Name);
            }

            if (type == null || propertyInfo == null || propertyInfo.SetMethod == null)
            {
                throw new NullReferenceException($"Type '{entity.GetType().Name}' doesn't have a set property '{property.Name}'.");
            }

            //propertyInfo.SetValue(entity, value);

            if (propertyInfo.PropertyType == typeof(string) && value is Guid guidValue)
            {
                propertyInfo.SetValue(entity, guidValue.ToString());
            }
            else
            {
                propertyInfo.SetValue(entity, value);
            }


        }
    }
}