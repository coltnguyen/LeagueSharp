using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace WolfZilean
{
    internal class Program
    {
        public static string ChampName = "Zilean";
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

            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R);

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
            Wolf.SubMenu("Combo").AddItem(new MenuItem("useW", "Use W").SetValue(true));
            Wolf.SubMenu("Combo").AddItem(new MenuItem("useE", "Use E").SetValue(true));
            Wolf.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Ult menu
            Wolf.AddSubMenu(new Menu("Extra", "Extra"));
            Wolf.SubMenu("Extra").AddItem(new MenuItem("useR", "Use R")).SetValue(true);
            Wolf.SubMenu("Extra").AddItem(new MenuItem("HPPercent", "R at % HP")).SetValue(new Slider(10, 1, 100));

            //Drawings menu
            Wolf.AddSubMenu(new Menu("Drawings", "Drawings"));
            Wolf.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q/E").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

            //Make the menu visible
            Wolf.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)

            Game.PrintChat("Wolf" + ChampName + " loaded! By GuiltyWolf");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Wolf.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
        }

        public static void Combo()
        {
            var useQ = Wolf.Item("useQ").GetValue<bool>();
            var useW = Wolf.Item("useW").GetValue<bool>();
            var useE = Wolf.Item("useE").GetValue<bool>();
            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if (useQ && Q.IsReady())
            {
                Q.CastOnUnit(target);
            }

            if (useW && W.IsReady() && !Q.IsReady()) //!Spell.IsReady() = Spell not ready
            {
                W.Cast();
            }

            if (useE && E.IsReady())
            {
                E.CastOnUnit(target);
            }
        }

        public static void AutoR()
        {
            var useR = Wolf.Item("user").GetValue<bool>();
            if (useR && R.IsReady() &&
                Wolf.Item("HPPercent").GetValue<Slider>().Value <= ((Player.Health/Player.MaxHealth)*100))
            {
                R.Cast(Player);
            }
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = Wolf.Item("QRange").GetValue<Circle>();
            if (menuItem.Active) Utility.DrawCircle(Player.Position, Q.Range, menuItem.Color);
            //Draw Ranges of Abilities
            foreach (Spell spell in SpellList)
            {
                menuItem = Wolf.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }
    }
}
