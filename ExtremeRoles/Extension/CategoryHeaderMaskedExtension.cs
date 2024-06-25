namespace ExtremeRoles.Extension.Option;

public static class CategoryHeaderMaskedExtension
{
	public static void ReplaceExRText(this CategoryHeaderMasked masked, string txt, int maskLayer)
	{
		masked.Title.text = txt;
		masked.Background.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		masked.Title.fontMaterial.SetFloat("_StencilComp", 3f);
		masked.Title.fontMaterial.SetFloat("_Stencil", maskLayer);
	}
}
