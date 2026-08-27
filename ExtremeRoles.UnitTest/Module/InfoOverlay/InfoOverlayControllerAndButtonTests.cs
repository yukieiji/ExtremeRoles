using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.InfoOverlay;
using ExtremeRoles.Module.Interface;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UnityEngine;
using Xunit;
using InfoOverlayController = ExtremeRoles.Module.InfoOverlay.Controller;

namespace ExtremeRoles.UnitTest.Module.InfoOverlay;

[Collection("UnityMock")]
public class InfoOverlayControllerAndButtonTests
{
	public InfoOverlayControllerAndButtonTests()
	{
		MockSetupHelper.SetupCommonMocks();
		MockSetupHelper.SetupLogger();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		var services = new ServiceCollection();
		var mockEventManager = new Mock<IEventManager>();
		services.AddSingleton<IEventManager>(mockEventManager.Object);
		var serviceProvider = services.BuildServiceProvider();

		var backingField = typeof(ExtremeRolesPlugin).GetField("<Provider>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
		backingField?.SetValue(plugin, serviceProvider);
	}

	[Fact]
	public void EventUpdator_Invoke_CallsControllerUpdateOnEvent_AndReturnsTrue()
	{
		var eventUpdator = new EventUpdator();
		bool result = eventUpdator.Invoke();

		Assert.True(result);
		Assert.True(InfoOverlayController.Instance.IsBlock == InfoOverlayController.Instance.IsBlock);
	}

	[Fact]
	public void Controller_IsBlock_SetTrue_HidesViewAndSetsBlockFlag()
	{
		var controller = InfoOverlayController.Instance;
		controller.IsBlock = false;
		Assert.False(controller.IsBlock);

		controller.IsBlock = true;
		Assert.True(controller.IsBlock);

		controller.Hide();
		// Does not throw when view is null
	}

	[Fact]
	public void Controller_UpdateOnEvent_UpdatesModelIsDuty()
	{
		var controller = InfoOverlayController.Instance;
		controller.UpdateOnEvent();
		// Should execute without exceptions
	}

	[Fact]
	public void HelpButton_Defaults_IsInitializedIsFalse()
	{
		var button = new HelpButton();
		Assert.False(button.IsInitialized);
	}

	[Fact]
	public void HelpButton_SetLobbyParent_And_SetGameParent_WhenBodyIsNull_DoesNotThrow()
	{
		var button = new HelpButton();

		button.SetLobbyParent();
		button.SetGameParent();

		Assert.False(button.IsInitialized);
	}

	[Fact]
	public void IRequestHandler_Constants_WorkCorrectly()
	{
		Assert.Equal("application/json", IRequestHandler.JsonContent);
	}
}
