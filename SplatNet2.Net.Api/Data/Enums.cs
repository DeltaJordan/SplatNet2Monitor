using System;
using System.Collections.Generic;
using System.Text;

namespace SplatNet2.Net.Api.Data
{
    public enum Lobby
    {
        TurfWar,
        Ranked,
        LeaguePair,
        LeagueTeam,
        Private,
        SplatfestPro,
        SplatfestNormal
    }

    public enum LobbyGroup
    {
        Regular,
        Gachi,
        Private,
        Splatfest
    }

    public enum LobbyType
    {
		Regular,
		Gachi,
		League,
		Splatfest
    }

    public enum GameMode
    {
        TurfWar,
        SplatZones,
        TowerControl,
        Rainmaker,
        ClamBlitz
    }

    public enum Stage
	{
        // ReSharper disable IdentifierTypo
        // ReSharper disable CommentTypo
        TheReef = 0, //battera
        MusselforgeFitness = 1, //fujitsubo
		StarfishMainstage = 2, //gangaze
		SturgeonShipyard = 3, //chozame
		InkblotArtAcademy = 4, //ama
		HumpbackPumpTrack = 5, //kombu
		MantaMaria = 6, //manta
		PortMackerel = 7, //hokke
		MorayTowers = 8, //tachiuo
		SnapperCanal = 9, //engawa
		KelpDome = 10, //mozuku
		BlackbellySkatepark = 11, //bbass
		ShellendorfInstitute = 12, //devon
		MakoMart = 13, //zatou
		WalleyeWarehouse = 14, //hakofugu
		ArowanaMall = 15, //arowana
		CampTriggerfish = 16, //mongara
		PiranhaPit = 17, //shottsuru
		GobyArena = 18, //ajifry
		NewAlbacoreHotel = 19, //otoro
		WahooWorld = 20, //sumeshi
		AnchoVGames = 21, //anchovy
		SkipperPavilion = 22, //mutsugoro
		ShiftyWindmillHouseOnThePearlie = 100, //mystery_04
		ShiftyWayslideCool = 101, //mystery_01
		ShiftyTheSecretOfSplat = 102, //mystery_02
		ShiftyGoosponge = 103, //mystery_03
		ShiftyCannonFirePearl = 105, //mystery_07
		ShiftyZoneOfGlass = 106, //mystery_06
		ShiftyFancySpew = 107, //mystery_05
		ShiftyGrapplinkGirl = 108, //mystery_09
		ShiftyZappyLongshocking = 109, //mystery_10
		ShiftyTheBunkerGames = 110, //mystery_08
		ShiftyASwiftlyTiltingBalance = 111, //mystery_11
		ShiftyTheSwitches = 112, //mystery_13
		ShiftySweetValleyTentacles = 113, //mystery_12
		ShiftyTheBounceyTwins = 114, //mystery_14
		ShiftyRailwayChillin = 115, //mystery_15
		ShiftyGusherTowns = 116, //mystery_16
		ShiftyTheMazeDasher = 117, //mystery_17
		ShiftyFloodersInTheAttic = 118, //mystery_18
		ShiftyTheSplatInOurZones = 119, //mystery_19
		ShiftyTheInkIsSpreading = 120, //mystery_20
		ShiftyBridgeToTentaswitchia = 121, //mystery_21
		ShiftyTheChroniclesOfRolonium = 122, //mystery_22
		ShiftyFurlerInTheAshes = 123, //mystery_23
		ShiftyMcPrincessDiaries = 124, //mystery_24
		ShiftyStation = 9999, //mystery
    }

