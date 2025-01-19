using Sandbox.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Facepunch.ActionGraphs.Node;

namespace Panelize;

public class PropertySheetGroup : Widget
{
	public abstract class Entry
	{
		public abstract Widget Build();
	}
	public class PropertyEntry : Entry
	{
		SerializedProperty property;
		public PropertyEntry(SerializedProperty prop)
		{
			property = prop;
		}

		public override Widget Build( )
		{
			var row = PropertySheetRow.Create( property );

			return row;
		}
	}
	public class WidgetBuilderEntry : Entry
	{
		Func<Widget> builder;
		string text;
		bool textColumn;
		public WidgetBuilderEntry(Func<Widget> builder, string label = "", bool labelSeperate = false)
		{
			ArgumentNullException.ThrowIfNull( builder );
			this.builder = builder;
			text = label;
			textColumn = labelSeperate;
		}

		public override Widget Build()
		{
			Widget widget = builder();

			if(!string.IsNullOrEmpty(text))
			{
				Widget body = new();
				body.Layout = textColumn ? Layout.Column() : Layout.Row();

				PropertySheetLabel label = new()
				{
					Text = text
				};
				body.Layout.Add( label );
				body.Layout.Add( widget );

				return body;
			}

			return widget;
		}
	}
	public string Id { get; set; }
	string groupCookie;

	List<Entry> entries = new();
	Widget Body;

	bool hasHeader;
	public PropertySheetGroup( string name )
	{
		if ( !string.IsNullOrWhiteSpace( name ) )
		{
			groupCookie = $"propertysheetgroup.{name}";
		}

		Id = name;
		VerticalSizeMode = SizeMode.Flexible;
		HorizontalSizeMode = SizeMode.Flexible;

		Layout = Layout.Column();
		Layout.Margin = new Margin( 0, 0, 12, 0 );

		bool open = false;
		GroupHeader headerWidget = null;

		if ( groupCookie is not null )
		{
			open = EditorCookie.Get( groupCookie, open );
		}

		visibilityDebounce = 10;

		if ( !string.IsNullOrWhiteSpace( name ) )
		{
			headerWidget = new GroupHeader( this );

			Layout.Add( headerWidget );

			headerWidget.Title = name;
			hasHeader = true;
		}

		Body = new Widget();
		Body.Hidden = true;
		Body.VerticalSizeMode = SizeMode.Flexible;
		Body.HorizontalSizeMode = SizeMode.Flexible;

		Body.Layout = Layout.Column();
		Body.Layout.Margin = new Margin( hasHeader ? 16 : 4, 4, 0, 4 );
		Body.Layout.Spacing = 2;

		Layout.Add( Body );

		if ( hasHeader )
		{
			firstOpen = BuildContents;
			headerWidget.OnToggled += SetVisible;

			if ( open )
				headerWidget.Toggle();
		}
		else
		{
			BuildContents();
			Body.Visible = true;
		}
	}
	Action firstOpen;
	public void AddProperty(SerializedProperty prop, bool createWidget = true)
	{
		var row = PropertySheetRow.Create( prop );
		entries.Add( new PropertyEntry(prop) );
	}
	public void AddProperty(PropertySheetRow row)
	{
		entries.Add( new PropertyEntry(row.Property) );
		Body.Layout.Add( row );
	}
	public Widget AddBuilder(Func<Widget> builder, string label = "")
	{
		ArgumentNullException.ThrowIfNull( builder );

		entries.Add( new WidgetBuilderEntry( builder, label ) );
		Widget widget = builder();
		Body.Layout.Add( widget );
		return widget;
	}
	public void ClearProperties()
	{
		entries.Clear();
	}

	void SetVisible( bool visible )
	{
		using var x = SuspendUpdates.For( Parent );

		firstOpen?.Invoke();
		firstOpen = null;

		Body.Hidden = !visible;

		UpdateGeometry();
		Parent?.UpdateGeometry();

		if ( groupCookie is not null )
		{
			EditorCookie.Set( groupCookie, !visible );
		}
	}

	public void BuildContents()
	{
		Body.Layout.Clear( true );

		foreach ( var entry in entries )
		{
			Widget row = entry.Build();
			if ( row.IsValid() )
			{
				Body.Layout.Add( row );
			}
		}
	}

