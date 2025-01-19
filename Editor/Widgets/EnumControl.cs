using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Panelize;

public abstract class EnumControl<T> : Widget where T : struct, Enum
{
	public Dictionary<T, DisplayInfo> DisplayOverrides { get; set; } = new();
	/// <summary>
	/// Display items in this order.
	/// </summary>
	public List<T> ValueOverrides { get; set; } = new();
	public T Value { get; protected set; }
	public Action<T> OnValueChanged { get; set; }
	public EnumControl(T defaultValue = default)
	{
		Value = default;
	}
	public virtual void SetValue(T value)
	{
		Value = value;
		OnValueChanged?.Invoke( value );
	}
	public void SetDisplay( T value, string icon = "", string name = "", string description = "", string group = "" )
	{
		bool iconChanged = !string.IsNullOrEmpty( icon );
		bool nameChanged = !string.IsNullOrEmpty( name );
		bool descriptionChanged = !string.IsNullOrEmpty( description );
		bool groupChanged = !string.IsNullOrEmpty( group );

		if ( DisplayOverrides.TryGetValue( value, out var o ) )
		{
			if(iconChanged)
				o.Icon = icon;

			if ( nameChanged )
				o.Name = name;

			if(descriptionChanged) 
				o.Description = description;

			if ( groupChanged )
				o.Group = group;

			DisplayOverrides[value] = o;
		}
		else
		{
			DisplayInfo info = new();
			DisplayInfo existing = DisplayInfo.ForEnumValues<T>()
				.Where(e => e.value.Equals(value)).FirstOrDefault().info;

			info.Icon = iconChanged ? icon : existing.Icon;
			info.Name = nameChanged ? name : existing.Name;
			info.Description = descriptionChanged ? description : existing.Description;
			info.Group = groupChanged ? group : existing.Group;
			info.Browsable = true;
			DisplayOverrides.Add( value, info );
		}
	}
	/// <summary>
	/// Set order of values to be displayed.
	/// Values not in list are not displayed.
	/// </summary>
	/// <param name="values"></param>
	public void SetOrder( params T[] values )
	{
		ValueOverrides = values.ToList();
	}
	public (T, DisplayInfo)[] GetValueDisplays()
	{
		var valueDisplays = DisplayInfo.ForEnumValues<T>();
		if ( ValueOverrides != null && ValueOverrides.Count > 0 )
		{
			valueDisplays = valueDisplays
				.IntersectBy( ValueOverrides, kv => kv.value )
				.OrderBy( kv => ValueOverrides.IndexOf( kv.value ) )
				.ToArray();
		}

		if(DisplayOverrides.Count > 0)
		{
			List<(T value, DisplayInfo info)> newDisplays = new();
			foreach((T value, DisplayInfo info) in valueDisplays)
			{
				if(DisplayOverrides.ContainsKey(value))
				{
					DisplayInfo o = DisplayOverrides[value];
					newDisplays.Add( (value, o) );
				}
				else
				{
					newDisplays.Add( (value, info) );
				}
			}

			valueDisplays = newDisplays.ToArray();
		}

		return valueDisplays;
	}

	public DisplayInfo GetValueDisplay(T value)
	{
		if(DisplayOverrides.ContainsKey(value))
		{
			return DisplayOverrides[value];
		}

		return DisplayInfo.ForEnumValues<T>()
				.Where( e => e.value.Equals( value ) ).FirstOrDefault().info;
	}
}
