using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;

public partial class PanelEditor
{
	private class Overlay : Widget
	{
		const float REDRAW_TIME = 0.1f;
		RealTimeSince timeSinceNeededRedraw = 0f;
		PanelEditor editor;
		public Overlay( PanelEditor editor ) : base( editor )
		{
			TranslucentBackground = true;
			NoSystemBackground = true;
			IsFramelessWindow = true;
			this.editor = editor;
		}

		protected override void OnPaint()
		{
			editor.PaintOverlay();
		}

		[EditorEvent.Frame]
		public void Frame()
		{
			if ( timeSinceNeededRedraw > REDRAW_TIME )
			{
				Update();
			}
		}

		public void Redraw()
		{
			timeSinceNeededRedraw = (RealTimeSince)0.0f;
		}
	}

	internal void PaintOverlay()
	{
		if ( Properties.SelectedObject is Panel p )
		{
			//Log.Info( p );
			Paint.Scale( 1.0f / DpiScale, 1.0f / DpiScale );

			DrawPanelHighlight( p, Color.Magenta );
		}
	}

	void DrawBoxSize( Rect inner, Rect outer, Rect rect, Color color )
	{
		var pos = outer.TopLeft;
		pos.y -= 20;

		if ( pos.x < 4 ) pos.x = 4;
		if ( pos.y < 4 ) pos.y = 4;

		var margin = "";
		if ( outer != rect )
		{
			margin = $" margin[{rect.Left - outer.Left:n0},{rect.Top - outer.Top:n0},{outer.Right - rect.Right:n0},{outer.Bottom - rect.Bottom:n0}]";
		}

		var padding = "";
		if ( inner != rect )
		{
			padding = $" padding[{inner.Left - rect.Left:n0},{inner.Top - rect.Top:n0},{rect.Right - inner.Right:n0},{rect.Bottom - inner.Bottom:n0}]";
		}

		Paint.SetBrush( color.WithAlpha( 0.9f ) );
		Paint.ClearPen();

		//Paint.DrawTextBox( new Rect( pos, new Vector2( 1000, 32 ) ), $"{rect.Width:n0}x{rect.Height:n0}{margin}{padding}", Color.Black, new Sandbox.UI.Margin( 5.0f, 2.0f ), 0, TextFlag.LeftTop );
		Paint.ClearBrush();
	}

	void DrawPanelHighlight( Panel panel, Color color )
	{
		if ( !panel.IsValid() || !panel.IsVisible )
			return;

		/*
		Paint.SetPen( color.WithAlpha( 0.8f ), 2.0f, PenStyle.Dash );
		Paint.DrawRect( panel.Box.RectInner.Shrink( 0, 0, 1, 1 ) );
		*/

		Paint.SetPen( color.WithAlpha( 0.8f ), 2.0f );
		Paint.DrawRect( panel.Box.RectOuter.Shrink( 0, 0, 1, 1 ) );

		Paint.SetPen( color.WithAlpha( 0.8f ), 2.0f, PenStyle.Dot );
		Paint.DrawRect( panel.Box.Rect.Shrink( 0, 0, 1, 1 ) );

		DrawBoxSize( panel.Box.RectInner, panel.Box.RectOuter, panel.Box.Rect, color );
	}
}
