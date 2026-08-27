using System;
using System.Collections.Generic;
using ExtremeRoles.Module.CustomOption;
using CustomOpt = ExtremeRoles.Module.CustomOption.Implemented.CustomOption;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.View;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption;

[Collection("UnityMock")]
public class CustomOptionRootTests
{
    public CustomOptionRootTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(ExtremeRolesPlugin.Instance);
    }

    [Fact]
    public void OptionTabContainer_Operations_ShouldWorkCorrectly()
    {
        var container = new OptionTabContainer(OptionTab.GeneralTab);
        Assert.Equal("GeneralTab", container.Name);
        Assert.Equal(0, container.Count);

        var pack = new OptionPack();
        var category = new OptionCategory(OptionTab.GeneralTab, 1, "TestCategory", pack);

        container.AddGroup(category);
        Assert.Equal(1, container.Count);

        Assert.True(container.TryGetCategory(1, out var retrieved));
        Assert.Same(category, retrieved);

        Assert.False(container.TryGetCategory(99, out _));
    }

    [Fact]
    public void OptionPack_Operations_ShouldWorkCorrectly()
    {
        var pack = new OptionPack();
        var mockOpt = new Mock<IOption>();

        pack.AddOption(10, mockOpt.Object);
        Assert.Single(pack.AllOptions);
        Assert.Same(mockOpt.Object, pack.Get(10));
    }

    [Fact]
    public void OptionUpdateRecorder_StartAndRecord_ShouldWorkCorrectly()
    {
        var recorder = new OptionUpdateRecorder();
        var mockOpt = new Mock<IOption>();

        recorder.RegisterRecordOption(1, mockOpt.Object);

        using (var record = recorder.StartRecord())
        {
            mockOpt.Raise(x => x.OnValueChanged += null);

            Assert.True(record.Result.ContainsKey(1));
            Assert.Contains(mockOpt.Object, record.Result[1]);
        }
    }


    [Fact]
    public void ClientOption_ConfigEntries_ShouldReadAndWriteValues()
    {
        ClientOption.Create();
        var clientOpt = ClientOption.Instance;

        clientOpt.GhostsSeeTask.Value = false;
        Assert.False(clientOpt.GhostsSeeTask.Value);

        clientOpt.GhostsSeeTask.Value = true;
        Assert.True(clientOpt.GhostsSeeTask.Value);

        clientOpt.Ip.Value = "192.168.1.1";
        Assert.Equal("192.168.1.1", clientOpt.Ip.Value);
    }

    [Fact]
    public void ConfigBinder_GetAndSet_ShouldWorkCorrectly()
    {
        string uniqueKey = "TestConfigKey_" + Guid.NewGuid().ToString("N");
        var binder = new ConfigBinder(uniqueKey, 42);
        Assert.Equal(42, binder.DefaultValue);
        Assert.Equal(42, binder.Value);

        binder.Value = 100;
        Assert.Equal(100, binder.Value);

        binder.Rebind();
        Assert.Equal(100, binder.Value);
    }

    [Fact]
    public void OptionCategory_And_OptionLoadWrapper_ShouldGetOptionsAndValues()
    {
        var pack = new OptionPack();
        var info = new OptionInfo(10, "Opt10");
        var valHolder = new IntOptionValue(5, 0, 10, 1);
        var opt = new CustomOpt(info, valHolder, new AlwaysActive());

        pack.AddOption(10, opt);

        var category = new OptionCategory(OptionTab.GeneralTab, 1, "CatName", pack, Color.red);
        Assert.Equal("CatName", category.Name);
        Assert.Equal(1, category.Id);
        Assert.Equal(1, category.Count);
        Assert.Equal(Color.red, category.Color);
        Assert.False(category.IsDirty);

        category.IsDirty = true;
        Assert.True(category.IsDirty);

        // Get and GetValue
        Assert.Same(opt, category.Get(10));
        Assert.Equal(5, category.GetValue<int>(10));

        Assert.True(category.TryGet(10, out var foundOpt));
        Assert.Same(opt, foundOpt);

        Assert.True(category.TryGetValue<int>(10, out int val));
        Assert.Equal(5, val);

        Assert.False(category.TryGet(99, out _));
        Assert.False(category.TryGetValue<int>(99, out _));

        // LoadWrapper with offset
        var wrapper = new OptionLoadWrapper(category, 5); // idOffset = 5
        Assert.Same(opt, wrapper.Get(5)); // 5 + 5 = 10
        Assert.Equal(5, wrapper.GetValue<int>(5));

        Assert.True(wrapper.TryGet(5, out var wrapOpt));
        Assert.Same(opt, wrapOpt);

        Assert.True(wrapper.TryGetValue<int>(5, out int wrapVal));
        Assert.Equal(5, wrapVal);

        Assert.False(wrapper.TryGet(99, out _));
        Assert.False(wrapper.TryGetValue<int>(99, out _));
    }

    [Fact]
    public void OptionManager_Operations_ShouldWorkCorrectly()
    {
        var manager = OptionManager.Instance;
        Assert.NotNull(manager);

        Assert.True(manager.TryGetTab(OptionTab.GeneralTab, out var tabContainer));
        Assert.NotNull(tabContainer);

        var pack = new OptionPack();
        var opt = new CustomOpt(new OptionInfo(1, "ChildOpt"), new BoolOptionValue(true), new AlwaysActive());
        pack.AddOption(1, opt);
        var category = new OptionCategory(OptionTab.GeneralTab, 100, "Group100", pack);

        manager.RegisterOptionGroup(OptionTab.GeneralTab, category);

        Assert.True(manager.TryGetCategory(OptionTab.GeneralTab, 100, out var foundCat));
        Assert.Same(category, foundCat);

        // Register child
        var parentOpt = new CustomOpt(new OptionInfo(2, "ParentOpt"), new BoolOptionValue(true), new AlwaysActive());
        manager.RegisterChild(parentOpt, opt);

        Assert.True(manager.TryGetChild(parentOpt, out var children));
        Assert.Contains(opt, children);

        Assert.False(manager.TryGetChild(opt, out _));
    }

    [Fact]
    public void OptionCategoryViewObject_Builder_ShouldBuildCorrectly()
    {
        var builder = new OptionCategoryViewObject<MonoBehaviour>.Builder(null!, 5);
        Assert.Null(builder.Category);
        Assert.Empty(builder.Options);

        var viewObj = builder.Build();
        Assert.NotNull(viewObj);
        Assert.Empty(viewObj.View);
    }
}
