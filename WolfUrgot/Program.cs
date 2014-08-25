using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Thanks Snorflake for template, PQMailer for some fixes and Trelli for helping fix Q Collision :D

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace WolfUrgot
{
    class Program
    {
        public static string ChampName = "Urgot";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player; // Instead of typing ObjectManager.Player you can just type Player
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, Q2, W, E;

        public static Menu Wolf;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 1000);
            Q2 = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);

            Q.SetSkillshot(0.10f, 100f, 1600f, true, SkillshotType.SkillshotLine); //Skillshot Delay, Width, Speed, Collision (T/F), Prediction (Line/Circle/Cone)
            Q2.SetSkillshot(0.10f, 100f, 1600f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.283f, 0f, 1750f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(Q2);
            SpellList.Add(W);
            SpellList.Add(E);

            //Base menu
            Wolf = new Menu("Wolf" + ChampName, ChampName, true);
            //Orbwalker and menu
            Wolf.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Wolf.SubMenu("Orbwalker"));
            //Target selector and menu
            var ts = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(ts);
            Wolf.AddSubMenu(ts);
            //Combo menu
            Wolf.AddSubMenu(new Menu("Combo", "Combo"));
            Wolf.SubMenu("Combo").AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            Wolf.SubMenu("Combo").AddItem(new MenuItem("useE", "Use E").SetValue(true));
            Wolf.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
             //Drawings menu:
            Wolf.AddSubMenu(new Menu("Drawings", "Drawings"));
            Wolf.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Wolf.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            //Exploits
            //Wolf.AddItem(new MenuItem("NFE", "No-Face Exploit").SetValue(true));
            //Make the menu visible
            Wolf.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)

            Game.PrintChat("Wolf" + ChampName + " loaded! By GuiltyWolf");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Wolf.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
        }

        private static bool HasDebuff(Obj_AI_Base target) //Urgot E Debuff
        {
            return target.HasBuff("UrgotCorrosiveDebuff");
        }

        public static void Combo()
        {
            var useQ = Wolf.Item("useQ").GetValue<bool>();
            var useE = Wolf.Item("useE").GetValue<bool>();
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (target == null) return;

            if (useQ && Q.IsReady() && target.HasBuff("UrgotCorrosiveDebuff"))
            {
                    Q2.Cast(target, true);
            }
            else
            {
                if (useQ && Q.IsReady())
                {
                        Q.CastIfHitchanceEquals(target, HitChance.Medium);
                    }
                }
                if (W.IsReady() && target.HasBuff("UrgotCorrosiveDebuff"))
                {
                    W.Cast();
                }

                if (useE && E.IsReady())
            {
                var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
                if (E.IsReady() && eTarget.IsValidTarget())
                {
                    E.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }
        public static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = Wolf.Item("QRange").GetValue<Circle>();
            var menuItem2 = Wolf.Item("ERange").GetValue<Circle>();
            if (menuItem.Active) Utility.DrawCircle(Player.Position, Q.Range, menuItem.Color);
            if (menuItem2.Active) Utility.DrawCircle(Player.Position, E.Range, menuItem.Color);
            //Draw Ranges of Abilities
            foreach (var spell in SpellList)
            {
                menuItem = Wolf.Item(spell.Slot + "Range").GetValue<Circle>();
                menuItem2 = Wolf.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                if (menuItem2.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem2.Color);
            }
        }
    }
}
