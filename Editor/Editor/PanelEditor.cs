using Sandbox;
using Sandbox.Internal;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Panelize;

public partial class PanelEditor : SceneRenderingWidget
{
	
	private class PanelRenderObject : SceneCustomObject
	{
		public RootPanel Panel { get; set; }
		public PanelRenderObject( Scene scene, RootPanel panel ) : base( scene.SceneWorld )
		{
			Panel = panel;
		}
		public override void RenderSceneObject()
		{
			Panel.RenderManual();
		}
	}
	public PanelEditorSession Session { get; private set; }
	public TypeDescription SelectedRootType => Session.SelectedRootType;
	public string SelectedRootFile => Session.SelectedRootFile;
	public Panel SelectedPanel => Session.SelectedPanel;
	public RootPanel RootPanel => rootPanel;
	ScreenPanel rootPanelComponent;
	PanelRenderObject rootPanelRenderer;
	RootPanel rootPanel;

	Overlay overlay;
	public PanelEditor()
	{
		MouseTracking = true;
		FocusMode = FocusMode.None;
		AcceptDrops = true;
		//DeleteOnClose = true;

		MinimumWidth = 500f;
		
		overlay = new( this );
		Scene = new();
		using(Scene.Push())
		{
			var sun = new GameObject( true, "sun" ).GetOrAddComponent<DirectionalLight>();
			sun.WorldRotation = Rotation.FromPitch( 50 );
			sun.LightColor = Color.White * 2.5f + Color.Cyan * 0.05f;

			var cubemap = new GameObject( true, "cubemap" ).GetOrAddComponent<EnvmapProbe>();
			cubemap.Texture = Texture.Load( "textures/cubemaps/default2.vtex" );

			Camera = new GameObject(true, "camera").AddComponent<CameraComponent>();

			rootPanelComponent = new GameObject(true, "panel").AddComponent<ScreenPanel>();
			rootPanelComponent.Scale = 10f;
			rootPanelComponent.AutoScreenScale = false;
		}

		rootPanel = (RootPanel)rootPanelComponent.GetPanel();
		rootPanel.Style.Width = Length.Fraction( 1 );
		rootPanel.Style.Height = Length.Fraction( 1 );

		rootPanelRenderer = new( Scene, rootPanel );

		Session = new( this );
		Session.New();
	}
	public override void OnDestroyed()
	{
		rootPanel.Delete();
	}
	public override void PreFrame()
	{
		Scene.GameTick();

		overlay.Visible = Visible;
		overlay.Position = ScreenPosition;
		overlay.Size = Size;
	}

	protected override void OnClosed()
	{
		Session.OnEditorClosed();
	}

	protected override void OnVisibilityChanged( bool visible )
	{
		base.OnVisibilityChanged( visible );

		if ( visible )
		{
			Session.MakeActive();
		}
	}

	internal void OnBuilderDrop(PanelBuilder builder, MouseEvent ev)
	{
		Vector2 position = FromScreen(ev.ScreenPosition);
		Log.Info( $"OnBuilderDrop {builder} {ev.ScreenPosition} {position}" );
		Panel parent = rootPanel.FindInRect( new Rect( position, Vector2.One ), false ).LastOrDefault() ?? rootPanel;
		Panel panel = builder
			.WithPosition( Length.Pixels(position.x).Value, Length.Pixels( position.y ).Value )
			.WithSize( 100f, 100f )
			//.WithPositionAbsolute()
			.Build( parent );

		Log.Info( $"Created panel! {panel.ComputedStyle?.Left}");
	}
	public override void OnDragDrop( DragEvent ev )
	{
		base.OnDragDrop( ev );
		Log.Info( $"OnDragDrop {ev.Data}" );
	}
}
