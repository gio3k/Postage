using System.Reflection;

namespace Postage;

public static class ReflectUtil
{
	public static PropertyInfo Property( this Type type, string name )
	{
		return type.GetProperty( name,
			BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
	}

	public static FieldInfo Field( this Type type, string name )
	{
		return type.GetField( name,
			BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
	}

	public static EventInfo Event( this Type type, string name )
	{
		return type.GetEvent( name,
			BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
	}

	public static MethodInfo Method( this Type type, string name )
	{
		return type.GetMethod( name,
			BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
	}

	public static MethodInfo Method( this Type type, string name, Type[] types )
	{
		return type.GetMethod( name,
			BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, types );
	}
}
