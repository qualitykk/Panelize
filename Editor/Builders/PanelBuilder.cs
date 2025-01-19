using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;

public abstract class PanelBuilder
{
	/// <summary>
	/// Title of the panel to be displayed in panel list.
	/// </summary>
	public abstract string Title { get; }
	/// <summary>
	/// Tag of the element (eg: div, h1, label)
	/// </summary>
	public abstract string Tag { get; }
	public string Id { get; set; }
	public string PanelClass { get; set; }
	public PositionMode PositionMode { get; set; } = PositionMode.Static;
	public Length Left { get; set; }
	public Length Top { get; set; }
	public Length Width { get; set; }
	public Length Height { get; set; }
	public PanelBuilder WithId(string id)
	{
		Id = id;
		return this;
	}
	public PanelBuilder WithClass(string className)
	{
		PanelClass = className;
		return this;
	}
	public PanelBuilder WithPositionStatic()
	{
		PositionMode = PositionMode.Static;
		return this;
	}
	public PanelBuilder WithPositionRelative()
	{
		PositionMode = PositionMode.Relative;
		return this;
	}
	public PanelBuilder WithPositionAbsolute()
	{
		PositionMode = PositionMode.Absolute;
		return this;
	}
	public PanelBuilder WithPosition(Length left, Length top)
	{
		Left = left;
		Top = top;
		return this;
	}
	public PanelBuilder WithSize(Length width, Length height) 
	{
		Width = width;
		Height = height;
		return this; 
	}

	public abstract Panel Build(Panel parent);
	public string BuildOpeningElement()
	{
		string content = $"<{Tag}";
		content += ElementProperty( "id", Id );
		content += ElementProperty( "class", PanelClass );


		content += " >";
		return content;
	}
	public string BuildClosingElement()
	{
		return $"</{Tag}>";
	}

	private string ElementProperty(string key, object value)
	{
		return $"{key}=\"{value}\" ";
	}
}

public class PanelBuilder<T> : PanelBuilder where T : Panel, new()
{
	public override string Title { get; }

	public override string Tag { get; }
	public PanelBuilder()
	{
		Type t = typeof( T );
		DisplayInfo display = DisplayInfo.ForType( t );
		if(!display.Equals(default))
		{
			Title = display.Name;
			Tag = display.ClassName;
		}
		else
		{
			Title = t.Name;
			Tag = t.Name.ToLower();
		}
	}
	public override Panel Build( Panel parent )
	{
		T panel = parent.AddChild<T>( PanelClass );
		panel.Id = Id;
		panel.Style.Position = PositionMode;

		panel.Style.Left = Left;
		panel.Style.Top = Top;
		panel.Style.Width = Width;
		panel.Style.Height = Height;

		return panel;
	}
}
