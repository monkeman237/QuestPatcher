﻿using Avalonia.Controls;
using QuestPatcher.Core;
using QuestPatcher.Core.Models;
using QuestPatcher.Core.Patching;
using QuestPatcher.Services;
using QuestPatcher.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QuestPatcher
{
    public class UIPrompter : IUserPrompter
    {
        private Window? _mainWindow;
        private Config? _config;
        private QuestPatcherUIService? _uiService;
        private PatchingManager? _patchingManager;

        /// <summary>
        /// This exists instead of a constructor since the prompter must be immediately passed on QuestPatcherService's creation, so we initialise its members after the fact.
        /// Maybe there's a better workaround, but this works fine for now
        /// </summary>
        public void Init(Window mainWindow, Config config, QuestPatcherUIService uiService, PatchingManager patchingManager)
        {
            _mainWindow = mainWindow;
            _config = config;
            _uiService = uiService;
            _patchingManager = patchingManager;
        }

        public Task<bool> PromptAppNotInstalled()
        {
            Debug.Assert(_config != null);

            DialogBuilder builder = new()
            {
                Title = "App Uninstalled",
                Text = $"The selected app - {_config.AppId} - is not installed",
                HideCancelButton = true
            };
            builder.OkButton.Text = "Close";
            builder.WithButtons(
                new ButtonInfo
                {
                    Text = "Change App",
                    CloseDialogue = true,
                    ReturnValue = false,
                    OnClick = async () =>
                    {
                        Debug.Assert(_uiService != null);
                        await _uiService.OpenChangeAppMenu(true);
                    }
                }
            );

            return builder.OpenDialogue(_mainWindow);
        }

        public Task<bool> PromptAdbDisconnect(DisconnectionType type)
        {
            DialogBuilder builder = new();
            builder.OkButton.Text = "Try Again";

            switch (type)
            {
                case DisconnectionType.NoDevice:
                    builder.Title = "Quest Not Connected";
                    builder.Text = "QuestPatcher could not detect your Quest.\nMake sure that your Quest is plugged in, and that you have setup developer mode as per the SideQuest installation instructions.";
                    builder.WithButtons(
                        new ButtonInfo
                        {
                            Text = "SideQuest Instructions",
                            OnClick = () =>
                            {
                                ProcessStartInfo psi = new()
                                {
                                    FileName = "https://sidequestvr.com/setup-howto",
                                    UseShellExecute = true
                                };
                                Process.Start(psi);
                            }
                        }
                    );
                    break;
                case DisconnectionType.DeviceOffline:
                    builder.Title = "Device Offline";
                    builder.Text = "Your Quest has been detected as offline.\nTry restarting your Quest and your PC";
                    break;
                case DisconnectionType.MultipleDevices:
                    builder.Title = "Multiple Devices Plugged In";
                    builder.Text = "Multiple Android devices are connected to your PC.\nPlease unplug all devices other than your Quest. (and turn off emulators such as BlueStacks)";
                    break;
                case DisconnectionType.Unauthorized:
                    builder.Title = "Device Unauthorized";
                    builder.Text = "Please press allow from this PC within the headset, even if you have done it before for SideQuest.";
                    break;
                default:
                    throw new NotImplementedException($"Variant {type} has no fallback/dialogue box");
            }

            return builder.OpenDialogue(_mainWindow);
        }

        public Task<bool> PromptUnstrippedUnityUnavailable()
        {
            DialogBuilder builder = new()
            {
                Title = "Missing libunity.so",
                Text = "No unstripped libunity.so is available for the the app you have selected. " +
                        "This may mean that certain mods will not work correctly until one is added to the index. " +
                        "Proceed with caution - if you're updating from an older version, it is wise to wait for the latest version of your app to be added."
            };
            builder.OkButton.Text = "Continue Anyway";

            return builder.OpenDialogue(_mainWindow);
        }

        public Task<bool> Prompt32Bit()
        {
            DialogBuilder builder = new()
            {
                Title = "32 bit APK",
                Text = "The app you are attempting to patch is 32 bit (armeabi-v7a). QuestPatcher supports a 32 version of QuestLoader, however most libraries like beatsaber-hook don't, unlesss you use a very old version. " +
                        "This will make modding much more difficult."
            };
            builder.OkButton.Text = "Continue Anyway";

            return builder.OpenDialogue(_mainWindow);
        }

        public Task<bool> PromptPauseBeforeCompile()
        {
            DialogBuilder builder = new()
            {
                Title = "Patching Paused",
                Text = "The APK has been patched and will recompile/reinstall when you continue. Pressing cancel will immediately stop patching"
            };
            builder.OkButton.Text = "Continue";

            return builder.OpenDialogue(_mainWindow);
        }
    }
}