	RealTimeSince visibilityDebounce = 0;


	[EditorEvent.Frame]
	public void UpdateVisibility()
	{
		if ( Parent is null )
			return;

		if ( visibilityDebounce < 0.2f )
			return;

		visibilityDebounce = Random.Shared.Float( 0, 0.1f );

		Visible = entries.Count > 0;
	}

	protected override void OnPaint()
	{
		base.OnPaint();
		Color outline = Theme.WindowBackground.Lighten( 0.3f );
		if ( hasHeader && !Body.Hidden )
		{
			Paint.SetBrushAndPen( outline );
			{
				var r = LocalRect.Shrink( 8, 4, 0, 0 );
				r.Width = 3;

				Paint.DrawRect( r, 5 );
			}

			{
				var r = LocalRect.Shrink( 10, 0, 0, 0 );
				r.Top = r.Bottom - 3;
				r.Width = 8;

				Paint.DrawRect( r, 5 );
			}
		}

		const float UNDERLINE_HEIGHT = 2f;
		var underline = LocalRect;
		underline.Top += underline.Height - UNDERLINE_HEIGHT;
		underline.Height = UNDERLINE_HEIGHT;

		Paint.SetBrushAndPen( outline );
		Paint.DrawRect( underline );
	}
}

file class GroupHeader : Widget
{
	public GroupHeader( Widget parent ) : base( parent )
	{
		FixedHeight = ControlWidget.ControlRowHeight;
		MinimumWidth = FixedHeight;
		HorizontalSizeMode = SizeMode.Flexible;
		VerticalSizeMode = SizeMode.CanGrow;

		Layout = Layout.Row();
		Layout.Spacing = 5;
		Layout.Margin = new Margin( 16, 0, 0, 0 );
		Layout.AddSpacingCell( 10 );
		Layout.AddStretchCell();
	}

	public string Title { get; set; }
	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );
		if ( e.Button == MouseButtons.Left )
		{
			Toggle();
		}
	}

	protected override void OnDoubleClick( MouseEvent e )
	{
		e.Accepted = false;
	}

	protected override void OnPaint()
	{
		var backgroundRect = LocalRect.Shrink( 3, 4, 4, 4 );
		backgroundRect.Height = 22 - 8;
		//backgroundRect.Right = textRect.Right + 8;
		backgroundRect.Width = backgroundRect.Height;
		//Log.Info( $"{Title} : {LocalRect}" );

		// Background
		{
			Paint.SetBrushAndPen( Theme.WindowBackground.Lighten( 0.2f ).WithAlphaMultiplied( state ? 1 : 0.5f ) );
			Paint.DrawRect( backgroundRect, 6 );
		}

		Paint.ClearBrush();
		Rect iconRect = default;
		if ( state )
		{
			Paint.Pen = Color.White.WithAlpha( 0.2f );
			iconRect = Paint.DrawIcon( backgroundRect, "remove", 12, TextFlag.Center );
		}
		else
		{
			Paint.Pen = Color.White.WithAlpha( 0.4f );
			iconRect = Paint.DrawIcon( backgroundRect, "add", 12, TextFlag.Center );
		}

		Paint.Pen = Theme.ControlText.WithAlpha( state ? 1 : 0.8f );

		Paint.SetFont( "Roboto", 18, 600, sizeInPixels: true);
		var textRect = Paint.MeasureText( LocalRect, Title, TextFlag.LeftCenter );
		float offset = iconRect.Width + 8f;
		textRect.Left += offset;
		textRect.Right += offset;
		Paint.DrawText( textRect, Title, TextFlag.LeftCenter );
	}

	bool state = false;

	public void Toggle()
	{
		state = !state;
		OnToggled?.Invoke( state );

		if ( CookieName is not null )
		{
			ProjectCookie.Set( CookieName, state );
		}
	}

	public Action<bool> OnToggled;

	string _cookieName;

	public string CookieName
	{
		get
		{
			return _cookieName;
		}

		set
		{
			_cookieName = value;

			var newState = ProjectCookie.Get<bool>( _cookieName, state );
			if ( newState == state ) return;

			Toggle();
		}
	}
}
