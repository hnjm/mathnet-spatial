﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace MathNet.Spatial
{
    /// <summary>
    /// Extensions for Xml generation
    /// </summary>
    [Obsolete("This class should not have been public, will be removed in a future version. Made obsolete 2017-12-03")]
    public static class XmlExt
    {
        public static void WriteValueToReadonlyField<TClass, TProperty>(
            TClass item,
            TProperty value,
            Expression<Func<TProperty>> fieldExpression)
        {
            string name = ((MemberExpression)fieldExpression.Body).Member.Name;
            var allFields = GetAllFields(item.GetType()).ToList();
            allFields
                .Single(x => x.Name == name)
                .SetValue(item, value);
        }

        public static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
            {
                return Enumerable.Empty<FieldInfo>();
            }

            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static |
                                       BindingFlags.Public | BindingFlags.NonPublic;
            return t.GetFields(bindingAttr)
                .Concat(GetAllFields(t.BaseType));
        }

        public static void SetReadonlyFields<TItem>(ref TItem self, string[] fields, double[] values)
            where TItem : struct
        {
            object boxed = self;
            for (int i = 0; i < fields.Length; i++)
            {
                var fieldInfo = self.GetType().GetField(fields[i]);
                fieldInfo.SetValue(boxed, values[i]);
            }

            self = (TItem)boxed;
        }

        internal static void SetReadonlyField<TItem, TValue>(
            ref TItem self,
            Expression<Func<TItem, TValue>> func,
            TValue value)
            where TItem : struct
        {
            var fieldInfo = self.GetType()
                .GetField(((MemberExpression)func.Body).Member.Name);
            object boxed = self;
            fieldInfo.SetValue(boxed, value);
            self = (TItem)boxed;
        }

        public static string ReadAttributeOrDefault(this XElement e, string localName)
        {
            return e.Attribute(localName)?.Value;
        }

        public static XElement SingleElement(this XElement e, string localName)
        {
            return e.Elements()
                .Single(x => x.Name.LocalName == localName);
        }

        public static XElement SingleElementOrDefault(this XElement e, string localName)
        {
            return e.Elements()
                .SingleOrDefault(x => x.Name.LocalName == localName);
        }

        public static double AsDouble(this XElement e, bool throwIfNull = true, double valueIfNull = 0)
        {
            if (!throwIfNull && e == null)
            {
                return valueIfNull;
            }

            return XmlConvert.ToDouble(e.Value);
        }

        public static double AsDouble(this XAttribute e, bool throwIfNull = true)
        {
            if (!throwIfNull && e == null)
            {
                return 0;
            }

            return XmlConvert.ToDouble(e.Value);
        }

        public static T AsEnum<T>(this XElement e)
            where T : struct, IConvertible
        {
            return (T)Enum.Parse(typeof(T), e.Value);
        }

        public static T AsEnum<T>(this XAttribute e)
            where T : struct, IConvertible
        {
            return (T)Enum.Parse(typeof(T), e.Value);
        }

        public static IEnumerable<XElement> ElementsNamed(this XElement e, string localName)
        {
            return e.Elements()
                .Where(x => x.Name.LocalName == localName);
        }

        public static XAttribute SingleAttribute(this XElement e, string localName)
        {
            return e.Attributes()
                .Single(x => x.Name.LocalName == localName);
        }

        public static XmlReader SingleElementReader(this XElement e, string localName)
        {
            return e.SingleElement(localName)
                .CreateReader();
        }

        public static string ReadAttributeOrElement(this XElement e, string localName)
        {
            XAttribute xattribute = e.Attributes()
                .SingleOrDefault(x => x.Name.LocalName == localName);
            if (xattribute != null)
            {
                return xattribute.Value;
            }

            XElement xelement = e.Elements()
                .SingleOrDefault(x => x.Name.LocalName == localName);
            if (xelement != null)
            {
                return xelement.Value;
            }

            throw new XmlException($"Attribute or element {localName} not found");
        }

        public static T ReadAttributeOrElementEnum<T>(this XElement e, string localName)
            where T : struct, IConvertible
        {
            return (T)Enum.Parse(typeof(T), e.ReadAttributeOrElement(localName));
        }

        public static string ReadAttributeOrElementOrDefault(this XElement e, string localName)
        {
            XAttribute xattribute = e.Attributes()
                .SingleOrDefault(x => x.Name.LocalName == localName);
            if (xattribute != null)
            {
                return xattribute.Value;
            }

            XElement xelement = e.Elements()
                .SingleOrDefault(x => x.Name.LocalName == localName);
            if (xelement != null)
            {
                return xelement.Value;
            }

            return null;
        }

        public static XmlWriter WriteAttribute<T>(this XmlWriter writer, string name, T value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(value);
            writer.WriteEndAttribute();
            return writer;
        }

        public static XmlWriter WriteAttribute(this XmlWriter writer, string name, double value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(value.ToString("G15"));
            writer.WriteEndAttribute();
            return writer;
        }
    }
}
