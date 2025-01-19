using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Panelize;
internal static class ReflectionUtils
{
	private const bool DEBUG_PRINT = false;
	public static object CreateObject( this Type type, params object[] constructorParameters )
	{
		Type[] parameterTypes = constructorParameters.Select( p => p.GetType() ).ToArray();

		var constructor = type.GetConstructor( BindingFlags.Public |
			BindingFlags.NonPublic, parameterTypes );

		if ( constructor != null )
		{
			return constructor.Invoke( null, parameterTypes );
		}

		return null;
	}

	public static T CreateObject<T>( params object[] constructorParameters )
	{
		var result = CreateObject( typeof( T ), constructorParameters );
		if ( result is T validResult )
			return validResult;

		return default;
	}
	public static PropertyInfo GetProperty<T>( string name, BindingFlags flags = default )
	{
		return GetProperty( typeof( T ), name, flags );
	}
	public static PropertyInfo GetProperty( Type type, string name, BindingFlags flags = default )
	{
		if ( flags == default )
			flags = BindingFlags.Instance | BindingFlags.Public;
		return type.GetProperty( name, flags );
	}
	public static object CallMethod(this object obj, string name, params object[] parameters)
	{
		if(obj == null) return null;

		Type[] parameterTypes = parameters.Select( p => p.GetType() ).ToArray();
		Type type = obj.GetType();
		MethodInfo method = null;
		while ( type != typeof( object ) && type != null )
		{
			method = type.GetMethod( name, BindingFlags.Instance | BindingFlags.NonPublic |
				BindingFlags.Public, parameterTypes );

			if ( method != null )
			{
				if ( DEBUG_PRINT )
				{
					Log.Info( $"Invoke {method}" );
				}
				return method.Invoke( obj, parameters );
			}

			type = type.BaseType;
		}

		return null;
	}

	public static T CallMethod<T>(this object obj, string name, params object[] parameters)
	{
		if(obj == null) return default;

		var result = CallMethod(obj, name, parameters);
		if (result is T validResult)
		{
			return validResult;
		}

		return default;
	}

	public static object CallStaticMethod( this Type type, string name, params object[] parameters )
	{
		if ( type == null ) return null;

		Type[] parameterTypes = parameters.Select( p => p.GetType() ).ToArray();
		MethodInfo method = null;
		while ( type != typeof( object ) && type != null )
		{
			method = type.GetMethod( name, BindingFlags.Static | BindingFlags.NonPublic |
				BindingFlags.Public, parameterTypes );

			if ( method != null )
			{
				if ( DEBUG_PRINT )
				{
					Log.Info( $"Invoke Static {method}" );
				}

				return method.Invoke( null, parameters );
			}

			type = type.BaseType;
		}

		return null;
	}

	public static T CallStaticMethod<T>(this Type type, string name, params object[] parameters)
	{
		if ( type == null ) return default;

		var result = CallStaticMethod( type, name, parameters );
		if ( result is T validResult )
		{
			return validResult;
		}

		return default;
	}

	public static object GetMember( this object obj, string name ) 
	{
		if ( obj == null ) return default;

		Type type = obj.GetType();
		MemberInfo member = null;
		while(type != typeof(object) && type != null)
		{
			member = type.GetMember( name, BindingFlags.Instance | BindingFlags.NonPublic |
						BindingFlags.Public ).FirstOrDefault();

			if ( member != null ) break;

			type = type.BaseType;
		}
		
		
		if ( member != null )
		{
			if ( member is PropertyInfo prop)
			{
				if ( DEBUG_PRINT )
					Log.Info( $"Get Member {member} => {prop.GetValue(obj, null)}" );
				
				return prop.GetValue(obj, null);
			}
			else if( member is FieldInfo field)
			{
				if ( DEBUG_PRINT )
					Log.Info( $"Get Member {member} => {field.GetValue( obj )}" );
				
				return field.GetValue( obj );
			}
			else
			{
				if(DEBUG_PRINT)
					Log.Info( $"GOT MEMBER {member} WHICH IS NOT PROP/FIELD!!!" );
			}
		}
		else
		{
			if ( DEBUG_PRINT )
				Log.Info( $"No member {name} found on {obj}" );
		}

		return null;
	}

	public static T GetMember<T>( this object obj, string name )
	{
		if ( obj == null ) return default;

		var result = GetMember( obj, name );
		if ( result is T validResult )
		{
			return validResult;
		}

		return default;
	}

	public static object GetStaticMember(this Type t, string name)
	{
		if ( t == null ) return default;

		MemberInfo member = t.GetMember( name, BindingFlags.Static | BindingFlags.NonPublic |
			BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.GetField ).FirstOrDefault();

		if ( member != null )
		{
			if ( member is PropertyInfo prop )
			{
				return prop.GetValue( null, null );
			}
			else if ( member is FieldInfo field )
			{
				return field.GetValue( null );
			}
		}

		return null;
	}

	public static T GetStaticMember<T>(this Type t, string name)
	{
		if ( t == null ) return default;

		var result = GetMember( t, name );
		if ( result is T validResult )
		{
			return validResult;
		}

		return default;
	}
}
