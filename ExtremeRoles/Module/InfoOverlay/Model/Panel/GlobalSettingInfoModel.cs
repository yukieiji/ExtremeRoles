using System;
using System.Text;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.GameMode.Option.ShipGlobal;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class GlobalSettingInfoModel : IInfoOverlayPanelModel
{
				public string Title => Translation.GetString("yourGhostRole");

				public (string, string) GetInfoText()
				{
								StringBuilder printOption = new StringBuilder();

								foreach (OptionCreator.CommonOptionKey key in Enum.GetValues(
												typeof(OptionCreator.CommonOptionKey)))
								{
												if (key == OptionCreator.CommonOptionKey.PresetSelection) { continue; }

												addOptionString(ref printOption, key);
								}

								foreach (GlobalOption key in Enum.GetValues(typeof(GlobalOption)))
								{
												addOptionString(ref printOption, key);
								}

								return (
												$"<size=125%>{Translation.GetString("vanilaOptions")}</size>\n{IGameOptionsExtensions.SettingsStringBuilder.ToString()}",
												$"<size=125%>{Translation.GetString("gameOption")}</size>\n{printOption}"
								);
				}

				private static void addOptionString<T>(
								ref StringBuilder builder, T optionKey) where T : struct, IConvertible
				{
								if (!OptionManager.Instance.TryGetIOption(
																Convert.ToInt32(optionKey), out IOptionInfo? option) ||
												option is null ||
												option.IsHidden)
								{
												return;
								}

								string optStr = option.ToHudString();
								if (optStr != string.Empty)
								{
												builder.AppendLine(optStr);
								}
				}
}
