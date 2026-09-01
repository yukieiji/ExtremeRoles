using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption.Factory;

public class ValueHolderAssemblerTests
{
	public ValueHolderAssemblerTests()
	{
		MockSetupHelper.SetupMathfHelpers();
	}

	[Fact]
	public void CreateBoolValue_ReturnsBoolOptionValueWithDefaultValue()
	{
		var holderTrue = ValueHolderAssembler.CreateBoolValue(true);
		Assert.IsType<BoolOptionValue>(holderTrue);
		holderTrue.Selection = holderTrue.DefaultIndex;
		Assert.True(((IValue<bool>)holderTrue).Value);

		var holderFalse = ValueHolderAssembler.CreateBoolValue(false);
		holderFalse.Selection = holderFalse.DefaultIndex;
		Assert.False(((IValue<bool>)holderFalse).Value);
	}

	[Fact]
	public void CreateFloatValue_ReturnsFloatOptionValueWithRange()
	{
		var holder = ValueHolderAssembler.CreateFloatValue(1.5f, 0.0f, 5.0f, 0.5f);
		Assert.IsType<FloatOptionValue>(holder);
		holder.Selection = holder.DefaultIndex;
		Assert.Equal(1.5f, ((IValue<float>)holder).Value);
	}

	[Fact]
	public void CreateIntValue_ReturnsIntOptionValueWithRange()
	{
		var holder = ValueHolderAssembler.CreateIntValue(10, 0, 100, 5);
		Assert.IsType<IntOptionValue>(holder);
		holder.Selection = holder.DefaultIndex;
		Assert.Equal(10, ((IValue<int>)holder).Value);
	}

	[Fact]
	public void CreateDynamicIntValue_WhenTempMaxIsZeroAndMinPlusStepLessThanDefault_UsesDefaultValueAsMax()
	{
		var holder = ValueHolderAssembler.CreateDynamicIntValue(10, 0, 1);
		Assert.IsType<IntOptionValue>(holder);
		holder.Selection = holder.DefaultIndex;
		Assert.Equal(10, ((IValue<int>)holder).Value);
	}

	[Fact]
	public void CreateDynamicIntValue_WhenTempMaxIsZeroAndMinPlusStepGreaterThanDefault_UsesMinPlusStepAsMax()
	{
		var holder = ValueHolderAssembler.CreateDynamicIntValue(10, 0, 10);
		Assert.IsType<IntOptionValue>(holder);
		holder.Selection = holder.DefaultIndex;
		Assert.Equal(10, ((IValue<int>)holder).Value);
	}

	[Fact]
	public void CreateDynamicIntValue_WhenTempMaxIsProvided_UsesTempMax()
	{
		var holder = ValueHolderAssembler.CreateDynamicIntValue(10, 0, 1, 50);
		Assert.IsType<IntOptionValue>(holder);
		holder.Selection = holder.DefaultIndex;
		Assert.Equal(10, ((IValue<int>)holder).Value);
	}

	[Fact]
	public void CreateDynamicFloatValue_WhenTempMaxIsZeroAndMinPlusStepLessThanDefault_UsesDefaultValueAsMax()
	{
		var holder = ValueHolderAssembler.CreateDynamicFloatValue(10.0f, 0.0f, 1.0f);
		Assert.IsType<FloatOptionValue>(holder);
		holder.Selection = holder.DefaultIndex;
		Assert.Equal(10.0f, ((IValue<float>)holder).Value);
	}

	[Fact]
	public void CreateDynamicFloatValue_WhenTempMaxIsZeroAndMinPlusStepGreaterThanDefault_UsesMinPlusStepAsMax()
	{
		var holder = ValueHolderAssembler.CreateDynamicFloatValue(10.0f, 0.0f, 10.0f);
		Assert.IsType<FloatOptionValue>(holder);
		holder.Selection = holder.DefaultIndex;
		Assert.Equal(10.0f, ((IValue<float>)holder).Value);
	}

	[Fact]
	public void CreateDynamicFloatValue_WhenTempMaxIsProvided_UsesTempMax()
	{
		var holder = ValueHolderAssembler.CreateDynamicFloatValue(10.0f, 0.0f, 1.0f, 50.0f);
		Assert.IsType<FloatOptionValue>(holder);
		holder.Selection = holder.DefaultIndex;
		Assert.Equal(10.0f, ((IValue<float>)holder).Value);
	}
}