    public enum Weapon
    {
		SplooshOmatic = 0, //bold
		NeoSplooshOmatic = 1, //bold_neo
		SplooshOmatic7 = 2, //bold_7
		SplattershotJr = 10, //wakaba
		CustomSplattershotJr = 11, //momiji
		KensaSplattershotJr = 12, //ochiba
		SplashOmatic = 20, //sharp
		NeoSplashOmatic = 21, //sharp_neo
		AerosprayMG = 30, //promodeler_mg
		AerosprayRG = 31, //promodeler_rg
		AerosprayPG = 32, //promodeler_pg
		Splattershot = 40, //sshooter
		TentatekSplattershot = 41, //sshooter_collabo
		KensaSplattershot = 42, //sshooter_becchu
		HeroShotReplica = 45, //heroshooter_replica
		OctoShotReplica = 46, //octoshooter_replica
		// Enum names can't start with a number, so .52 Gal and .92 Gal have their names swapped.
		Gal52 = 50, //52gal
		Gal52Deco = 51, //52gal_deco
		KensaGal52 = 52, //52gal_becchu
		NZAP85 = 60, //nzap85
		NZAP89 = 61, //nzap89
		NZAP83 = 62, //nzap83
		SplattershotPro = 70, //prime
		ForgeSplattershotPro = 71, //prime_collabo
		KensaSplattershotPro = 72, //prime_becchu
		Gal96 = 80, //96gal
		Gal96Deco = 81, //96gal_deco
		JetSquelcher = 90, //jetsweeper
		CustomJetSquelcher = 91, //jetsweeper_custom
		LunaBlaster = 200, //nova
		LunaBlasterNeo = 201, //nova_neo
		KensaLunaBlaster = 202, //nova_becchu
		Blaster = 210, //hotblaster
		CustomBlaster = 211, //hotblaster_custom
		HeroBlasterReplica = 215, //heroblaster_replica
		RangeBlaster = 220, //longblaster
		CustomRangeBlaster = 221, //longblaster_custom
		GrimRangeBlaster = 222, //longblaster_necro
		ClashBlaster = 230, //clashblaster
		ClashBlasterNeo = 231, //clashblaster_neo
		RapidBlaster = 240, //rapid
		RapidBlasterDeco = 241, //rapid_deco
		KensaRapidBlaster = 242, //rapid_becchu
		RapidBlasterPro = 250, //rapid_elite
		RapidBlasterProDeco = 251, //rapid_elite_deco
		L3Nozzlenose = 300, //l3reelgun
		L3NozzlenoseD = 301, //l3reelgun_d
		KensaL3Nozzlenose = 302, //l3reelgun_becchu
		H3Nozzlenose = 310, //h3reelgun
		H3NozzlenoseD = 311, //h3reelgun_d
		CherryH3Nozzlenose = 312, //h3reelgun_cherry
		Squeezer = 400, //bottlegeyser
		FoilSqueezer = 401, //bottlegeyser_foil
		CarbonRoller = 1000, //carbon
		CarbonRollerDeco = 1001, //carbon_deco
		SplatRoller = 1010, //splatroller
		KrakOnSplatRoller = 1011, //splatroller_collabo
		KensaSplatRoller = 1012, //splatroller_becchu
		HeroRollerReplica = 1015, //heroroller_replica
		DynamoRoller = 1020, //dynamo
		GoldDynamoRoller = 1021, //dynamo_tesla
		KensaDynamoRoller = 1022, //dynamo_becchu
		FlingzaRoller = 1030, //variableroller
		FoilFlingzaRoller = 1031, //variableroller_foil
		Inkbrush = 1100, //pablo
		InkbrushNouveau = 1101, //pablo_hue
		PermanentInkbrush = 1102, //pablo_permanent
		Octobrush = 1110, //hokusai
		OctobrushNouveau = 1111, //hokusai_hue
		KensaOctobrush = 1112, //hokusai_becchu
		HerobrushReplica = 1115, //herobrush_replica
		ClassicSquiffer = 2000, //squiclean_a
		NewSquiffer = 2001, //squiclean_b
		FreshSquiffer = 2002, //squiclean_g
		SplatCharger = 2010, //splatcharger
		FirefinSplatCharger = 2011, //splatcharger_collabo
		KensaCharger = 2012, //splatcharger_becchu
		HeroChargerReplica = 2015, //herocharger_replica
		Splatterscope = 2020, //splatscope
		FirefinSplatterscope = 2021, //splatscope_collabo
		KensaSplatterscope = 2022, //splatscope_becchu
		Eliter4K = 2030, //liter4k
		CustomEliter4K = 2031, //liter4k_custom
		Eliter4KScope = 2040, //liter4k_scope
		CustomEliter4KScope = 2041, //liter4k_scope_custom
		Bamboozler14MkI = 2050, //bamboo14mk1
		Bamboozler14MkII = 2051, //bamboo14mk2
		Bamboozler14MkIII = 2052, //bamboo14mk3
		GooTuber = 2060, //soytuber
		CustomGooTuber = 2061, //soytuber_custom
		Slosher = 3000, //bucketslosher
		SlosherDeco = 3001, //bucketslosher_deco
		SodaSlosher = 3002, //bucketslosher_soda
		HeroSlosherReplica = 3005, //heroslosher_replica
		TriSlosher = 3010, //hissen
		TriSlosherNouveau = 3011, //hissen_hue
		SloshingMachine = 3020, //screwslosher
		SloshingMachineNeo = 3021, //screwslosher_neo
		KensaSloshingMachine = 3022, //screwslosher_becchu
		Bloblobber = 3030, //furo
		BloblobberDeco = 3031, //furo_deco
		Explosher = 3040, //explosher
		CustomExplosher = 3041, //explosher_custom
		MiniSplatling = 4000, //splatspinner
		ZinkMiniSplatling = 4001, //splatspinner_collabo
		KensaMiniSplatling = 4002, //splatspinner_becchu
		HeavySplatling = 4010, //barrelspinner
		HeavySplatlingDeco = 4011, //barrelspinner_deco
		HeavySplatlingRemix = 4012, //barrelspinner_remix
		HeroSplatlingReplica = 4015, //herospinner_replica
		HydraSplatling = 4020, //hydra
		CustomHydraSplatling = 4021, //hydra_custom
		BallpointSplatling = 4030, //kugelschreiber
		BallpointSplatlingNouveau = 4031, //kugelschreiber_hue
		Nautilus47 = 4040, //nautilus47
		Nautilus79 = 4041, //nautilus79
		DappleDualies = 5000, //sputtery
		DappleDualiesNouveau = 5001, //sputtery_hue
		ClearDappleDualies = 5002, //sputtery_clear
		SplatDualies = 5010, //maneuver
		EnperrySplatDualies = 5011, //maneuver_collabo
		KensaSplatDualies = 5012, //maneuver_becchu
		HeroDualieReplicas = 5015, //heromaneuver_replica
		GloogaDualies = 5020, //kelvin525
		GloogaDualiesDeco = 5021, //kelvin525_deco
		KensaGloogaDualies = 5022, //kelvin525_becchu
		DualieSquelchers = 5030, //dualsweeper
		CustomDualieSquelchers = 5031, //dualsweeper_custom
		DarkTetraDualies = 5040, //quadhopper_black
		LightTetraDualies = 5041, //quadhopper_white
		SplatBrella = 6000, //parashelter
		SorellaBrella = 6001, //parashelter_sorella
		HeroBrellaReplica = 6005, //heroshelter_replica
		TentaBrella = 6010, //campingshelter
		TentaSorellaBrella = 6011, //campingshelter_sorella
		TentaCamoBrella = 6012, //campingshelter_camo
		UndercoverBrella = 6020, //spygadget
		UndercoverSorellaBrella = 6021, //spygadget_sorella
		KensaUndercoverBrella = 6022, //spygadget_becchu
		GrizzcoBlaster = 20000, //kuma_blaster
		GrizzcoBrella = 20010, //kuma_brella
		GrizzcoCharger = 20020, //kuma_charger
		GrizzcoSlosher = 20030, //kuma_slosher
    }

    public enum BattleResult
    {
		Victory,
		Defeat
    }

    public enum Gender
    {
		Male,
		Female
    }

    public enum Species
    {
		Inkling,
		Octoling
    }
	public class GearEnums
	{
		public enum Ability
		{
			None = -1, // locked ("?") or does not exist
			InkSaverMain = 0,
			InkSaverSub = 1,
			InkRecoveryUp = 2,
			RunSpeedUp = 3,
			SwimSpeedUp = 4,
			SpecialChargeUp = 5,
			SpecialSaver = 6,
			SpecialPowerUp = 7,
			QuickRespawn = 8,
			QuickSuperJump = 9,
			SubPowerUp = 10,
			InkResistanceUp = 11,
			BombDefenseUp = 12,
			ColdBlooded = 13,
			OpeningGambit = 100,
			LastDitchEffort = 101,
			Tenacity = 102,
			Comeback = 103,
			NinjaSquid = 104,
			Haunt = 105,
			ThermalInk = 106,
			RespawnPunisher = 107,
			AbilityDoubler = 108,
			StealthJump = 109,
			ObjectShredder = 110,
			DropRoller = 111,
			BombDefenseUpDx = 200,
			MainPowerUp = 201
		}
	}
}
