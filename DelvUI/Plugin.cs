﻿using System;
using System.IO;
using System.Reflection;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using ImGuiNET;
using DelvUI.Interface;
using FFXIVClientStructs;

namespace DelvUI {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public string Name => "DelvUI";

        private DalamudPluginInterface _pluginInterface;
        private PluginConfiguration _pluginConfiguration;
        private HudWindow _hudWindow;
        private ConfigurationWindow _configurationWindow;

        private bool _fontBuilt;
        private bool _fontLoadFailed;
        
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            _pluginInterface = pluginInterface;
            _pluginConfiguration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
            _pluginConfiguration.Init(_pluginInterface);
            _configurationWindow = new ConfigurationWindow(_pluginConfiguration);

            _pluginInterface.CommandManager.AddHandler("/pdelvui", new CommandInfo(PluginCommand)
            {
                HelpMessage = "Opens the DelvUI configuration window.", 
                ShowInHelp = true
            });

            _pluginInterface.UiBuilder.OnBuildUi += Draw;
            _pluginInterface.UiBuilder.OnBuildFonts += BuildFont;
            _pluginInterface.UiBuilder.OnOpenConfigUi += BuildBanner;
            _pluginInterface.UiBuilder.OnOpenConfigUi += OpenConfigUi;
            if (!_fontBuilt && !_fontLoadFailed) {
                _pluginInterface.UiBuilder.RebuildFonts();
            }

            Resolver.Initialize();
        }
        
        private void BuildFont() {
            var fontFile = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Fonts", "big-noodle-too.ttf");
            _fontBuilt = false;
            
            if (File.Exists(fontFile)) {
                try {
                    _pluginConfiguration.BigNoodleTooFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontFile, 24);
                    _fontBuilt = true;
                } catch (Exception ex) {
                    PluginLog.Log($"Font failed to load. {fontFile}");
                    PluginLog.Log(ex.ToString());
                    _fontLoadFailed = true;
                }
            } else {
                PluginLog.Log($"Font doesn't exist. {fontFile}");
                _fontLoadFailed = true;
            }
        }

        private void BuildBanner(object sender, EventArgs eventArgs)
        {
            var bannerImage = Path.Combine(Path.GetDirectoryName(AssemblyLocation) ?? "", "Media", "Images", "banner_short_x150.png");

            if (File.Exists(bannerImage)) {
                try {
                    _pluginConfiguration.BannerImage = _pluginInterface.UiBuilder.LoadImage(bannerImage);
                } catch (Exception ex) {
                    PluginLog.Log($"Image failed to load. {bannerImage}");
                    PluginLog.Log(ex.ToString());
                }
            } else {
                PluginLog.Log($"Image doesn't exist. {bannerImage}");
            }


        }
        private void PluginCommand(string command, string arguments) {
            _configurationWindow.IsVisible = !_configurationWindow.IsVisible;
        }

        private void Draw() {
            
            var hudState = _pluginInterface.ClientState.Condition[ConditionFlag.WatchingCutscene]
                             || _pluginInterface.ClientState.Condition[ConditionFlag.WatchingCutscene78]
                             || _pluginInterface.ClientState.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                             || _pluginInterface.ClientState.Condition[ConditionFlag.CreatingCharacter]
                             || _pluginInterface.ClientState.Condition[ConditionFlag.BetweenAreas]
                             || _pluginInterface.ClientState.Condition[ConditionFlag.BetweenAreas51];

            _pluginInterface.UiBuilder.OverrideGameCursor = false;
            
            _configurationWindow.Draw();

            if (_hudWindow?.JobId != _pluginInterface.ClientState.LocalPlayer?.ClassJob.Id) {
                SwapJobs();
            }

            if (_fontBuilt) {
                ImGui.PushFont(_pluginConfiguration.BigNoodleTooFont);
            }
            

            if (!hudState) { 
                _hudWindow?.Draw();
            }

            if (_fontBuilt) {
                ImGui.PopFont();
            }
        }

        private void SwapJobs() {
            _hudWindow = _pluginInterface.ClientState.LocalPlayer?.ClassJob.Id switch
            {
                //Tanks
                Jobs.DRK => new DarkKnightHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.GNB => new GunbreakerHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.WAR => new WarriorHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.PLD => new PaladinHudWindow(_pluginInterface, _pluginConfiguration),

                //Healers
                Jobs.WHM => new WhiteMageHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.SCH => new ScholarHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.AST => new AstrologianHudWindow(_pluginInterface, _pluginConfiguration),


                //Melee DPS
                Jobs.DRG => new DragoonHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.SAM => new SamuraiHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.MNK => new MonkHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.NIN => new NinjaHudWindow(_pluginInterface, _pluginConfiguration),

                //Ranged DPS
                Jobs.BRD => new BardHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.DNC => new DancerHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.MCH => new MachinistHudWindow(_pluginInterface, _pluginConfiguration),
                
                //Caster DPS
                Jobs.RDM => new RedMageHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.SMN => new SummonerHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.BLM => new BlackMageHudWindow(_pluginInterface, _pluginConfiguration),

                //Low jobs
                Jobs.MRD => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.GLD => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.CNJ => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.PGL => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.LNC => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.ROG => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.ARC => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.THM => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.ACN => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                
                //Hand
                Jobs.CRP => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.BSM => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.ARM => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.GSM => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.LTW => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.WVR => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.ALC => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.CUL => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                
                //Land
                Jobs.MIN => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.BOT => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                Jobs.FSH => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                
                //dont have packs yet
                Jobs.BLU => new UnitFrameOnlyHudWindow(_pluginInterface, _pluginConfiguration),
                _ => _hudWindow
            };
        }
        
        private void OpenConfigUi(object sender, EventArgs e) {
            _configurationWindow.IsVisible = !_configurationWindow.IsVisible;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }

            _configurationWindow.IsVisible = false;

            if (_hudWindow != null) {
                _hudWindow.IsVisible = false;
            }

            _pluginInterface.CommandManager.RemoveHandler("/pdelvui");
            _pluginInterface.UiBuilder.OnBuildUi -= Draw;
            _pluginInterface.UiBuilder.OnBuildFonts -= BuildFont;
            _pluginInterface.UiBuilder.OnOpenConfigUi -= BuildBanner;
            _pluginInterface.UiBuilder.OnOpenConfigUi -= OpenConfigUi;
            _pluginInterface.UiBuilder.RebuildFonts();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
