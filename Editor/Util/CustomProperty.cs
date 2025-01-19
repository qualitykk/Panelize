using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;

public class CustomProperty : SerializedProperty
{
	public override bool IsMethod => false;
	public Type Type { get; }
	public Func<object> Get { get; }
	public Action<object> Set { get; }
	public CustomProperty(Type type, Func<object> get, Action<object> set)
	{
		Type = type;
		Get = get;
		Set = set;
	}
	public override Type PropertyType => Type;
	public override T GetValue<T>( T defaultValue = default )
	{
		var result = Get?.Invoke();

		// Stupid hack because stupid color doesnt format correctly by default.. stupid!
		if ( typeof( T ) == typeof( string ) && result is Color color )
			result = color.ToString( true, true );

		T value = ValueToType<T>( result );
		
		if ( value is T t )
			return t;

		return defaultValue;
	}

	public override void SetValue<T>( T value )
	{
		Set?.Invoke( value );
	}
}
