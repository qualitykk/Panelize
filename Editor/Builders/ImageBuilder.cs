using Sandbox.UI;

namespace Panelize;

public class ImageBuilder : PanelBuilder<Image>
{
	public override string Title => "Image";
	public override string Tag => "img";
	public override Panel Build( Panel parent )
	{
		Image image = (Image)base.Build( parent );
		image.Style.BackgroundColor = Color.Red;

		return image;
	}
}
