using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SpellSiegeData
{
    public enum Cards
    {
        turtle, //turtle
        peggy, //peggy
        Wyvern, //wyvern
        cerberus, //cerberus
        ElvishMystic,
        dragon, //dragon
        python, //python
        monkey, //monkey
        Leviathan,//levi
        Squirell, //squirell
        SlumberingAncient, 
        kitsune, //kitsune
        kirin, //kirin
        Murder, //murder
        LightningStrike, //ls
        MeteorShower, //msh
        Dog, //dog
        iguana, //iguana
        alpaca, //alpaca
        ArcanePortal, 
        Buff, //buff
        Cow, //cow
        Harvest, 
        Growth, //growth
        Pig, //pig
        Sacrifice, //sac
        Hamster, //hamster
        Lion, //lion
        Wildebeest, //wildebeest
        Lynx, //lynx
        Panther, //panther
        Rebirth, //rebirth
        Cat, //cat
        Bat, //bat
        Doomsday, //doomsday
        Turkey, //turkey
        FastMana, //fastmana
        Phoenix, //phoenix
        Poison, //poison
        Bow, //bow
        Hedgehog, //hedgehog
        Attack, //attack
        Baboon, //baboon

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
