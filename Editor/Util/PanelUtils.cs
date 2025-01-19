using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Panelize;

internal static class PanelUtils
{
	private static Type styleType = typeof( Styles );
	private static PropertyInfo[] styleProperties;
	private static Styles styleReference;
	static PanelUtils()
	{
		MakeReferenceStyle();
	}
	[EditorEvent.Hotload]
	private static void MakeReferenceStyle()
	{
		styleProperties = styleType.GetProperties();
		styleReference = new();
		styleReference.FillDefaults();
	}
	public static PropertyInfo[] GetStyleProperties()
	{
		return styleProperties;
	}
	public static PropertyInfo GetGeneratedProperty(string nameCSS)
	{
		if ( nameCSS == "color" ) // Special case
		{
			return styleType.GetProperty( "FontColor", BindingFlags.Instance);
		}

		string name = nameCSS.ToTitleCase().Replace( " ", "" );
		//Log.Info( $"PropName {nameCSS} -> {name}" );

		return styleType.GetProperty( name );
	}
	public static string GetStyleProperty(PropertyInfo prop)
	{
		string name = "";
		char previous = default;
		foreach(char c in prop.Name)
		{
			if(char.IsUpper(c))
			{
				if(previous != default)
					name += '-';
				name += char.ToLower(c);
			}
			else
			{
				name += c;
			}

			previous = c;
		}

		return name;
	}
	public static string GetStyleDefaultValue(PropertyInfo prop)
	{
		string value = "";

		Type type = prop.PropertyType;
		if(type.Name.Contains("Nullable"))
		{
			type = type.GetGenericArguments()[0];
		}

		if ( type == typeof( Length ) )
			return "auto";
		if(prop.Name == "FontFamily")
			return "Arial";

		if( type.IsValueType)
		{
			value = Activator.CreateInstance( type, true ).ToString();
		}

		if ( !styleType.IsAssignableTo(prop.ReflectedType) )
			return value;

		string propValue = prop.GetValue( styleReference )?.ToString();
		value = LowerLetters(propValue);

		//Log.Info( $"StyleDefaultValue {prop.Name} = '{value}'" );
		return value;
	}
	public static T GetDefaultStyle<T>(Panel p, string name)
	{
		if ( p == null ) return default;

		var prop = styleType.GetProperty( name );
		if(prop == null ) return default;

		if( prop.GetValue( p.Style ) is T value)
			return value;

		return default;
	}
	public static T GetDefaultStyle<T>(Panel p, PropertyInfo prop) => GetDefaultStyle<T>( p, prop.Name );
	public static void DirtyStyles(Panel p)
	{
		p.CallMethod( "DirtyStylesRecursive" );
	}
	public static Widget CreateEditor( StyleBlock block, PropertyInfo prop, IStyleBlock.StyleProperty styleProp, Panel panel, bool rawEdit = false )
	{
		Assert.True( prop.CanRead );

		bool readOnly = !prop.CanWrite;

		Type type = prop.PropertyType;
		if ( type.IsGenericType && type.Name.Contains( "Nullable" ) )
		{
			// Stupid check because we cant compare to Nullable<> for some reason...
			type = type.GetGenericArguments()[0];
		}

		string propName = styleProp.Name;
		CustomProperty sp = new( type, () => GetStyle(block, type, propName), (val) => SetStyle(block, type, propName, val, panel) );
		Widget editor = null;

		if(!rawEdit)
		{
			if ( type == typeof( PositionMode ) )
			{
				editor = new PanelPositionModeControl( sp );
			}
			else if ( type == typeof( Align ) )
			{
				editor = new PanelAlignControl( sp );
			}
			else if ( type == typeof( Justify ) )
			{
				editor = new PanelJustifyControl( sp );
			}
			else if ( type == typeof( FlexDirection ) )
			{
				editor = new FlexDirectionControl( sp );
			}
			else if ( type == typeof( BackgroundRepeat ) )
			{
				editor = new BackgroundRepeatControl( sp );
			}
			else if ( type == typeof( TextAlign ) )
			{
				editor = new TextAlignControl( sp );
			}
			else if ( type == typeof( TextOverflow ) )
			{
				editor = new TextOverflowControl( sp );
			}
			else if ( type == typeof( WordBreak ) )
			{
				editor = new WordBreakControl( sp );
			}
			else if ( type == typeof( TextTransform ) )
			{
				editor = new TextTransformControl( sp );
			}
			else if ( type == typeof( OverflowMode ) )
			{
				editor = new OverflowModeControl( sp );
			}
			else if ( type == typeof( MaskMode ) )
			{
				editor = new MaskModeControl( sp );
			}
			else if ( type == typeof( MaskScope ) )
			{
				editor = new MaskScopeControl( sp );
			}
			else if ( type == typeof( Length ) )
			{
				LengthControl control = new();
				control.Bind( sp );
				editor = control;
			}
		}
		else
		{
			var rawEditor = new StringControlWidget( sp );
			editor = rawEditor;
		}

		if(editor == null)
		{
			editor = ControlWidget.Create( sp );
			if ( editor == null )
			{
				editor = new StringControlWidget( sp );
				Log.Warning( $"Created default editor for type {type} - {editor}" );
			}
		}
		

		return editor;
	}

	/// <summary>
	/// Turns letters into lowercase and ignores non-letters
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	private static string LowerLetters( string input )
	{
		if ( string.IsNullOrEmpty( input ) )
			return "";

		string value = "";
		if ( !string.IsNullOrEmpty( input ) )
		{
			foreach ( char c in input )
			{
				if ( char.IsLetter( c ) )
					value += char.ToLower( c );
				else
					value += c;
			}
		}

		return value;
	}
	private static object GetStyle( StyleBlock block, Type type, string name )
	{
		var value = block.GetRawValues().FirstOrDefault( v => v.Name == name ).Value;
		//Log.Info( $"GetStyle {string.Join( ' ', block.SelectorStrings )} [{name} = {ParseStyle( type, value )}]" );

		return ParseStyle( type, value );
	}

	private static void SetStyle(StyleBlock block, Type type, string name, object value, Panel panel )
	{
		//Log.Info( $"SetStyle {string.Join(' ', block.SelectorStrings)} [{name} = {StyleToString( type, value )} ({value})]" );
		block.SetRawValue( name, StyleToString(type, value) );
		DirtyStyles( panel );
	}

	private static object ParseStyle(Type type, string value)
	{
		object r = default;
		if ( type.IsEnum )
		{
			r = Enum.Parse( type, value, true );
		}
		else if ( type == typeof( Length ) )
		{
			r = Length.Parse( value );
		}
		else if(type == typeof(Color))
		{
			r = Color.Parse( value );
		}
		else
		{
			r = Convert.ChangeType( value, type );
		}

		return r;
	}

	private static string StyleToString(Type type, object value)
	{
		if ( value is Color color )
			return color.ToString( true, true );

		return LowerLetters(value.ToString());
	}
}
