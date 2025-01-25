
using Sandbox.Razor;
using Sandbox.UI;
using System.IO;
using System.Linq;

namespace Panelize;

public partial class PanelEditorSession
{
	[EditorEvent.Hotload]
	private static void OnHotload()
	{
		UpdatePanelTypes();
	}
	private static void UpdatePanelTypes()
	{
		panelTypes = EditorTypeLibrary.GetTypes<Panel>().ToList();
	}
	static List<TypeDescription> panelTypes;
	public static PanelEditorSession Current { get; internal set; }
	public string FilePath { get; set; }
	public Panel Panel { get; set; }
	public PanelEditor EditorWidget { get; set; }
	public IReadOnlyList<StyleSheet> PanelSheets => panelSheets.AsReadOnly();
	public bool HasChanges { get; set; }
	private Dictionary<Panel, PanelState> panelStates = new();
	private List<StyleSheet> panelSheets = new();
	private List<Panel> createdPanels = new();
	public PanelEditorSession(PanelEditor editor)
	{
		EditorWidget = editor;
		UpdateWindowTitle();

		if ( panelTypes == null )
			UpdatePanelTypes();
	}
	public Panel New()
	{
		Panel newPanel = new();
		newPanel.ElementName = "newpanel";
		StyleSheet sheet = new();

		newPanel.StyleSheet.Add( sheet );
		panelSheets.Add( sheet );

		Panel = newPanel;
		EditorWidget.RootPanel.DeleteChildren();
		EditorWidget.RootPanel.AddChild( newPanel );

		UpdateWindowTitle();
		return newPanel;
	}
	public Panel LoadPanel( string path )
	{
		if ( string.IsNullOrEmpty( path ) )
		{
			Log.Warning( $"Cant load panel, path is invalid!" );
			return null;
		}

		if ( panelTypes.Count == 0 )
		{
			Log.Warning( $"Cant load panel, no types found!" );
			return null;
		}

		TypeDescription panelType = null;
		foreach ( var type in panelTypes )
		{
			if ( path.ToLower() == GetFullPath( type ).ToLower() )
			{
				panelType = type;
				break;
			}
		}

		if ( panelType == null )
		{
			Log.Warning( "Cant load panel, not found in types!" );
			return null;
		}

		var referencePanel = panelType.Create<Panel>();
		// Render reference manually
		referencePanel.CallMethod( "InternalRenderTree" );
		void MovePanel(Panel reference, Panel parent)
		{
			parent.AddChild( reference );
			if(path.Contains(reference.SourceFile))
				createdPanels.Add(reference);
			if ( reference.HasChildren )
			{
				foreach ( var child in reference.Children )
				{
					MovePanel( child, reference );
				}
			}
		}

		Panel newPanel = new();
		newPanel.StyleSheet = referencePanel.StyleSheet;
		EditorWidget.RootPanel.DeleteChildren( true );
		EditorWidget.RootPanel.AddChild( newPanel );

		createdPanels = [newPanel];
		newPanel.ElementName = referencePanel.ElementName;
		foreach(var child in referencePanel.Children.ToArray() )
			MovePanel(child, newPanel);

		FilePath = path;
		Panel = newPanel;
		panelSheets = newPanel.StyleSheet.GetMember<List<StyleSheet>>( "List" ) ?? new();
		if ( !panelSheets.Any() )
			panelSheets.Add( new() );

		referencePanel.Delete( true );
		Log.Info( $"Load panel {newPanel} ({newPanel.ChildrenCount} children) ({createdPanels.Count} created)" );

		UpdateWindowTitle();
		return referencePanel;
	}
	/// <summary>
	/// Registers a panel as created by this editor.
	/// </summary>
	/// <param name="panel"></param>
	public void AddEditorPanel(Panel panel)
	{
		createdPanels.Add( panel );
	}
	public bool IsEditorPanel(Panel panel)
	{
		return createdPanels?.Contains( panel ) ?? false;
	}
	public List<StyleBlock> GetStyleBlocks(Panel panel)
	{
		List<StyleBlock> blocks = new();
		foreach(var sheet in panelSheets)
		{
			blocks.AddRange(sheet.Nodes.Where( b => b.TestBroadphase(panel) ));
		}

		return blocks;
	}
	public PanelState GetPanelState(Panel panel)
	{
		if ( panelStates.TryGetValue( panel, out var value ) )
			return value;

		return new();
	}
	public void SetPanelState( Panel panel, PanelState state )
	{
		if ( panelStates.ContainsKey( panel ) )
			panelStates[panel] = state;
		else
			panelStates.Add( panel, state );
	}
	private string GetFullPath( TypeDescription type )
	{
		return Editor.FileSystem.Mounted.GetFullPath( type.SourceFile )?.Replace( '\\', '/' ) ?? "";
	}
	public void MakeActive()
	{
		Current = this;
		UpdateWindowTitle();
		BringToFront();
	}
	public void BringToFront()
	{
		MainWindow.Instance.DockManager.RaiseDock( EditorWidget );
	}
	public void UpdateWindowTitle()
	{
		if(Panel.IsValid())
		{
			EditorWidget.WindowTitle = $"Preview - {Panel.ElementName}";
		}
		else if ( !string.IsNullOrEmpty(FilePath))
		{
			EditorWidget.WindowTitle = $"Preview - {FilePath}";
		}
		else
		{
			EditorWidget.WindowTitle = $"Preview - Unnamed";
		}
	}
	public void DockEditorTab()
	{
		if ( !EditorWidget.IsValid() )
			return;

		Widget sibling = MainWindow.Instance.Children.OfType<PanelEditor>().Where(e => e != EditorWidget).FirstOrDefault();
		MainWindow.Instance.DockManager.AddDock( sibling, EditorWidget, DockArea.Inside );
	}

	internal void OnEditorClosed()
	{
		MainWindow.Instance.RemoveEditor( EditorWidget );
	}

	public override int GetHashCode()
	{
		return HashCode.Combine( FilePath, Panel, createdPanels );
	}
}
