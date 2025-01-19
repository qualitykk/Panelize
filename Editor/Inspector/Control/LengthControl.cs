using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;

public class LengthControl : Widget
{
	public LengthUnit Unit { get; set; }
	public float Length { get; set; }
	public Length Value
	{
		get
		{
			return new()
			{
				Unit = Unit,
				Value = Length
			};
		}
		set
		{
			if ( value.Unit == LengthUnit.Undefined )
				value.Unit = LengthUnit.Auto;

			SetUnit( value.Unit );
			Length = value.Value;
		}
	}
	public Action<Length> OnValueChanged;
	public float UnitControlWidth { get; set; } = 135f;
	public bool UnitControlWrap { get; } = false;
	public float AmountSliderWidth { get; set; } = 90f;
	DropdownEnumControl<LengthUnit> unitControl;
	LineEdit amountControl;
	FloatSlider amountSlider;
	Layout amountRow;
	Layout controlRow;
	public LengthControl( Widget parent = null, bool unitControlWrap = false ) : base(parent)
	{
		Layout = Layout.Row();
		//Layout.Margin = new( 4f, 0 );
		Layout.Spacing = 4f;
		amountRow = Layout.AddRow();
		controlRow = Layout.AddRow();

		unitControl = new( LengthUnit.Auto )
		{
			FixedWidth = UnitControlWidth
		};
		Unit = LengthUnit.Auto;
		BuildUnitControl();

		amountSlider = new( this )
		{
			Minimum = 0f,
			Maximum = 100f,
			Step = 0.1f,
			MinimumWidth = AmountSliderWidth
		};
		amountSlider.OnValueEdited += AmountSliderChanged;
		amountControl = new( this )
		{
			Text = "0",
			RegexValidator = "[0123456789\\.]*", // Only allow numbers and . (for decimals)
			Alignment = TextFlag.RightCenter
		};
		amountControl.TextChanged += AmountTextChanged;

		if( unitControlWrap )
		{
			controlRow.Add( unitControl );
		}
		else
		{
			amountRow.Add( unitControl );
			amountRow.AddSpacingCell( 4f );
		}

		amountRow.Add( amountSlider, 1 );
		amountRow.AddSpacingCell( 4f );
		amountRow.Add( amountControl, 1 );

		UnitControlWrap = unitControlWrap;
	}

	public void Bind(SerializedProperty prop)
	{
		Assert.True( prop != null && prop.IsEditable );

		var length = prop.GetValue<Length>();
		if( length != null)
		{
			unitControl.SetValue(length.Unit);
			amountControl.Text = length.Value.ToString();
		}

		OnValueChanged += (v) =>
		{
			prop.SetValue( v );
		};
	}
	protected override void DoLayout()
	{
		amountSlider.MinimumWidth = AmountSliderWidth;
		unitControl.FixedWidth = UnitControlWidth;
	}
	private void BuildUnitControl()
	{
		//unitControl.CellColumns = 0;
		unitControl.SetOrder( LengthUnit.Pixels, LengthUnit.Percentage, 
			LengthUnit.ViewHeight, LengthUnit.ViewWidth, LengthUnit.Auto );
		unitControl.SetDisplay( LengthUnit.Pixels, "straighten" );
		unitControl.SetDisplay( LengthUnit.Percentage, "aspect_ratio" );
		unitControl.SetDisplay( LengthUnit.ViewHeight, "swap_vert" );
		unitControl.SetDisplay( LengthUnit.ViewWidth, "swap_horiz" );
		unitControl.SetDisplay( LengthUnit.Auto, "open_in_full" );

		unitControl.OnValueChanged += (unit) => SetUnit(unit, false);
	}
	public void SetUnit( LengthUnit unit, bool manual = true )
	{
		Unit = unit;
		OnValueChanged?.Invoke( Value );
		UpdateValueControl( unit );
		if( manual )
		{
			unitControl.SetValue(unit);
		}
	}

	private void UpdateValueControl(LengthUnit unit)
	{
		amountControl.ReadOnly = false;
		amountControl.Text = Length.ToString();

		if ( unit == LengthUnit.Pixels )
		{
			amountSlider.Hidden = true;
		}
		else if ( unit >= LengthUnit.Percentage && unit <= LengthUnit.ViewMax )
		{
			Length = Length.Clamp( 0f, 100f );
			amountSlider.Hidden = false;

			amountControl.Update();
		}
		else
		{
			amountControl.Text = "Auto";
			amountControl.ReadOnly = true;
			amountSlider.Hidden = true;
		}
	}

	private void AmountTextChanged( string value )
	{
		if ( float.TryParse( value, out float number ) )
		{
			Length = number;
		}
		else
		{
			Length = 0f;
		}

		amountSlider.Value = Length;
		OnValueChanged?.Invoke( Value );
	}

	private void AmountSliderChanged()
	{
		float value = amountSlider.Value;
		Length = value;
		amountControl.Text = value.ToString();
		OnValueChanged?.Invoke( Value );
	}
}
