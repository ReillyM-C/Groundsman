﻿using System;

namespace Groundsman.Models
{
    public enum PropertyType
    {
        String, Integer, Float, Bool
    }

    public class Property
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public int Type { get; set; }

        public Property(string key, object value, int type = 0)
        {
            Key = key;
            Value = value;
            Type = type;
        }

        public static Property FromObject(string name, object value)
        {
            Type propertyType = value.GetType();

            if (propertyType == typeof(bool))
            {
                return new Property(name, Convert.ToBoolean(value), 3);
            }
            else if (propertyType == typeof(float) || propertyType == typeof(double))
            {
                return new Property(name, Convert.ToSingle(value), 2);
            }
            else if (propertyType == typeof(long) || propertyType == typeof(int) || propertyType == typeof(short))
            {
                return new Property(name, Convert.ToInt32(value), 1);
            }
            else
            {
                return new Property(name, value.ToString(), 0);
            }
        }
    }

}
