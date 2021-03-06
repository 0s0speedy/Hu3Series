﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;
using SharpDX;

namespace ThreshHu3
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Active Q2;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Menu ThreshMenu, SettingsMenu;


        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Thresh")
                return;

            TargetSelector.Init();
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, 1080, SkillShotType.Linear, (int)0.500f, Int32.MaxValue, (int)70f);
            Q2 = new Spell.Active(SpellSlot.Q, 1300);
            W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Linear, (int)0.25f, Int32.MaxValue, (int)80f);
            E = new Spell.Skillshot(SpellSlot.E, 500, SkillShotType.Linear, 1, 2000, 110);
            R = new Spell.Skillshot(SpellSlot.R, 2000, SkillShotType.Linear, (int)1f, Int32.MaxValue, (int)(160f));

            ThreshMenu = MainMenu.AddMenu("Thresh Hu3", "threshhu3");
            ThreshMenu.AddGroupLabel("Thresh Hu3 1.0");
            ThreshMenu.AddSeparator();
            ThreshMenu.AddLabel("Made By MarioGK");
            SettingsMenu = ThreshMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings");
            SettingsMenu.AddLabel("Combo");
            SettingsMenu.Add("comboQ", new CheckBox("Use Q on Combo"));
            SettingsMenu.Add("comboQ2", new CheckBox("Follow with Q on Combo"));
            SettingsMenu.Add("comboW", new CheckBox("Use W on Combo"));
            SettingsMenu.Add("comboR", new CheckBox("Use R on Combo"));
            SettingsMenu.Add("comboRmin", new Slider("Min Enemies to use R", 2, 1, 5));
            SettingsMenu.AddLabel("Harass");
            SettingsMenu.Add("harassQ", new CheckBox("Use Q on Harass"));
            SettingsMenu.Add("harassW", new CheckBox("Use E on Harass"));
            SettingsMenu.AddLabel("Draw");
            SettingsMenu.Add("drawQ", new CheckBox("Draw Q"));
            SettingsMenu.Add("drawW", new CheckBox("Draw W"));
            SettingsMenu.Add("drawE", new CheckBox("Draw E"));
            SettingsMenu.Add("drawR", new CheckBox("Draw R Combo"));


            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }
        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
            {
                Harass();
            }

        }
        
        private static void Combo()
        {
            
            var useQ = SettingsMenu["comboQ"].Cast<CheckBox>().CurrentValue;
            var useQ2 = SettingsMenu["comboQ2"].Cast<CheckBox>().CurrentValue;
            var useW = SettingsMenu["comboW"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["comboE"].Cast<CheckBox>().CurrentValue;
            var useR = SettingsMenu["comboR"].Cast<CheckBox>().CurrentValue;
            var minR = SettingsMenu["comboRmin"].Cast<Slider>().CurrentValue;

            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.High)
                {
                    Q.Cast(target);
                }
                if (useQ2 && Q2.IsReady() && target.HasBuff("ThreshQ"))
                {
                    Q2.Cast();
                }
                if (useE && E.IsReady() && E.GetPrediction(target).HitChance >= HitChance.Medium && !target.HasBuff("ThreshQ") && target.IsValidTarget(E.Range))
                {
                    var x = Player.Instance.ServerPosition.X - target.ServerPosition.X;
                    var y = Player.Instance.ServerPosition.Y - target.ServerPosition.Y;

                    var vector = new Vector3(
                        Player.Instance.ServerPosition.X + x,
                        Player.Instance.ServerPosition.Y + y,
                        Player.Instance.ServerPosition.Z);

                    E.Cast(vector);
                }
                if (useR && R.IsReady() && Player.Instance.CountEnemiesInRange(R.Range) >= minR)
                {
                    R.Cast();
                }
            }
        }

        private static void Harass()
        {

            var useQ = SettingsMenu["harassQ"].Cast<CheckBox>().CurrentValue;
            var useE = SettingsMenu["harassE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
            {
                if (useQ && Q.IsReady() && Q.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    Q.Cast(target);
                }
                if (useE && E.IsReady() && E.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SettingsMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.White, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (SettingsMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Purple, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}