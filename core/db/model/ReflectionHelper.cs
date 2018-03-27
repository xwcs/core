using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Linq;
using xwcs.core.db.binding.attributes;
using System.Reflection.Emit;

namespace xwcs.core.db.model{

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
            {
                //throw new ArgumentOutOfRangeException("propertyPath", propertyPath, string.Format("Invalid property path for type '{0}' ", rootType.Name));
                return null;
            }

            if (!lastProperty)
                return GetPropertyDescriptorFromPath(propertyDescriptor.PropertyType, propertyPath.Substring(propertyPath.IndexOf(".") + 1));
            else
                return propertyDescriptor;
        }

		public static IEnumerable<CustomAttribute> GetCustomAttributesFromPath(Type rootType, string propertyPath) {
			PropertyDescriptor pd = GetPropertyDescriptorFromPath(rootType, propertyPath);
            if (ReferenceEquals(pd, null))
            {
                return new List<CustomAttribute>();
            }
			Type t1 = pd.ComponentType;
			IEnumerable<CustomAttribute> attrs1 = t1.GetProperty(pd.Name).GetCustomAttributes(typeof(CustomAttribute), true).Cast<CustomAttribute>();
			List<MetadataTypeAttribute> l = TypeDescriptor.GetAttributes(t1).OfType<MetadataTypeAttribute>().ToList();
			if(l.Count > 0) {
				Type t2 = l.Single().MetadataClassType;
				PropertyInfo pi = t2.GetProperty(pd.Name);
				if(pi != null) {
					IEnumerable<CustomAttribute> attrs2 = pi.GetCustomAttributes(typeof(CustomAttribute), true).Cast<CustomAttribute>();
                    attrs1 = attrs1.Union(attrs2);
				}
			}
            return attrs1;
		}

        public static IEnumerable<Attribute> GetAttributesFromPath(Type rootType, string propertyPath)
        {
            PropertyDescriptor pd = null;
            Type t1 = null;
            IEnumerable<Attribute> attrs1 = null;
            string propertyName = "";
            try
            {
                pd = GetPropertyDescriptorFromPath(rootType, propertyPath);
                if (ReferenceEquals(pd, null))
                {
                    t1 = rootType;
                    attrs1 = new List<Attribute>();
                    propertyName = propertyPath;
                }
                else {
                t1 = pd.ComponentType;
                attrs1 = t1.GetProperty(pd.Name).GetCustomAttributes(typeof(Attribute), true).Cast<Attribute>();
                propertyName = pd.Name;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                pd = null;
                t1 = rootType;
                attrs1 = new List<Attribute>();
                propertyName = propertyPath;
            }          
            

            List<MetadataTypeAttribute> l = TypeDescriptor.GetAttributes(t1).OfType<MetadataTypeAttribute>().ToList();
            if (l.Count > 0)
            {
                Type t2 = l.Single().MetadataClassType;
                PropertyInfo pi = t2.GetProperty(propertyName);
                if (pi != null)
                {
                    IEnumerable<Attribute> attrs2 = pi.GetCustomAttributes(typeof(Attribute), true).Cast<Attribute>();
                    attrs1=attrs1.Union(attrs2);
                }
            }
            // custom just static meta types private to xwcs
            List<StaticMetatypeAttribute> l1 = TypeDescriptor.GetAttributes(t1).OfType<StaticMetatypeAttribute>().ToList();
            if (l1.Count > 0)
            {
                Type t2 = l1.Single().MetadataClassType;
                PropertyInfo pi = t2.GetProperty(propertyName);
                if (pi != null)
                {
                    IEnumerable<Attribute> attrs2 = pi.GetCustomAttributes(typeof(Attribute), true).Cast<Attribute>();
                    attrs1=attrs1.Union(attrs2);
                }
            }
            // custom just dynamic meta types private to xwcs
            List<DynamicMetatypeAttribute> l2 = TypeDescriptor.GetAttributes(t1).OfType<DynamicMetatypeAttribute>().ToList();
            if (l2.Count > 0)
            {
                Type t2 = l2.Single().MetadataClassType;

                binding.IDynamicAttributeProvider pr = (binding.IDynamicAttributeProvider)Activator.CreateInstance(t2);
               
                IEnumerable<Attribute> attrs2 = pr.GetAttributes(t1, propertyName);
                attrs1 = attrs1.Union(attrs2);
               
            }

            return attrs1;
        }

        public static void CopyObject(object from, object to) {
			to.CopyFrom(from);
		}


        public static PropertyBuilder AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig;

            FieldBuilder field = typeBuilder.DefineField("_" + propertyName, typeof(string), FieldAttributes.Private);
            PropertyBuilder property = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType,
                new[] { propertyType });

            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_value", getSetAttr, propertyType,
                Type.EmptyTypes);
            ILGenerator getIl = getMethodBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, field);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_value", getSetAttr, null,
                new[] { propertyType });
            ILGenerator setIl = setMethodBuilder.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, field);
            setIl.Emit(OpCodes.Ret);

            property.SetGetMethod(getMethodBuilder);
            property.SetSetMethod(setMethodBuilder);

            return property;
        }
    }
}
