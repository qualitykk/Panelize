using Sandbox.Diagnostics;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;
public abstract class StyleEnumControl<T> : DropdownEnumControl<T> where T : struct, Enum
{
	SerializedProperty property;
	public StyleEnumControl( SerializedProperty prop )
    {
		Assert.True( prop != null);
		Assert.True(prop.PropertyType == typeof( T ) );

		Value = prop.GetValue<T>();
		property = prop;

		if(!prop.IsEditable )
		{
			ReadOnly = true;
		}
	}
	public override void SetValue( T value )
	{
		base.SetValue( value );
		property.SetValue( value );
	}
}

public class PanelPositionModeControl : StyleEnumControl<PositionMode>
{
	public PanelPositionModeControl( SerializedProperty prop ) : base( prop )
	{
		SetDisplay( PositionMode.Static, "fullscreen" );
		SetDisplay( PositionMode.Absolute, "straighten" );
		SetDisplay( PositionMode.Relative, "picture_in_picture" );

		//BuildOptions();
	}
}

public class PanelAlignControl : StyleEnumControl<Align>
{
	public PanelAlignControl( SerializedProperty prop ) : base( prop )
	{
		SetOrder(
			Align.FlexStart, Align.Center, Align.FlexEnd,
			Align.SpaceBetween, Align.SpaceAround, Align.SpaceEvenly,
			Align.Stretch, Align.Baseline, Align.Auto
		);

		SetDisplay( Align.Auto, "autorenew" );
		SetDisplay( Align.FlexStart, "first_page" );
		SetDisplay( Align.Center, "control_camera" );
		SetDisplay( Align.FlexEnd, "last_page" );
		SetDisplay( Align.Stretch, "fullscreen" );
		SetDisplay( Align.Baseline, "picture_in_picture" );
		SetDisplay( Align.SpaceBetween, "straighten" );
		SetDisplay( Align.SpaceAround, "space_bar" );
		SetDisplay( Align.SpaceEvenly, "calendar_view_week" );

		//BuildOptions();
	}
}

public class PanelJustifyControl : StyleEnumControl<Justify>
{
	public PanelJustifyControl( SerializedProperty prop ) : base( prop )
	{
		SetDisplay( Justify.FlexStart, "first_page" );
		SetDisplay( Justify.Center, "control_camera" );
		SetDisplay( Justify.FlexEnd, "last_page" );
		SetDisplay( Justify.SpaceBetween, "straighten" );
		SetDisplay( Justify.SpaceAround, "space_bar" );
		SetDisplay( Justify.SpaceEvenly, "calendar_view_week" );

		//BuildOptions();
	}
}

public class FlexDirectionControl : StyleEnumControl<FlexDirection>
{
	public FlexDirectionControl( SerializedProperty prop ) : base( prop )
	{
		//CellColumns = 2;

		SetOrder(
			FlexDirection.Row, FlexDirection.RowReverse,
			FlexDirection.Column, FlexDirection.ColumnReverse
		);
		SetDisplay( FlexDirection.Column, "south" );
		SetDisplay( FlexDirection.ColumnReverse, "north" );
		SetDisplay( FlexDirection.Row, "east" );
		SetDisplay( FlexDirection.RowReverse, "west" );

		//BuildOptions();
	}
}

public class BackgroundRepeatControl : StyleEnumControl<BackgroundRepeat>
{
	public BackgroundRepeatControl( SerializedProperty prop ) : base( prop )
	{

		SetOrder(
			BackgroundRepeat.NoRepeat,
			BackgroundRepeat.Repeat, 
			BackgroundRepeat.RepeatX,
			BackgroundRepeat.RepeatY, 
			BackgroundRepeat.Clamp
		);
		SetDisplay( BackgroundRepeat.NoRepeat, "sync_disabled" );
		SetDisplay( BackgroundRepeat.Repeat, "sync" );
		SetDisplay( BackgroundRepeat.RepeatX, "horizontal_split" );
		SetDisplay( BackgroundRepeat.RepeatY, "vertical_split" );
		SetDisplay( BackgroundRepeat.Clamp, "crop" );
	}
}
#region Text
public class TextAlignControl : StyleEnumControl<TextAlign>
{
	public TextAlignControl( SerializedProperty prop ) : base( prop )
	{
		SetDisplay( TextAlign.Auto, "format_align_justify" );
		SetDisplay( TextAlign.Left, "format_align_left" );
		SetDisplay( TextAlign.Center, "format_align_center" );
		SetDisplay( TextAlign.Right, "format_align_right" );
	}
}

public class TextOverflowControl : StyleEnumControl<TextOverflow>
{
	public TextOverflowControl( SerializedProperty prop ) : base( prop )
	{
		SetDisplay( TextOverflow.None, "start" );
		SetDisplay( TextOverflow.Ellipsis, "blur_linear" );
		SetDisplay( TextOverflow.Clip, "gradient" );
	}
}

public class WordBreakControl : StyleEnumControl<WordBreak>
{
	public WordBreakControl( SerializedProperty prop ) : base( prop )
	{
		SetDisplay( WordBreak.Normal, "notes" );
		SetDisplay( WordBreak.BreakAll, "wrap_text" );
	}
}

public class TextTransformControl : StyleEnumControl<TextTransform>
{
	public TextTransformControl( SerializedProperty prop ) : base( prop )
	{
		SetDisplay( TextTransform.None, "not_interested" );
		SetDisplay( TextTransform.Uppercase, "format_color_text" );
		SetDisplay( TextTransform.Lowercase, "text_format" );
		SetDisplay( TextTransform.Capitalize, "text_fields" );
	}
}
#endregion

public class OverflowModeControl : StyleEnumControl<OverflowMode>
{
	public OverflowModeControl( SerializedProperty prop ) : base( prop )
	{
		SetDisplay( OverflowMode.Visible, "visibility" );
		SetDisplay( OverflowMode.Hidden, "visibility_off" );
		SetDisplay( OverflowMode.Scroll, "expand" );
	}
}

public class MaskModeControl : StyleEnumControl<MaskMode>
{
	public MaskModeControl( SerializedProperty prop ) : base( prop )
	{
		SetDisplay( MaskMode.MatchSource, "sync" );
		SetDisplay( MaskMode.Alpha, "gradient" );
		SetDisplay( MaskMode.Luminance, "contrast" );
	}
}

public class MaskScopeControl : StyleEnumControl<MaskScope>
{
	public MaskScopeControl( SerializedProperty prop ) : base( prop )
	{
		SetDisplay(MaskScope.Default, "info" );
		SetDisplay(MaskScope.Filter, "filter" );
	}
}
