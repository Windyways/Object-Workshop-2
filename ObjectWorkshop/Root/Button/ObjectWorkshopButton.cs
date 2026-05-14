using System.Globalization;
using TownOfUs.Modifiers;
using UnityEngine;

namespace TownOfUs.Buttons;

[MiraIgnore]
public abstract class ObjectWorkshopButton : CustomActionButton
{
    public PlayerControl Player => PlayerControl.LocalPlayer;

    public override string Name => string.Empty;

    public static float MapCooldown =>
        OptionGroupSingleton<TownOfUsMapOptions>.Instance.GetMapBasedCooldownDifference();

    public override float InitialCooldown => 10;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override string CooldownTimerFormatString => "0";

    public virtual bool UsableInDeath => false;
    public virtual bool ShouldPauseInVent => true;

    public virtual bool DecreaseCharge => true;

    public PassiveButton PassiveComp { get; set; }

    public virtual int ConsoleBind()
    {
        var bind = -1;
        if (Keybind == Keybinds.PrimaryAction)
        {
            bind = Keybinds.PrimaryConsole;
        }
        else if (Keybind == Keybinds.SecondaryAction)
        {
            bind = Keybinds.SecondaryConsole;
        }
        else if (Keybind == Keybinds.ModifierAction)
        {
            bind = Keybinds.ModifierConsole;
        }
        else if (Keybind == Keybinds.VentAction)
        {
            bind = Keybinds.VentConsole;
        }

        return bind;
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        if (Timer >= 0)
        {
            if (!TimerPaused && (!(ShouldPauseInVent && PlayerControl.LocalPlayer.inVent) || EffectActive))
            {
                Timer -= Time.deltaTime;
            }
        }
        else if (HasEffect && EffectActive)
        {
            EffectActive = false;
            Timer = Cooldown;
            OnEffectEnd();
        }

        if (Button)
        {
            if (CanUse())
            {
                Button!.SetEnabled();
            }
            else
            {
                Button!.SetDisabled();
            }

            if (EffectActive)
            {
                Button.SetFillUp(Timer, EffectDuration);

                Button.cooldownTimerText.text =
                    Timer.ToString(CooldownTimerFormatString, NumberFormatInfo.InvariantInfo);
                Button.cooldownTimerText.gameObject.SetActive(true);
            }
            else
            {
                Button.SetCooldownFormat(Timer, Cooldown, CooldownTimerFormatString);
            }
        }

        FixedUpdate(playerControl);
    }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        Button?.ToggleVisible(visible && Enabled(role) && !role.Player.HasDied());
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (Button == null)
        {
            Logger<OWPlugin>.Error($"Button is null for {GetType().FullName}");
            return;
        }

        Button.usesRemainingSprite.sprite = TouAssets.AbilityCounterBasicSprite.LoadAsset();

        
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button.usesRemainingSprite.color = TextOutlineColor;
        }

        PassiveComp = Button.GetComponent<PassiveButton>();
    }

    public override void SetUses(int amount)
    {
        base.SetUses(amount);
        
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button!.usesRemainingSprite.color = TextOutlineColor;
        }
    }

    public override bool CanUse()
    {
        if (PlayerControl.LocalPlayer == null)
            return false;
        
        if (PlayerControl.LocalPlayer.HasDied() && !UsableInDeath)
            return false;

        if (!PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
            return false;

        if (!PlayerControl.LocalPlayer.AbilityUsable())
            return false;

        return base.CanUse();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        Button?.gameObject.SetActive(HudManager.Instance.UseButton.isActiveAndEnabled ||
                                     HudManager.Instance.PetButton.isActiveAndEnabled);
    }

    public override void ClickHandler()
    {
        if (!CanClick())
            return;

        if (LimitedUses)
        {
            if (DecreaseCharge) UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);
            
            if (TextOutlineColor != Color.clear)
            {
                SetTextOutline(TextOutlineColor);
                if (Button != null)
                {
                    Button.usesRemainingSprite.color = TextOutlineColor;
                }
            }
        }

        OnClick();

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }
}

[MiraIgnore]
public abstract class ObjectWorkshopTargetButton<T> : CustomActionButton<T> where T : MonoBehaviour
{
    public override string Name => string.Empty;

    public static float MapCooldown =>
        OptionGroupSingleton<TownOfUsMapOptions>.Instance.GetMapBasedCooldownDifference();

    public override float InitialCooldown => 10;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override string CooldownTimerFormatString => "0";

    public virtual bool ShouldPauseInVent => true;
    public virtual bool UsableInDeath => false;

    private PassiveButton PassiveComp { get; set; }


    public virtual bool DecreaseCharge => true;
    public virtual int ConsoleBind()
    {
        var bind = -1;
        if (Keybind == Keybinds.PrimaryAction)
        {
            bind = Keybinds.PrimaryConsole;
        }
        else if (Keybind == Keybinds.SecondaryAction)
        {
            bind = Keybinds.SecondaryConsole;
        }
        else if (Keybind == Keybinds.ModifierAction)
        {
            bind = Keybinds.ModifierConsole;
        }
        else if (Keybind == Keybinds.VentAction)
        {
            bind = Keybinds.VentConsole;
        }

        return bind;
    }

