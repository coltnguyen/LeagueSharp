//Thanks iSnorflake for the template, PQMailer for some error fixing and FlapperDoodle for some info
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace WolfZilean
{
    class Program
    {
        //Calls Champion
        public static string ChampName = "Zilean";
        //Orbwalker
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player; // Instead of typing ObjectManager.Player you can just type Player
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;

        public static Menu Wolf;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 900);

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
            Wolf.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            //Ult menu
            Wolf.AddSubMenu(new Menu("Ult", "Ult Settings"));
            Wolf.SubMenu("Ult").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Wolf.SubMenu("Ult").AddItem(new MenuItem("HealthPercent", "HP Trigger Percent").SetValue(new Slider(15)));
            //Drawings menu:
            Wolf.AddSubMenu(new Menu("Drawings", "Drawings"));
            Wolf.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q/E").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Wolf.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
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

        private void OnGameUpdate(EventArgs args)
        {
                if (Wolf.Item("Ult").GetValue<Boolean>())
                {
                    if (GetPlayerHealthPercentage() <= Wolf.Item("HealthPercent").GetValue<Slider>().Value)
                    {
                        R.Cast(Player);
                    }
                }
            }


        public static void Combo()
        {
            var useQ = Wolf.Item("useQ").GetValue<bool>();
            var useW = Wolf.Item("useW").GetValue<bool>();
            var useE = Wolf.Item("useE").GetValue<bool>();
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if (useQ && Q.IsReady())
            {
                Q.CastOnUnit(target, true);
            }
            if (useW && W.IsReady())
            {
                W.Cast();
            }
            if (useE && E.IsReady())
            {
                E.CastOnUnit(target, true);
            }
        }
        public static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = Wolf.Item("QRange").GetValue<Circle>();
            var menuItem2 = Wolf.Item("RRange").GetValue<Circle>();
            if (menuItem.Active) Utility.DrawCircle(Player.Position, Q.Range, menuItem.Color);
            if (menuItem2.Active) Utility.DrawCircle(Player.Position, R.Range, menuItem.Color);
            //Draw Ranges of Abilities
            foreach (var spell in SpellList)
            {
                menuItem = Wolf.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active) 
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }
        private float GetPlayerHealthPercentage()
        {
            return ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth;
        }
    }
}