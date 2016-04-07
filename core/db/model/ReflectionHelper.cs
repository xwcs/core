using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Linq;
using System.Text;
using xwcs.core.db.binding.attributes;

namespace xwcs.core.db.model{

	static class ExtensionMethods
	{
		public static bool TryGetInterfaceGenericParameters(this Type type, Type @interface, out Type[] typeParameters)
		{
			typeParameters = null;

			if (type.IsGenericType && type.GetGenericTypeDefinition() == @interface)
			{
				typeParameters = type.GetGenericArguments();
				return true;
			}

			var implements = type.FindInterfaces((ty, obj) => ty.IsGenericType && ty.GetGenericTypeDefinition() == @interface, null).FirstOrDefault();
			if (implements == null)
				return false;

			typeParameters = implements.GetGenericArguments();
			return true;
		}
	}

	

	/// <summary>
	/// Contains helper functions related to reflection
	/// </summary>
	public static class ReflectionHelper
    {
        /// <summary>
        /// Searches for a property in the given property path
        /// </summary>
        /// <param name="rootType">The root/starting point to start searching</param>
        /// <param name="propertyPath">The path to the property. Ex Customer.Address.City</param>
        /// <returns>A <see cref="PropertyInfo"/> describing the property in the property path.</returns>
        public static PropertyInfo GetPropertyFromPath(Type rootType,string propertyPath)
        {
            if (rootType == null)
                throw new ArgumentNullException("rootType");
            
            Type propertyType = rootType;
            PropertyInfo propertyInfo = null;
            string[] pathElements = propertyPath.Split(new char[1] { '.' });
            for (int i = 0; i < pathElements.Length; i++)
            {
                propertyInfo = propertyType.GetProperty(pathElements[i], BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    propertyType = propertyInfo.PropertyType;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("propertyPath",propertyPath,"Invalid property path");
                }
            }
            return propertyInfo;
        }


        public static PropertyDescriptor GetPropertyDescriptorFromPath(Type rootType, string propertyPath)
        {
            string propertyName;
            bool lastProperty = false;
            if (rootType == null)
                throw new ArgumentNullException("rootType");

            if (propertyPath.Contains("."))
                propertyName = propertyPath.Substring(0, propertyPath.IndexOf("."));
            else
            {
                propertyName = propertyPath;
                lastProperty = true;
            }

            PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(rootType)[propertyName];
            if (propertyDescriptor == null)
                throw new ArgumentOutOfRangeException("propertyPath", propertyPath, string.Format("Invalid property path for type '{0}' ",rootType.Name));


            if (!lastProperty)
                return GetPropertyDescriptorFromPath(propertyDescriptor.PropertyType, propertyPath.Substring(propertyPath.IndexOf(".") + 1));
            else
                return propertyDescriptor;
        }

		public static IEnumerable<CustomAttribute> GetCustomAttributesFromPath(Type rootType, string propertyPath) {
			PropertyDescriptor pd = ReflectionHelper.GetPropertyDescriptorFromPath(rootType, propertyPath);
			Type t1 = pd.ComponentType;
			IEnumerable<CustomAttribute> attrs1 = t1.GetProperty(pd.Name).GetCustomAttributes(typeof(CustomAttribute), true).Cast<CustomAttribute>();
			List<MetadataTypeAttribute> l = TypeDescriptor.GetAttributes(t1).OfType<MetadataTypeAttribute>().ToList();
			if(l.Count > 0) {
				Type t2 = l.Single().MetadataClassType;
				IEnumerable<CustomAttribute> attrs2 = t2.GetProperty(pd.Name).GetCustomAttributes(typeof(CustomAttribute), true).Cast<CustomAttribute>();
				return attrs1.Union(attrs2);
			}
			else {
				return attrs1;
			}			
		}
	}
}