    public override void FixedUpdateHandler(PlayerControl playerControl)
    {
        if (Timer >= 0)
        {
            if (!TimerPaused && (!(ShouldPauseInVent && PlayerControl.LocalPlayer.inVent) || EffectActive))
            {
               Timer -= Time.deltaTime;
            }
        }
        else if (HasEffect && EffectActive)
        {
            EffectActive = false;
            Timer = Cooldown;
            OnEffectEnd();
        }

        if (Button)
        {
            if (CanUse())
            {
                Button!.SetEnabled();
            }
            else
            {
                Button!.SetDisabled();
            }

            if (EffectActive)
            {
                Button.SetFillUp(Timer, EffectDuration);

                Button.cooldownTimerText.text =
                    Timer.ToString(CooldownTimerFormatString, NumberFormatInfo.InvariantInfo);
                Button.cooldownTimerText.gameObject.SetActive(true);
            }
            else
            {
                Button.SetCooldownFormat(Timer, Cooldown, CooldownTimerFormatString);
            }
        }

        FixedUpdate(playerControl);
    }

    public override void SetActive(bool visible, RoleBehaviour role)
    {
        if (UsableInDeath) Button?.ToggleVisible(visible && Enabled(role));
        else Button?.ToggleVisible(visible && Enabled(role) && !role.Player.HasDied());
    }

    public override bool CanUse()
    {
        if (PlayerControl.LocalPlayer.HasDied() && !UsableInDeath)
        {
            return false;
        }
        
        if (!PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return base.CanUse();
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (Button == null)
        {
            Logger<OWPlugin>.Error($"Button is null for {GetType().FullName}");
            return;
        }

        switch (typeof(T))
        {
            case Type t when t == typeof(Vent):
                Button.usesRemainingSprite.sprite = TouAssets.AbilityCounterVentSprite.LoadAsset();
                break;
            case Type t when t == typeof(DeadBody):
                Button.usesRemainingSprite.sprite = TouAssets.AbilityCounterBodySprite.LoadAsset();
                break;
            case Type t when t == typeof(PlayerControl):
                Button.usesRemainingSprite.sprite = TouAssets.AbilityCounterPlayerSprite.LoadAsset();
                break;
            default:
                Button.usesRemainingSprite.sprite = TouAssets.AbilityCounterBasicSprite.LoadAsset();
                break;
        }

        
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button.usesRemainingSprite.color = TextOutlineColor;
        }

        

        PassiveComp = Button.GetComponent<PassiveButton>();
    }

    public override void SetUses(int amount)
    {
        base.SetUses(amount);
        
        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
            Button!.usesRemainingSprite.color = TextOutlineColor;
        }

        
    }

    public override void ClickHandler()
    {
        if (CanClick() && 
            !PlayerControl.LocalPlayer.HasModifier<DisabledModifier>())
        {
            if (LimitedUses)
            {
                if (DecreaseCharge) UsesLeft--;
                Button?.SetUsesRemaining(UsesLeft);
                
                if (TextOutlineColor != Color.clear)
                {
                    SetTextOutline(TextOutlineColor);
                    if (Button != null)
                    {
                        Button.usesRemainingSprite.color = TextOutlineColor;
                    }
                }

                
            }

            OnClick();
            if (HasEffect)
            {
                EffectActive = true;
                Timer = EffectDuration;
            }
            /*else
            {
                Timer = Cooldown;
            }*/
        }
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (MeetingHud.Instance)
        {
            return;
        }

        Button?.gameObject.SetActive(HudManager.Instance.UseButton.isActiveAndEnabled ||
                                     HudManager.Instance.PetButton.isActiveAndEnabled);
    }
}

[MiraIgnore]
public abstract class ObjectWorkshopRoleButton<TRole> : ObjectWorkshopButton where TRole : RoleBehaviour
{
    public TRole Role => PlayerControl.LocalPlayer.GetTrueRole<TRole>()!;
    public PlayerControl Player => Role.Player;
    public CustomActionButton button => this;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is TRole;
    }
}

[MiraIgnore]
public abstract class ObjectWorkshopRoleButton<TRole, TTarget> : ObjectWorkshopTargetButton<TTarget>
    where TTarget : MonoBehaviour where TRole : RoleBehaviour
{
    public TRole Role => PlayerControl.LocalPlayer.GetRole<TRole>()!;
    public PlayerControl Player => Role.Player;
    public CustomActionButton button => this;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is TRole;
    }

    public override void SetOutline(bool active)
    {
        if (Target != null && !PlayerControl.LocalPlayer.HasDied())
        {
            if (Target is PlayerControl target)
            {
                target.cosmetics.currentBodySprite.BodySprite.SetOutline(active ? Role.TeamColor : null);
            }
            else if (Target is DeadBody body)
            {
                body.bodyRenderers.Do(x => x.SetOutline(active ? Role.TeamColor : null));
            }
            else if (Target is Vent vent)
            {
                vent.SetOutline(active, true, Role.TeamColor);
            }
            else if (Target is OWObject obj)
            {
                obj.SetOutline(active, true, Role.TeamColor);
            }
        }
    }

    public override bool IsTargetValid(TTarget? target)
    {
        if (target is PlayerControl playerTarget)
        {
            return base.IsTargetValid(target) && !playerTarget.inVent &&
                   !playerTarget.GetModifiers<DisabledModifier>().Any(mod => !mod.CanBeInteractedWith);
        }

        return base.IsTargetValid(target);
    }
}

public interface IAftermathablePlayerButton : IAftermathableButton
{
    PlayerControl? Target { get; set; }
}

public interface IAftermathableBodyButton : IAftermathableButton
{
    DeadBody? Target { get; set; }
}

public interface IAftermathableButton
{
    void ClickHandler();
}

public interface IDiseaseableButton
{
    void SetDiseasedTimer(float multiplier);
}

public interface IKillButton
{
}