global using BTD_Mod_Helper.Extensions;
using MelonLoader;
using BTD_Mod_Helper;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using System.Linq;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Towers;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu.TowerSelectionMenuThemes;
using Il2CppAssets.Scripts.Unity.Bridge;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.Components;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using Newtonsoft.Json.Linq;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppSystem.Collections.Generic;
using HarmonyLib;
using TowerScaler;

[assembly: MelonInfo(typeof(TowerScaler.TowerScaler), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace TowerScaler;

public class TowerScaler : BloonsTD6Mod
{
    public static Dictionary<Tower, float> stacks = new();
    public static float temp = 0;
    public override void OnNewGameModel(GameModel gameModel)
    {
        foreach (var towerModel in gameModel.towers)
        {
            towerModel.towerSelectionMenuThemeId = ModContent.GetId<TowerScalerTSM>();
        }
    }


    public class TowerScalerTSM : ModTsmTheme
    {
        public TSMButton ResetButton { get; private set; } = null!;

        public TSMButton BananaButton { get; private set; } = null!;
        public TSMButton SubtractButton { get; private set; } = null!;

        public ModHelperImage Icon { get; private set; } = null!;
        public ModHelperImage Banana { get; private set; } = null!;
        public ModHelperImage Minus { get; private set; } = null!;
        public override string BaseTheme => "Default";

        public override void SetupTheme(BaseTSMTheme theme)
        {
            ResetButton = theme.gameObject.AddTSMButton(
            new Info(nameof(ResetButton), LeftArrowX, AboveArrowsY, DefaultButtonSize),
            VanillaSprites.GreenBtnSquare, nameof(ResetButton));

            BananaButton = theme.gameObject.AddTSMButton(
            new Info(nameof(BananaButton), RightArrowX, AboveArrowsY, DefaultButtonSize),
            VanillaSprites.GreenBtnSquare, nameof(BananaButton));

            SubtractButton = theme.gameObject.AddTSMButton(
            new Info(nameof(SubtractButton), RightArrowX, AboveArrowsY + 220, DefaultButtonSize),
            VanillaSprites.GreenBtnSquare, nameof(SubtractButton));

            Icon = ResetButton.gameObject.AddImage(new Info(nameof(Icon), DefaultIconSize), "");
            Banana = BananaButton.gameObject.AddImage(new Info(nameof(Banana), DefaultIconSize), "");
            Minus = SubtractButton.gameObject.AddImage(new Info(nameof(Minus), DefaultIconSize), "");

            var reseticon = GetSpriteReference("Reset");
            var Bananaicon = GetSpriteReference("Banana");
            var MinusIcon = GetSpriteReference("Minus");

            Icon.Image.SetSprite(reseticon.AssetGUID);
            Banana.Image.SetSprite(Bananaicon.AssetGUID);
            Minus.Image.SetSprite(MinusIcon.AssetGUID);
        }

        public override void OnButtonPressed(BaseTSMTheme theme, TowerToSimulation tower, string buttonId)
        {
            if (buttonId == nameof(ResetButton))
            {
                if (stacks.ContainsKey(tower.tower))
                {
                    stacks.Remove(tower.tower);
                }
                tower.tower.RemoveMutator<ScaleMutator>();
            }
            else if (buttonId == nameof(BananaButton))
            {
                if (!stacks.ContainsKey(tower.tower))
                {
                    stacks.Add(tower.tower, 1);
                }
                if (stacks[tower.tower] < 1)
                {
                    stacks[tower.tower] /= 0.9f;
                }
                else
                {
                    stacks[tower.tower] += 1f;
                }
                if (tower.tower.IsMutatedBy<ScaleMutator>())
                {
                    tower.tower.RemoveMutator<ScaleMutator>();
                    temp = stacks[tower.tower];
                    tower.tower.AddMutator<ScaleMutator>();
                }
                else
                {
                    temp = stacks[tower.tower];
                    tower.tower.AddMutator<ScaleMutator>();
                }
            }
            else if (buttonId == nameof(SubtractButton))
            {
                if (!stacks.ContainsKey(tower.tower))
                {
                    stacks.Add(tower.tower, 1);
                }
                if (stacks[tower.tower] > 1)
                {
                    stacks[tower.tower] -= 1;
                }
                else
                {
                    stacks[tower.tower] *= 0.9f;
                }
                if (tower.tower.IsMutatedBy<ScaleMutator>())
                {
                    tower.tower.RemoveMutator<ScaleMutator>();
                    temp = stacks[tower.tower];
                    tower.tower.AddMutator<ScaleMutator>();
                }
                else
                {
                    temp = stacks[tower.tower];
                    tower.tower.AddMutator<ScaleMutator>();
                }
            }
        }
    }
    [HarmonyPatch(typeof(Tower), nameof(Tower.OnSold))]
    [HarmonyPatch(typeof(Tower), nameof(Tower.OnDestroy))]
    private static class Tower_OnDestroy
    {
        public static void Postfix(Tower __instance)
        {
            if (stacks.ContainsKey(__instance))
            {
                stacks.Remove(__instance);
            }
        }
    }

    public class ScaleMutator : ModMutator
    {
        public override bool CantBeAbsorbed => true;
        public override bool Saved => true;
        public override int Priority => 5;

        public override bool Mutate(Model baseModel, Model model, Newtonsoft.Json.Linq.JToken data)
        {
            if (!model.Is(out TowerModel tower)) return false;

            tower.displayScale = temp;
            return true;
        }
    }
}