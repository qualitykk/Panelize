using Sandbox.Diagnostics;
using System.IO;
using System.Linq;

namespace Panelize;

[EditorApp( "Panelize", "preview", "Create and preview UI panels." )]
public partial class MainWindow : DockWindow
{
	public static MainWindow Instance { get; private set; }
	private Editor.ActionGraphs.UndoStack undoStack;
	private Option undoMenuOption;
	private Option redoMenuOption;
	internal List<PanelEditor> editors = new();
	private PanelList list;
	private Properties properties;
	private PanelEditorConfig config;
	public MainWindow()
	{
		if(Instance.IsValid())
		{
			//Instance.Focus();
			//Destroy();
			//return;
			Instance.Destroy();
		}
		Instance = this;
		DeleteOnClose = true;

		Title = "Panelize";
		Size = new Vector2( 1920, 1080f );

		undoStack = new( () => "Serialize" );

		BuildUI();
		Show();
		Focus();
	}
	~MainWindow()
	{
		if(Instance == this || !Instance.IsValid())
			Instance = null;
	}
	private void BuildUI()
	{
		BuildMenuBar();
		RestoreDefaultDockLayout();
	}

	private void BuildMenuBar()
	{
		MenuBar.Clear();

		var file = MenuBar.AddMenu( "File" );
		file.AddOption( "New", "common/new.png", New, "editor.new" ).StatusTip = "New Graph";
		file.AddOption( "Open", "common/open.png", Open, "editor.open" ).StatusTip = "Open Graph";
		file.AddOption( "Save", "common/save.png", Save, "editor.save" ).StatusTip = "Save Graph";
		file.AddOption( "Save As...", "common/save.png", SaveAs, "editor.save-as" ).StatusTip = "Save Graph As...";

		file.AddSeparator();

		file.AddOption( "Quit", null, Quit, "editor.quit" ).StatusTip = "Quit";

		/*
		var edit = MenuBar.AddMenu( "Edit" );
		undoMenuOption = edit.AddOption( "Undo", "undo", Undo, "editor.undo" );
		redoMenuOption = edit.AddOption( "Redo", "redo", Redo, "editor.redo" );
		*/

		/*
		edit.AddSeparator();
		edit.AddOption( "Cut", "common/cut.png", CutSelection, "editor.cut" );
		edit.AddOption( "Copy", "common/copy.png", CopySelection, "editor.copy" );
		edit.AddOption( "Paste", "common/paste.png", PasteSelection, "editor.paste" );
		edit.AddOption( "Select All", "select_all", SelectAll, "editor.select-all" );
		*/

		var view = MenuBar.AddMenu( "View" );
		view.AboutToShow += () => OnViewMenu( view );
	}
	protected override void RestoreDefaultDockLayout()
	{
		PanelEditorSession.Current = null;
		Properties.SelectedObject = null;

		editors.Clear();
		DockManager.Clear();
		DockManager.RegisterDockType( "Inspector", "edit", () => new Properties( this ) );
		DockManager.RegisterDockType( "Panels", "web_asset", () => new PanelList( ) );

		config = new();
		properties = new( this );
		list = new();

		DockManager.AddDock( null, list, DockArea.Left, DockManager.DockProperty.HideOnClose, 0.3f );
		DockManager.AddDock( null, properties, DockArea.Right, DockManager.DockProperty.HideOnClose, 0.8f );
		DockManager.AddDock( properties, config, DockArea.Left, DockManager.DockProperty.HideCloseButton | DockManager.DockProperty.DisallowUserDocking, 0.7f );

		// Add empty editor
		//CreateEditor();
	}

	private void OnViewMenu( Menu view )
	{
		view.Clear();
		view.AddOption( "Restore To Default", "settings_backup_restore", RestoreDefaultDockLayout );
		view.AddSeparator();

		foreach ( var dock in DockManager.DockTypes )
		{
			var o = view.AddOption( dock.Title, dock.Icon );
			o.Checkable = true;
			o.Checked = DockManager.IsDockOpen( dock.Title );
			o.Toggled += ( b ) => DockManager.SetDockState( dock.Title, b );
		}
	}
	public PanelEditor CreateEditor()
	{
		PanelEditor editor = new();
		if ( editors.Count > 0 )
		{
			DockManager.AddDock( editors.First(), editor, DockArea.Inside, DockManager.DockProperty.DisallowFloatWindow );
		}
		else
		{
			DockManager.AddDock( config, editor, DockArea.Inside, DockManager.DockProperty.DisallowFloatWindow );
		}

		editors.Add( editor );
		editor.Session.MakeActive();
		return editor;
	}
	public void RemoveEditor(PanelEditor editor)
	{
		editors.Remove( editor );
		if(editors.Count != 0)
		{
			editors.Last().Session.MakeActive();
		}
	}
	private void New()
	{
		var editor = CreateEditor();
		editor.Session.MakeActive();
	}

	private void Open()
	{
		FileDialog openDialog = new( null )
		{
			Title = "Open Razor Panel",
			DefaultSuffix = $".razor"
		};
		openDialog.SetFindExistingFile();
		openDialog.SetNameFilter( "Razor Files (*.razor)" );

		if ( !openDialog.Execute() )
			return;

		Open( openDialog.SelectedFile );
	}

	private void Open(string path )
	{
		var editor = CreateEditor();
		if ( !editor.IsValid() ) return;

		editor.Session.LoadPanel( path );
		editor.Session.MakeActive();
	}

	private void Save()
	{
		var session = PanelEditorSession.Current;
		if ( string.IsNullOrEmpty( session.SelectedRootFile ))
		{
			SaveAs();
			return;
		}

		Save( session, session.SelectedRootFile );
	}

	private void SaveAs()
	{
		FileDialog saveDialog = new( null )
		{
			Title = "Save Razor Panel",
			DefaultSuffix = $".razor",
		};
		saveDialog.SetNameFilter( "Razor Files (*.razor)" );

		if ( !saveDialog.Execute() )
			return;

		Save( PanelEditorSession.Current, saveDialog.SelectedFile );
	}
	private void Save(PanelEditorSession session, string path)
	{
		Dialog.AskConfirm( () => DoSave( session, path ), $"Do you want to save? Panelize is experimental and might destroy your razor file." );
	}

	private void DoSave( PanelEditorSession session, string path )
	{
		ArgumentNullException.ThrowIfNull( session );
		Assert.False( string.IsNullOrEmpty( path ) );

		// 1) Remove directory from path
		// 2) Remove file extension from path
		string panelName = path.Split( '/', '\\' ).Last()
			.Split( '.' ).First();
		string content = PanelSerializer.SerializeSession( session, true, panelName );

		File.WriteAllText( path, content );
	}

	private void Quit()
	{
		Close();
	}

	private void Redo()
	{
		throw new NotImplementedException();
	}

	private void Undo()
	{
		throw new NotImplementedException();
	}
}
