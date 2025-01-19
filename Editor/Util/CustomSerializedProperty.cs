using Sandbox.Diagnostics;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Panelize;
internal class CustomSerializedProperty : SerializedProperty
{
	protected Type _type;
	private readonly PropertyInfo _prop;
	private readonly object _target;
	private readonly DisplayInfo _info;

	public CustomSerializedProperty( PropertyInfo prop, object target )
	{
		Assert.NotNull( target );
		Assert.True( prop.CanRead );

		_type = prop.PropertyType;
		_prop = prop;
		_target = target;
		_info = DisplayInfo.ForMember( _prop );

		InitProperties();
	}

	public CustomSerializedProperty( MemberExpression expression, object target )
	{
		Assert.NotNull( expression );
		if ( expression.Member is not PropertyInfo prop )
			throw new ArgumentException( "Member must be a property!" );

		_type = expression.Type;
		_prop = prop;
		_target = target;
		_info = DisplayInfo.ForMember( prop );

		InitProperties();
	}

	protected void InitProperties()
	{
		CustomName = _prop.Name;
		CustomTitle = string.IsNullOrEmpty(_info.Name) ? _prop.Name.ToTitleCase() : _info.Name;
		CustomDescription = _info.Description;
		CustomGroup = _info.Group;
		CustomOrder = _info.Order;
		EditableOverride = _info.Browsable && _prop.CanWrite;
	}

	public override SerializedObject Parent
	{ 
		get
		{
			if(EditorTypeLibrary.TryGetType(_target.GetType(), out var desc))
			{
				return EditorTypeLibrary.GetSerializedObject( _target );
			}

			return null;
		} 
	}
	public override bool IsProperty => true;
	public override string Name => CustomName;
	public override string DisplayName => CustomTitle;
	public override string Description => CustomDescription;
	public override string GroupName => CustomGroup;
	public override int Order => CustomOrder;
	public override bool IsEditable => EditableOverride;
	public override Type PropertyType => _prop.PropertyType;
	public override string SourceFile => default;
	public override int SourceLine => default;
	public override bool HasChanges => false;

	public string CustomName;
	public string CustomTitle;
	public string CustomDescription;
	public string CustomGroup;
	public int CustomOrder;
	public bool EditableOverride;
	public Action<object> OnSetValue;
	public override T GetValue<T>( T defaultValue = default )
	{
		try
		{
			return (T)ValueToType( typeof(T), _prop.GetValue( _target ), defaultValue );
		}
		catch
		{
			return defaultValue;
		}
	}
	public override void SetValue<T>( T value )
	{
		try
		{
			_prop.SetValue( _target, ValueToType( _type, value ) );
			OnSetValue?.Invoke( value );
		}
		catch
		{

		}
	}

	public override IEnumerable<Attribute> GetAttributes()
	{
		return _prop?.GetCustomAttributes() ?? base.GetAttributes();
	}
	public override bool TryGetAsObject( out SerializedObject obj )
	{
		if(EditorTypeLibrary.TryGetType(_target.GetType(), out var _))
		{
			obj = EditorTypeLibrary.GetSerializedObject( _target );
			return true;
		}
		return base.TryGetAsObject( out obj );
	}
	protected object ValueToType( Type type, object value, object defaultValue = default )
	{
		try
		{
			if ( value == null )
			{
				return defaultValue;
			}

			if ( value.GetType().IsAssignableTo( type ) )
			{
				return value;
			}

			if ( type == typeof( string ) )
			{
				return $"{value}";
			}

			if ( value.GetType() == typeof( string ) )
			{
				return JsonSerializer.Deserialize( (string)value, type );
			}

			if ( type.IsEnum && value is IConvertible )
			{
				try
				{
					return Enum.ToObject( type, Convert.ToInt64( value ) );
				}
				catch
				{
					return defaultValue;
				}
			}

			object converted = Convert.ChangeType( value, type );
			if ( converted != null )
			{
				return converted;
			}

			return JsonSerializer.SerializeToElement( value ).Deserialize( type );
		}
		catch ( Exception )
		{
			return defaultValue;
		}
	}
}
