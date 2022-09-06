using Modding;
using GlobalEnums;
using Satchel.BetterMenus;
using System;
using UnityEngine;

namespace MashDash
{
    public class MashDash : Mod, IGlobalSettings<GlobalSetts>, ICustomMenuMod, ITogglableMod
    {
        private float attack_time = 0f;

        private Menu MenuRef;

        private static MashDash? _instance;

        public bool ToggleButtonInsideMenu => true;

        public static GlobalSetts GS { get; set; } = new GlobalSetts();

        new public string GetName() => "Mash Dash";

        public override string GetVersion() => "1.0.0";

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? modToggleDelegates)
        {
            if (MenuRef == null)
            {
                MenuRef = new Menu("Mash Dash", new Element[]
                {
                    Blueprints.CreateToggle(
                        modToggleDelegates.Value,
                        "Mod Enabled",
                        ""
                        ),
                    new HorizontalOption(
                        "Require Dashmaster?",
                        "",
                        new string[] {"NO","YES"},
                        (setting) =>
                        {
                            GS.dashmaster = setting;
                        },
                        () => GS.dashmaster
                        )
                }
                );
            }
            return MenuRef.GetMenuScreen(modListMenu);
        }

        internal static MashDash Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"{nameof(MashDash)} was never initialized");
                }
                return _instance;
            }
        }

        public MashDash() : base()
        {
            _instance = this;
        }

       public override void Initialize()
        {
            Log("Initializing");

            On.HeroController.CanDash += CanDash;
            ModHooks.HeroUpdateHook += AttackTimeUp;
            On.HeroController.ResetAttacks += ResetAttacks;
            On.HeroController.ResetAttacksDash += ResetAttacksDash;

            Log("Initialized");
        }

        private bool CanDash(On.HeroController.orig_CanDash orig, HeroController self)
        {
            if (GS.dashmaster == 1 && !HeroController.instance.playerData.GetBool("equippedCharm_31"))
            {
                return orig(self);
            }
            if (     HeroController.instance.hero_state != ActorStates.no_input
                &&   HeroController.instance.hero_state != ActorStates.hard_landing
                && (!HeroController.instance.cState.attacking || !(attack_time < HeroController.instance.ATTACK_RECOVERY_TIME))
                &&  !HeroController.instance.cState.preventDash
                && ( HeroController.instance.cState.onGround
                    || HeroController.instance.cState.jumping
                    || HeroController.instance.cState.falling
                    || HeroController.instance.cState.doubleJumping
                    || HeroController.instance.cState.bouncing
                    || HeroController.instance.cState.shroomBouncing
                    || HeroController.instance.cState.wallSliding)
                &&  !HeroController.instance.cState.hazardDeath
                &&   HeroController.instance.playerData.GetBool("canDash"))
            {
                return true;
            }

            return false;
        }

        private void AttackTimeUp()
        {
            if (HeroController.instance.hero_state != ActorStates.no_input
                && HeroController.instance.cState.attacking
                && !HeroController.instance.cState.dashing)
            {
                attack_time += Time.deltaTime;
            }
        }

        private void ResetAttacks(On.HeroController.orig_ResetAttacks orig, HeroController self)
        {
            orig(self);
            attack_time = 0f;
        }

        private void ResetAttacksDash(On.HeroController.orig_ResetAttacksDash orig, HeroController self)
        {
            orig(self);
            attack_time = 0f;
        }

        public void Unload()
        {
            On.HeroController.CanDash -= CanDash;
            ModHooks.HeroUpdateHook -= AttackTimeUp;
            On.HeroController.ResetAttacks -= ResetAttacks;
            On.HeroController.ResetAttacksDash -= ResetAttacksDash;
            Log("Unloaded");
        }

        public void OnLoadGlobal(GlobalSetts s) => GS = s;

        public GlobalSetts OnSaveGlobal() => GS;
    }
}

public class GlobalSetts
{
    public int dashmaster = 0;
}
