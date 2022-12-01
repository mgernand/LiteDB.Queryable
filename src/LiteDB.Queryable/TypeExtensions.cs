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

		public static Type GetSequenceType(this Type type)
		{
			Type sequenceType = type.TryGetSequenceType();
			return sequenceType ?? throw new ArgumentException($"The type {type.Name} does not represent a sequence.");
		}

		public static Type TryGetSequenceType(this Type type)
		{
			Type elementType = type.TryGetElementType(typeof(IEnumerable<>));
			return (object)elementType != null ? elementType : type.TryGetElementType(typeof(IAsyncEnumerable<>));
		}

		public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
		{
			if(type.IsGenericTypeDefinition)
			{
				return null;
			}

			IEnumerable<Type> typeImplementations = type.GetGenericTypeImplementations(interfaceOrBaseType);
			Type type1 = null;
			foreach(Type type2 in typeImplementations)
			{
				if(type1 == null)
				{
					type1 = type2;
				}
				else
				{
					type1 = null;
					break;
				}
			}

			return (object)type1 == null ? null : type1.GenericTypeArguments.FirstOrDefault();
		}

		public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			if(!typeInfo.IsGenericTypeDefinition)
			{
				foreach(Type type1 in interfaceOrBaseType.GetTypeInfo().IsInterface ? typeInfo.ImplementedInterfaces : type.GetBaseTypes())
				{
					if(type1.IsGenericType && type1.GetGenericTypeDefinition() == interfaceOrBaseType)
					{
						yield return type1;
					}
				}

				if(type.IsGenericType && type.GetGenericTypeDefinition() == interfaceOrBaseType)
				{
					yield return type;
				}
			}
		}

		public static IEnumerable<Type> GetBaseTypes(this Type type)
		{
			for(Type currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
			{
				yield return currentType;
			}
		}
	}
}
