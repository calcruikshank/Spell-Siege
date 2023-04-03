using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SpellSiegeData
{
    public enum Cards
    {
        AngryTurtle,
        Avacyn,
        BattleScarredDemon,
        DemonicVulture,
        ElvishMystic,
        GoldspanDragon,
        HiredAssassin,
        PrimitiveWeaponsmith,
        Pudge,
        ShamanDarkWays,
        SlumberingAncient,
        SpiritSummoner,
        SymbioticOoze,
        Dissolve,
        LightningStrike,
        MeteorStrike,
        OvercommitedCleric,
        DragonWhisperer,
        Archer,
        ArcanePortal,
        LeylineOfTheHarvest,
        CaptainOfTheGuard,
        Harvest,
        NaturalGrowth,
        Bear,
        BottomFeeder,
        NumOfCardTypes
    }

    public enum cardRarity
    {
        common,
        uncommon,
        rare,
        mythic,
        Legendary
    }
    public enum CardType
    {
        Creature,
        Spell,
        Structure
    }
    public enum ManaType
    {
        Red,
        Green,
        Blue,
        Black,
        White
    }
    public enum traversableType
    {
        Untraversable,
        OnlyFlying,
        SwimmingAndFlying,
        TraversableByAll
    }
    public enum CreatureType
    {
        None,
        Dragon, //On The turn created
        Elf,
        Goblin,
        Reptile,
        Human,
        Angel,
        Wizard,
        Demon,
        Beast

        //not sure if i need a tapped state yet trying to keep it as simple as possible
    }
    public enum travType
    {
        Walking,
        Swimming,
        SwimmingAndWalking,
        Flying
    }
}
