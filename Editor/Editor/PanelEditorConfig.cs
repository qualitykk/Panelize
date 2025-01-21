using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;

public class PanelEditorConfig : Widget
{
	Layout Tutorial;
	public PanelEditorConfig()
	{
		WindowTitle = "Preview Settings";
		SetWindowIcon( "tune" );

		Layout = Layout.Column();
		Tutorial = Layout.AddColumn();
		Tutorial.Margin = 4f;
		Tutorial.Alignment = TextFlag.LeftTop;
		
		Rebuild();
	}

	private void Rebuild()
	{
		Tutorial.Clear(true);

		Tutorial.Add(new Label.Title( "How to use" ), 0);
		Label tutorialText = new( @"
			1) Create new panel with File -> New or open existing panel with File -> Open (Select existing .razor file)
			2) Select elements on the left side, add new element with right click on existing element -> Create child
			3) Add style by selecting panel -> 'Styles' -> Add rule (same as css)
			4) Save file with File -> Save (This might break complex files, use at your own risk!)

			Opening files which use C# mixed with html will break upon saving.
		" );
		Tutorial.Add( tutorialText, 0 );

		Tutorial.Add( new Label.Title( "Common Issues" ), 0 );
		Label issuesText = new( @"
			Panel in preview is a smaller than the preview window -> Open your game preview window and set the size to '1920x1080' (Sbox issue)

			Panel pseudoclass styles dont work -> Not supported yet, sbox issue

			For other issues, feel free to ping me on the sbox discord. (@qualitykk)
		" );
		Tutorial.Add( issuesText, 0 );
	}
}
