using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Operator;
using ExtremeRoles.Module.JsonData;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using TMPro;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Compat.Operator;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class OperatorTests
{
	public OperatorTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupCompatModManager();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((StringNames id, Il2CppReferenceArray<Il2CppSystem.Object> parts) => id.ToString());

		var mockPopup = new Mock<GenericPopup>(IntPtr.Zero);
		var mockTmp = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockTmpTransform = new Mock<Transform>(IntPtr.Zero);
		mockTmpTransform.SetupGet(t => t.transform).Returns(mockTmpTransform.Object);
		mockTmpTransform.SetupProperty(t => t.localScale, new Vector3(1f, 1f, 1f));
		mockTmp.SetupGet(t => t.transform).Returns(mockTmpTransform.Object);
		mockPopup.SetupGet(p => p.TextAreaTMP).Returns(mockTmp.Object);

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockPopup.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);
		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.SetupGet(t => t.transform).Returns(mockTransform.Object);
		mockTransform.SetupProperty(t => t.localScale, new Vector3(1f, 1f, 1f));
		mockPopup.SetupGet(p => p.transform).Returns(mockTransform.Object);

		var mockExitButton = new Mock<Transform>(IntPtr.Zero);
		mockExitButton.SetupGet(t => t.transform).Returns(mockExitButton.Object);
		mockExitButton.SetupProperty(t => t.localScale, new Vector3(1f, 1f, 1f));
		mockExitButton.SetupProperty(t => t.localPosition, new Vector3(0f, 0f, 0f));

		var mockExitButtonGo = new Mock<GameObject>(IntPtr.Zero);
		mockExitButton.SetupGet(t => t.gameObject).Returns(mockExitButtonGo.Object);
		mockExitButton.Setup(t => t.GetComponentInChildren<TextTranslatorTMP>()).Returns((TextTranslatorTMP)null!);
		mockExitButton.Setup(t => t.GetComponentInChildren<TextMeshPro>()).Returns(mockTmp.Object);
		var mockButton = new Mock<PassiveButton>(IntPtr.Zero);
		var mockOnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
		mockButton.SetupGet(b => b.OnClick).Returns(mockOnClick);
		mockExitButton.Setup(t => t.GetComponent<PassiveButton>()).Returns(mockButton.Object);

		mockTransform.Setup(t => t.FindChild("ExitGame")).Returns(mockExitButton.Object);

		ExtremeRoles.Module.Prefab.Prop = mockPopup.Object;

		var m7 = new Mock<MockObjectInstantiateHelper7>();
		m7.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>())).Returns((UnityEngine.Object orig) =>
		{
			if (orig is GenericPopup) return mockPopup.Object;
			if (orig is Transform) return mockExitButton.Object;
			return orig;
		});
		MockObjectInstantiateHelper7.Instance = m7.Object;

		var m10 = new Mock<MockObjectInstantiateHelper10>();
		m10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>())).Returns((UnityEngine.Object orig, Transform parent) =>
		{
			if (orig is GenericPopup) return mockPopup.Object;
			if (orig is Transform) return mockExitButton.Object;
			return orig;
		});
		MockObjectInstantiateHelper10.Instance = m10.Object;
	}

	private static string SetupTestModFolder(OperatorBase op)
	{
		string tempDir = Path.Combine(Path.GetTempPath(), "TestPlugins_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(tempDir);

		var modFolderField = typeof(OperatorBase).GetField("ModFolderPath", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		modFolderField?.SetValue(op, tempDir);

		var modDllPathField = op.GetType().GetField("modDllPath", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		if (modDllPathField != null)
		{
			string currentPath = (string)modDllPathField.GetValue(op)!;
			string fileName = Path.GetFileName(currentPath.Replace('\\', '/'));
			modDllPathField.SetValue(op, Path.Combine(tempDir, fileName));
		}

		return tempDir;
	}

	private sealed class TestOperator : OperatorBase
	{
		public string ExposedModFolderPath => this.ModFolderPath;
		public GenericPopup ExposedPopup => this.Popup;

		public void TestShowPopup(string msg) => ShowPopup(msg);
		public void TestSetPopupText(string msg) => SetPopupText(msg);

		public override void Excute() { }
	}

	[Fact]
	public void OperatorBase_Constructor_SetsModFolderPathAndInstantiatesPopup()
	{
		var op = new TestOperator();
		Assert.NotNull(op.ExposedModFolderPath);
		Assert.Contains(@"BepInEx\plugins", op.ExposedModFolderPath);
		Assert.NotNull(op.ExposedPopup);
	}

	[Fact]
	public void OperatorBase_ShowPopup_And_SetPopupText_WorksCorrectly()
	{
		var op = new TestOperator();
		var mockTmp = Mock.Get(op.ExposedPopup.TextAreaTMP);

		op.TestSetPopupText("Test Message");
		mockTmp.VerifySet(t => t.text = "Test Message", Times.AtLeastOnce());

		op.TestShowPopup("Show Message");
		mockTmp.VerifySet(t => t.text = "Show Message", Times.AtLeastOnce());
	}

	[Fact]
	public void ExRAddonInstaller_Constructor_SetsAddonDll()
	{
		var installer = new ExRAddonInstaller(CompatModType.Submerged);
		string addonDll = (string)installer.GetType().GetField("addonDll", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(installer)!;
		Assert.Equal("Submerged.dll", addonDll);
	}

	[Fact]
	public void ExRAddonUninstaller_Excute_FileDoesNotExist_ShowsAlreadyUninstall()
	{
		var uninstaller = new ExRAddonUninstaller(CompatModType.Submerged);
		SetupTestModFolder(uninstaller);

		var mockPopup = Mock.Get((GenericPopup)uninstaller.GetType().GetField("Popup", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(uninstaller)!);

		string modDllPath = (string)uninstaller.GetType().GetField("modDllPath", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(uninstaller)!;
		if (File.Exists(modDllPath)) File.Delete(modDllPath);

		uninstaller.Excute();
		mockPopup.Verify(p => p.Show(It.IsAny<string>()), Times.Once());
	}

	[Fact]
	public void ExRAddonUninstaller_Excute_FileExists_MovesFileAndCleansOldUninstalled()
	{
		var uninstaller = new ExRAddonUninstaller(CompatModType.Submerged);
		string tempDir = SetupTestModFolder(uninstaller);

		string modDllPath = (string)uninstaller.GetType().GetField("modDllPath", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(uninstaller)!;
		File.WriteAllText(modDllPath, "dummy dll content");

		string oldUninstalledFile = Path.Combine(tempDir, "old.uninstalled");
		File.WriteAllText(oldUninstalledFile, "dummy old content");

		try
		{
			uninstaller.Excute();

			Assert.False(File.Exists(modDllPath));
			Assert.True(File.Exists($"{modDllPath}.uninstalled"));
			Assert.False(File.Exists(oldUninstalledFile));
		}
		finally
		{
			if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void Uninstaller_Excute_FileDoesNotExist_ShowsAlreadyUninstall()
	{
		var info = CompatModManager.ModInfo[CompatModType.Submerged];
		var uninstaller = new Uninstaller(info);
		SetupTestModFolder(uninstaller);

		var mockPopup = Mock.Get((GenericPopup)uninstaller.GetType().GetField("Popup", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(uninstaller)!);

		string modDllPath = (string)uninstaller.GetType().GetField("modDllPath", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(uninstaller)!;
		if (File.Exists(modDllPath)) File.Delete(modDllPath);

		uninstaller.Excute();
		mockPopup.Verify(p => p.Show(It.IsAny<string>()), Times.Once());
	}

	[Fact]
	public void Uninstaller_Excute_FileExists_MovesFileAndHandlesReactor()
	{
		var info = CompatModManager.ModInfo[CompatModType.Submerged];
		var uninstaller = new Uninstaller(info);
		string tempDir = SetupTestModFolder(uninstaller);

		string modDllPath = (string)uninstaller.GetType().GetField("modDllPath", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(uninstaller)!;
		File.WriteAllText(modDllPath, "dummy dll content");

		string reactorPath = Path.Combine(tempDir, "Reactor.dll");
		File.WriteAllText(reactorPath, "dummy reactor content");

		try
		{
			uninstaller.Excute();

			Assert.False(File.Exists(modDllPath));
			Assert.True(File.Exists($"{modDllPath}.uninstalled"));
			Assert.False(File.Exists(reactorPath));
			Assert.True(File.Exists($"{reactorPath}.uninstalled"));
		}
		finally
		{
			if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void Installer_Excute_FileAlreadyExists_ShowsAlreadyInstall()
	{
		var info = CompatModManager.ModInfo[CompatModType.Submerged];
		var installer = new Installer(info);
		string tempDir = SetupTestModFolder(installer);

		string dllName = (string)installer.GetType().GetField("dllName", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(installer)!;
		string filePath = Path.Combine(tempDir, dllName);
		File.WriteAllText(filePath, "dummy dll");

		var mockPopup = Mock.Get((GenericPopup)installer.GetType().GetField("Popup", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(installer)!);

		try
		{
			installer.Excute();
			mockPopup.Verify(p => p.Show(It.IsAny<string>()), Times.Once());
		}
		finally
		{
			if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void Installer_Excute_RequiresReactorAndNotInstalled_CreatesConfirmMenu()
	{
		var info = new CompatModInfo("TestMod", "TestGuid", "https://api.github.com/repos/test/test/releases/latest", true, typeof(object));
		var installer = new Installer(info);
		string tempDir = SetupTestModFolder(installer);

		string reactorPath = Path.Combine(tempDir, "Reactor.dll");
		if (File.Exists(reactorPath)) File.Delete(reactorPath);

		try
		{
			installer.Excute();

			var createdPopup = (GenericPopup?)installer.GetType().GetField("popup", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(installer);
			Assert.NotNull(createdPopup);
		}
		finally
		{
			if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void Updater_Excute_FileDoesNotExist_ShowsAlreadyUninstallAfterInstall()
	{
		var info = CompatModManager.ModInfo[CompatModType.Submerged];
		var updater = new Updater(info);
		string tempDir = SetupTestModFolder(updater);

		string dllName = (string)updater.GetType().GetField("dllName", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(updater)!;
		string filePath = Path.Combine(tempDir, dllName);
		if (File.Exists(filePath)) File.Delete(filePath);

		var mockPopup = Mock.Get((GenericPopup)updater.GetType().GetField("Popup", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(updater)!);

		try
		{
			updater.Excute();
			mockPopup.Verify(p => p.Show(It.IsAny<string>()), Times.Once());
		}
		finally
		{
			if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
		}
	}
}
