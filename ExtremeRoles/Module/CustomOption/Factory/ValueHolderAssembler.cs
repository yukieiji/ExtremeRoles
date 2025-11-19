using ExtremeRoles.Module.CustomOption.Implemented.Value;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Factory;

public static class ValueHolderAssembler
{
	public static IValueHolder CreateBoolValue(bool defaultValue)
		=> new BoolOptionValue(defaultValue);
	public static FloatOptionValue CreateFloatValue(float defaultValue, float min, float max, float step)
		=> new FloatOptionValue(defaultValue, min, max, step);
	public static IntOptionValue CreateIntValue(int defaultValue, int min, int max, int step)
		=> new IntOptionValue(defaultValue, min, max, step);
	
	public static IntOptionValue CreateDynamicIntValue(int defaultValue, int min, int step, int tempMax=0)
	{
		int max = createMaxValue(min, step, defaultValue, tempMax);
		return CreateIntValue(defaultValue, min, max, step);
	}

	public static FloatOptionValue CreateDynamicFloatValue(float defaultValue, float min, float step, float tempMax = 0.0f)
	{
		float max = createMaxValue(min, step, defaultValue, tempMax);
		return CreateFloatValue(defaultValue, min, max, step);
	}

	private static int createMaxValue(int min, int step, int defaultValue, int tempMaxValue)
		=> tempMaxValue == 0 ?
			min + step < defaultValue ? defaultValue : min + step :
			tempMaxValue;
	private static float createMaxValue(float min, float step, float defaultValue, float tempMaxValue)
		=> tempMaxValue == 0.0f ?
			min + step < defaultValue ? defaultValue : min + step :
			tempMaxValue;
}
