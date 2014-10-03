//This assembly is for Crittlesticks only
using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace WolfYOLOSticks
{
    internal class Program
    {
        public static string ChampName = "Fiddlesticks";
        public static Orbwalking.Orbwalker Orbwalker;

        public static Obj_AI_Base Player = ObjectManager.Player;
        // Instead of typing ObjectManager.Player you can just type Player

        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;

        public static Menu Wolf;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 575);
            W = new Spell(SpellSlot.W, 575);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 800);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

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
            Wolf.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass menu
            Wolf.AddSubMenu(new Menu("Harass", "Harass"));
            Wolf.SubMenu("Harass").AddItem(new MenuItem("useQ", "Use Q").SetValue(true));
            Wolf.SubMenu("Harass").AddItem(new MenuItem("useE", "Use E").SetValue(true));
            Wolf.SubMenu("Harass")
                .AddItem(new MenuItem("HarassActive", "Harass").SetValue(new KeyBind(88, KeyBindType.Press)));

            //Misc menu
            Wolf.AddSubMenu(new Menu("Misc", "Misc"));
            Wolf.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Interrrupt Spells").SetValue(true));

            //Drawings menu
            Wolf.AddSubMenu(new Menu("Drawings", "Drawings"));
            Wolf.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Wolf.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

            //Make the menu visible
            Wolf.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)
            Interrupter.OnPosibleToInterrupt += Interrupter_OnPosibleToInterrupt;

            Game.PrintChat("Wolf" + ChampName + " loaded! By GuiltyWolf");
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Wolf.Item("Interrupt").GetValue<bool>()) return;
            Q.Cast(unit);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Wolf.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Wolf.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass();
            }
        }

        public static void Combo()
        {
            var useQ = Wolf.Item("useQ").GetValue<bool>();
            var useE = Wolf.Item("useE").GetValue<bool>();
            Obj_AI_Hero qtarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            Obj_AI_Hero etarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (qtarget == null) return;
            if (etarget == null) return;

            if (useQ && Q.IsReady())
            {
                Q.Cast(qtarget, true);
            }

            if (useE && E.IsReady())
            {
                E.Cast(etarget, true);
            }
        }

        public static void Harass()
        {
            var usehE = Wolf.Item("useHE").GetValue<bool>();
            Obj_AI_Hero etarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (etarget == null) return;

            if (usehE && E.IsReady())
            {
                E.Cast(etarget);
            }
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = Wolf.Item("QRange").GetValue<Circle>();
            var menuItem2 = Wolf.Item("ERange").GetValue<Circle>();
            if (menuItem.Active) Utility.DrawCircle(Player.Position, Q.Range, menuItem.Color);
            if (menuItem.Active) Utility.DrawCircle(Player.Position, E.Range, menuItem.Color);
            //Draw Ranges of Abilities
            foreach (Spell spell in SpellList)
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