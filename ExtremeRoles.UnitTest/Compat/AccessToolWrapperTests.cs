using System;
using System.Reflection;
using ExtremeRoles.Compat;
using Xunit;

namespace ExtremeRoles.UnitTest.Compat;

public class AccessToolWrapperTests
{
    private sealed class SampleClass
    {
        public int SampleField = 0;
        public string SampleProperty { get; set; } = string.Empty;
        public void SampleMethod()
        {
        }
    }

    [Fact]
    public void GetTypesFromAssembly_ReturnsTypes()
    {
        var wrapper = new AccessToolWrapper();
        var assembly = typeof(SampleClass).Assembly;

        var types = wrapper.GetTypesFromAssembly(assembly);

        Assert.NotNull(types);
        Assert.Contains(typeof(SampleClass), types);
    }

    [Fact]
    public void GetMethod_ReturnsCorrectMethodInfo()
    {
        var wrapper = new AccessToolWrapper();

        var method = wrapper.GetMethod(typeof(SampleClass), nameof(SampleClass.SampleMethod));

        Assert.NotNull(method);
        Assert.Equal(nameof(SampleClass.SampleMethod), method.Name);
    }

    [Fact]
    public void GetField_ReturnsCorrectFieldInfo()
    {
        var wrapper = new AccessToolWrapper();

        var field = wrapper.GetField(typeof(SampleClass), nameof(SampleClass.SampleField));

        Assert.NotNull(field);
        Assert.Equal(nameof(SampleClass.SampleField), field.Name);
    }

    [Fact]
    public void GetProperty_ReturnsCorrectPropertyInfo()
    {
        var wrapper = new AccessToolWrapper();

        var property = wrapper.GetProperty(typeof(SampleClass), nameof(SampleClass.SampleProperty));

        Assert.NotNull(property);
        Assert.Equal(nameof(SampleClass.SampleProperty), property.Name);
    }
}
