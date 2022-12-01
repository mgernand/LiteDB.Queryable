namespace LiteDB.Queryable
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	internal static class TypeExtensions
	{
		public static bool IsNullable(this Type type)
		{
			bool isNullable = type.Name == "Nullable`1";
			return isNullable;
		}

		public static bool IsComparer(this Type type)
		{
			bool isComparer = type.Name is "IComparer" or "IComparer`1";
			return isComparer;
		}

		public static Type GetSequenceType(this Type type)
		{
			Type sequenceType = type.TryGetSequenceType();
			return sequenceType ?? throw new ArgumentException($"The type {type.Name} does not represent a sequence.");
		}

		private static Type TryGetSequenceType(this Type type)
		{
			Type elementType = type.TryGetElementType(typeof(IEnumerable<>)) ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));
			return elementType ;
		}

		private static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
		{
			if(type.IsGenericTypeDefinition)
			{
				return null;
			}

			Type resultType = null;

			foreach(Type typeImplementation in type.GetGenericTypeImplementations(interfaceOrBaseType))
			{
				if(resultType == null)
				{
					resultType = typeImplementation;
				}
				else
				{
					break;
				}
			}

			return resultType?.GenericTypeArguments.FirstOrDefault();
		}

		private static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
		{
			TypeInfo typeInfo = type.GetTypeInfo();

			if(!typeInfo.IsGenericTypeDefinition)
			{
				foreach(Type baseType in interfaceOrBaseType.GetTypeInfo().IsInterface ? typeInfo.ImplementedInterfaces : type.GetBaseTypes())
				{
					if(baseType.IsGenericType && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
					{
						yield return baseType;
					}
				}

				if(type.IsGenericType && type.GetGenericTypeDefinition() == interfaceOrBaseType)
				{
					yield return type;
				}
			}
		}

		private static IEnumerable<Type> GetBaseTypes(this Type type)
		{
			for(Type currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
			{
				yield return currentType;
			}
		}
	}
}
