using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;

public class Checkbox : Widget
{
	public bool Value { get; set; }
	public string IconEnabled { get; set; }
	public string IconDisabled { get; set; }
	public Action<bool> OnEdited;
	public Checkbox( Widget parent = null, bool value = false, string iconEnabled = null, string iconDisabled = null, float? size = null) : base(parent)
	{
		Value = value;
		IconEnabled = iconEnabled;
		IconDisabled = iconDisabled;

		Cursor = CursorShape.Finger;
		MinimumWidth = size ?? ControlWidget.ControlRowHeight;
		MinimumHeight = size ?? ControlWidget.ControlRowHeight;
		HorizontalSizeMode = SizeMode.CanShrink;
		VerticalSizeMode = SizeMode.CanShrink;
	}
	protected override void OnMouseClick( MouseEvent e )
	{
		if(e.LeftMouseButton)
		{
			Value = !Value;
			OnEdited?.Invoke(Value);
		}
	}
	protected override void OnPaint()
	{
		Paint.Antialiasing = true;
		Paint.TextAntialiasing = true;

		float alpha = (ReadOnly ? 0.5f : 1f);
		Rect localRect = LocalRect;

		Color color = Theme.Blue;
		Rect rect = localRect.Shrink( 2 );

		Paint.ClearPen();
		Paint.SetBrush( ControlWidget.ControlColor.Lighten( ReadOnly ? 0.5f : 0f ).WithAlphaMultiplied( alpha ) );
		Paint.DrawRect( rect, 2f );

		if(Value)
		{
			Paint.SetPen( color.WithAlpha( 0.3f * alpha ), 1f );
			Paint.SetBrush( color.WithAlpha( 0.2f * alpha ) );

			Paint.DrawRect( rect, 2f );
			Paint.SetPen( color.WithAlphaMultiplied( 0.5f ) );
			Paint.DrawIcon( rect, IconEnabled ?? "done", 13f );
		}
		else if ( IconDisabled != null )
		{
			Paint.SetPen( Theme.Grey.WithAlphaMultiplied( 0.5f ) );
			Paint.DrawIcon( rect, IconDisabled, 13f );
		}

		if ( IsUnderMouse && !ReadOnly )
		{
			Paint.SetPen( color.WithAlpha(0.5f * alpha), 1);
			Paint.ClearBrush();
			Paint.DrawRect( in rect, 1f );
		}
	}
}
