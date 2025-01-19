using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;

public class BoxSizeControl : Widget
{
	LengthControl topControl;
	LengthControl leftControl;
	LengthControl rightControl;
	LengthControl bottomControl;
	GridLayout editor;
	public BoxSizeControl()
	{
		Layout = Layout.Column();
		editor = Layout.Grid();
		editor.Spacing = 16f;
		Layout.Add(editor);

		SetSizeMode( SizeMode.CanShrink, SizeMode.Default );

		topControl = CreateControl();
		leftControl = CreateControl();
		rightControl = CreateControl();
		bottomControl = CreateControl();

		editor.AddCell( 1, 0, topControl );
		editor.AddCell( 0, 1, leftControl );
		editor.AddCell( 2, 1, rightControl );
		editor.AddCell( 1, 2, bottomControl );
	}

	public void Bind(SerializedProperty topProperty, SerializedProperty leftProperty, SerializedProperty rightProperty, SerializedProperty bottomProperty )
	{
		topControl.Bind( topProperty );
		leftControl.Bind( leftProperty );
		rightControl.Bind( rightProperty );
		bottomControl.Bind( bottomProperty );
	}

	private LengthControl CreateControl()
	{
		LengthControl control = new( this, true )
		{
			MaximumWidth = 110f,
			//AmountControlWidth = 110f,
			//AmountSliderControlWidth = 40f,
			AmountSliderWidth = 70f,
			//UnitControlWidth = 110f
		};

		control.SetUnit( LengthUnit.Auto );
		return control;
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		var rect = editor.GetCellRect( 1, 1 ).Shrink(24f, 8f);

		Paint.SetPen( Theme.Grey );
		Paint.DrawRect( rect );

		Paint.SetPen( Theme.White );

		var topRect = editor.GetCellRect( 0, 1 );
		topRect.Top += topRect.Height + 16f;
		Paint.DrawText( topRect, "Top" );

		var leftRect = editor.GetCellRect( 1, 0 );
		leftRect.Left += leftRect.Width + 32f;
		Paint.DrawText( leftRect, "Left" );

		var rightRect = editor.GetCellRect( 1, 2 );
		rightRect.Left -= rightRect.Width + 32f;
		Paint.DrawText( rightRect, "Right" );

		var bottomRect = editor.GetCellRect( 2, 1 );
		bottomRect.Top -= bottomRect.Height + 16f;
		Paint.DrawText( bottomRect, "Bottom" );
	}
}
