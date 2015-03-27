using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using SharpDX;
using LeagueSharp.Common;

namespace WolfAshe
{
    internal class Program
    {
        private const string hero = "Ashe";

        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q, _w, _e, _r;
        private static readonly List<Spell> _spellList = new List<Spell>();
        private static SpellSlot _ignite;

        private static bool QisActive
        {
            get { return Player.HasBuff("FrostShot", true); }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        #region Main

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (_menu.Item("WolfAshe.hitChance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        #endregion

        #region Gameloaded

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.ChampionName.Equals(hero, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }
            Notifications.AddNotification("WolfAshe by GuiltyWolf", 10000);

            #region Spell Data

            // Initialize spells
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W, 1200);
            _e = new Spell(SpellSlot.E);
            _r = new Spell(SpellSlot.R);

            // Add to spell list
            _spellList.AddRange(new[] {_q, _w, _e, _r});

            // Initialize ignite
            _ignite = Player.GetSpellSlot("summonerdot");

            #endregion

            //Event handlers
            Game.OnUpdate += Game_OnGameUpdate;
            _q.SetSkillshot(250f, (float) (24.32f*Math.PI/180), 902f, true, SkillshotType.SkillshotCone);
            _r.SetSkillshot(250f, 130f, 1600f, false, SkillshotType.SkillshotLine);
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;


            try
            {
                InitializeMenu();
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region OnGameUpdate

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }
        }

        #endregion

        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawingW = _menu.Item("WDraw", true).GetValue<Circle>();

            if (Player.IsDead)
                return;
            if (_w.IsReady())
                Render.Circle.DrawCircle(Player.Position, _w.Range, drawingW.Color);
        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && !QisActive)
            {
                if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
                    _menu.Item("QCombo", true).GetValue<Boolean>())
                    _q.Cast();
            }
            if (!(args.Target is Obj_AI_Hero) && QisActive)
                _q.Cast();
        }

        #region Combo

        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
            {
                return;
            }
            if (_menu.Item("WCombo").GetValue<bool>() && _w.IsReady())
            {
                _w.CastIfHitchanceEquals(target, HitChance.Medium);
            }

            if (_menu.Item("RCombo").GetValue<bool>() && _r.IsReady())
            {
                foreach (
                    Obj_AI_Hero hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                hero =>
                                    hero.IsValidTarget(_r.Range) &&
                                    ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R, 1) - 20 > hero.Health))
                    _r.CastIfHitchanceEquals(hero, CustomHitChance);
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health &&
                _menu.Item("UseIgnite").GetValue<bool>())
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region Harass

        private static void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            if (_menu.Item("HarassW").GetValue<bool>() && _w.IsReady())
            {
                _w.CastIfHitchanceEquals(target, HitChance.Medium);
            }
        }

        #endregion

        #region Ignite

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float) Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region Intterupt

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(ObjectManager.Player) > _r.Range)
            {
                return;
            }

            if (sender.IsValidTarget(_r.Range) && args.DangerLevel == Interrupter2.DangerLevel.High && _r.IsReady())
            {
                _r.Cast(ObjectManager.Player);
            }

            else if (sender.IsValidTarget(_r.Range) && args.DangerLevel == Interrupter2.DangerLevel.High && _r.IsReady() &&
                     !_r.IsReady())
            {
                _r.Cast(ObjectManager.Player);
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsValidTarget(_q.Range))
            {
                if (_menu.Item("InterR").GetValue<bool>() && !_q.IsReady() && _r.IsReady())
                {
                    _r.CastIfHitchanceEquals(gapcloser.Sender, HitChance.High);
                }
            }
        }

        #endregion

        #region Menu

        private static void InitializeMenu()
        {
            _menu = new Menu("WolfAshe", hero, true);

            //Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            //Combo
            Menu comboMenu = _menu.AddSubMenu(new Menu("Combo", "Combo"));
            comboMenu.AddItem(new MenuItem("QCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("WCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("RCombo", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("WDraw", "W Range", true).SetValue(new Circle(true, Color.DodgerBlue)));
            comboMenu.AddItem(
                new MenuItem("WolfAshe.hitChance", "Hitchance").SetValue(
                    new StringList(new[] {"Low", "Medium", "High", "Very High"}, 3)));
            comboMenu.AddItem(new MenuItem("UseIgnite", "Use Ignite in combo when killable").SetValue(true));

            //Harass
            Menu harassMenu = _menu.AddSubMenu(new Menu("Harass", "H"));
            harassMenu.AddItem(new MenuItem("HarassW", "Use W").SetValue(false));
            harassMenu.AddItem(
                new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Interupt
            Menu interruptMenu = _menu.AddSubMenu(new Menu("Interrupt", "I"));
            interruptMenu.AddItem(new MenuItem("InterR", "Use R").SetValue(false));


            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        #endregion
    }
}
