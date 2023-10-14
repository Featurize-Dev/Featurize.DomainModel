using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace Featurize.DomainModel;

internal class PrivateReflectionDynamicObject : DynamicObject
{

#pragma warning disable IDE0044 // Add readonly modifier
    private static IDictionary<Type, IDictionary<string, IProperty>> _propertiesOnType = new ConcurrentDictionary<Type, IDictionary<string, IProperty>>();
#pragma warning restore IDE0044 // Add readonly modifier

    // Simple abstraction to make field and property access consistent
    private interface IProperty
    {
        string Name { get; }
        object GetValue(object obj, object[] index);
        void SetValue(object obj, object val, object[] index);
    }

    // IProperty implementation over a PropertyInfo
    private class Property : IProperty
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal PropertyInfo PropertyInfo { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        string IProperty.Name
        {
            get
            {
                return PropertyInfo.Name;
            }
        }

        object IProperty.GetValue(object obj, object[] index)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return PropertyInfo.GetValue(obj, index);
#pragma warning restore CS8603 // Possible null reference return.
        }

        void IProperty.SetValue(object obj, object val, object[] index)
        {
            PropertyInfo.SetValue(obj, val, index);
        }
    }

    // IProperty implementation over a FieldInfo
    private class Field : IProperty
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal FieldInfo FieldInfo { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        string IProperty.Name
        {
            get
            {
                return FieldInfo.Name;
            }
        }


        object IProperty.GetValue(object obj, object[] index)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return FieldInfo.GetValue(obj);
#pragma warning restore CS8603 // Possible null reference return.
        }

        void IProperty.SetValue(object obj, object val, object[] index)
        {
            FieldInfo.SetValue(obj, val);
        }
    }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private object RealObject { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable IDE1006 // Naming Styles
    private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
#pragma warning restore IDE1006 // Naming Styles

    internal static object WrapObjectIfNeeded(object o)
    {
        // Don't wrap primitive types, which don't have many interesting internal APIs
        if (o == null || o.GetType().IsPrimitive || o is string)
#pragma warning disable CS8603 // Possible null reference return.
            return o;
#pragma warning restore CS8603 // Possible null reference return.

        return new PrivateReflectionDynamicObject() { RealObject = o };
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        IProperty prop = GetProperty(binder.Name);

        // Get the property value
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        result = prop.GetValue(RealObject, index: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Wrap the sub object if necessary. This allows nested anonymous objects to work.
        result = WrapObjectIfNeeded(result);

        return true;
    }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override bool TrySetMember(SetMemberBinder binder, object value)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    {
        IProperty prop = GetProperty(binder.Name);

        // Set the property value
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        prop.SetValue(RealObject, value, index: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        return true;
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        // The indexed property is always named "Item" in C#
        IProperty prop = GetIndexProperty();
        result = prop.GetValue(RealObject, indexes);

        // Wrap the sub object if necessary. This allows nested anonymous objects to work.
        result = WrapObjectIfNeeded(result);

        return true;
    }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    {
        // The indexed property is always named "Item" in C#
        IProperty prop = GetIndexProperty();
        prop.SetValue(RealObject, value, indexes);
        return true;
    }

    // Called when a method is called
#pragma warning disable CS8610 // Nullability of reference types in type of parameter doesn't match overridden member.
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
#pragma warning restore CS8610 // Nullability of reference types in type of parameter doesn't match overridden member.
    {
        result = InvokeMemberOnType(RealObject.GetType(), RealObject, binder.Name, args);

        // Wrap the sub object if necessary. This allows nested anonymous objects to work.
        result = WrapObjectIfNeeded(result);

        return true;
    }

    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        result = Convert.ChangeType(RealObject, binder.Type);
        return true;
    }

    public override string ToString()
    {
#pragma warning disable CS8603 // Possible null reference return.
        return RealObject.ToString();
#pragma warning restore CS8603 // Possible null reference return.
    }

    private IProperty GetIndexProperty()
    {
        // The index property is always named "Item" in C#
        return GetProperty("Item");
    }

    private IProperty GetProperty(string propertyName)
    {
        // Get the list of properties and fields for this type
        IDictionary<string, IProperty> typeProperties = GetTypeProperties(RealObject.GetType());

        // Look for the one we want
#pragma warning disable IDE0018 // Inline variable declaration
        IProperty property;
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        if (typeProperties.TryGetValue(propertyName, out property))
        {
            return property;
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        // The property doesn't exist

        // Get a list of supported properties and fields and show them as part of the exception message
        // For fields, skip the auto property backing fields (which name start with <)
        var propNames = typeProperties.Keys.Where(name => name[0] != '<').OrderBy(name => name);
        throw new ArgumentException(
            string.Format(
            "The property {0} doesn't exist on type {1}. Supported properties are: {2}",
            propertyName, RealObject.GetType(), string.Join(", ", propNames)));
    }

    private static IDictionary<string, IProperty> GetTypeProperties(Type type)
    {
        // First, check if we already have it cached
#pragma warning disable IDE0018 // Inline variable declaration
        IDictionary<string, IProperty> typeProperties;
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        if (_propertiesOnType.TryGetValue(type, out typeProperties))
        {
            return typeProperties;
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        // Not cache, so we need to build it

        typeProperties = new ConcurrentDictionary<string, IProperty>();

        // First, add all the properties
        foreach (PropertyInfo prop in type.GetProperties(bindingFlags).Where(p => p.DeclaringType == type))
        {
            typeProperties[prop.Name] = new Property() { PropertyInfo = prop };
        }

        // Now, add all the fields
        foreach (FieldInfo field in type.GetFields(bindingFlags).Where(p => p.DeclaringType == type))
        {
            typeProperties[field.Name] = new Field() { FieldInfo = field };
        }

        // Finally, recurse on the base class to add its fields
        if (type.BaseType != null)
        {
            foreach (IProperty prop in GetTypeProperties(type.BaseType).Values)
            {
                typeProperties[prop.Name] = prop;
            }
        }

        // Cache it for next time
        _propertiesOnType[type] = typeProperties;

        return typeProperties;
    }

    private static object InvokeMemberOnType(Type type, object target, string name, object[] args)
    {
        try
        {
            // Try to incoke the method
#pragma warning disable CS8603 // Possible null reference return.
            return type.InvokeMember(
                name,
                BindingFlags.InvokeMethod | bindingFlags,
                null,
                target,
                args);
#pragma warning restore CS8603 // Possible null reference return.
        }
        catch (MissingMethodException)
        {
            // If we couldn't find the method, try on the base class
            if (type.BaseType != null)
            {
                return InvokeMemberOnType(type.BaseType, target, name, args);
            }
            //quick greg hack to allow methods to not exist!
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}


internal static class PrivateReflectionDynamicObjectExtensions
{
    public static dynamic AsDynamic(this object o)
    {
        return PrivateReflectionDynamicObject.WrapObjectIfNeeded(o);
    }
}
