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
        Factory,
        OvercommitedCleric,
        DragonWhisperer,
        Archer,
        ArcanePortal,
        LeylineOfTheHarvest,
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
        Demon

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
