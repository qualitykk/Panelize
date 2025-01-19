using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Label = Sandbox.UI.Label;

namespace Panelize;

public class LabelBuilder : PanelBuilder<Label>
{
	const string PLACEHOLDER_TEXT = "Label";
	public override string Title => "Label";
	public override string Tag => "label";
	public override Panel Build( Panel parent )
	{
		Label label = (Label)base.Build( parent );
		label.Text = PLACEHOLDER_TEXT;

		return label;
	}
}
