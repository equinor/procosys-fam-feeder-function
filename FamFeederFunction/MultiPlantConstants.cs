﻿using System.Collections.Generic;

namespace FamFeederFunction;

internal class MultiPlantConstants
{
    /// <summary>
    /// Attempts to retrieve a list of known plants associated with the specified name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="plants">When this method returns, contains the list of plants associated with the specified name, if the name was found; otherwise, an empty list.</param>
    /// <returns>true if the plants were retrieved successfully; otherwise, false.</returns>
    public static bool TryGetByMultiPlant(string name, out List<string> plants)
    {
        plants = new List<string>();
        switch (name.ToUpper())
        {
            case "OSEBERG":
                plants = new List<string> { "PCS$OSEBERG_C", "PCS$OSEBERG_OST","PCS$OSEBERG_SOR", "PCS$OSF_DELTA", "PCS$KOLLSNES" };
                return true;
            case "OSEBERG_STRICT":
                plants = new List<string> { "PCS$OSEBERG_SOR", "PCS$OSF_DELTA", "PCS$KOLLSNES" };
                return true;
            case "ALL_ACCEPTED":
                plants = new List<string> { "PCS$JOHAN_CASTBERG", "PCS$OSEBERG_SOR", "PCS$OSF_DELTA", "PCS$KOLLSNES" };
                return true;
            case "ALL":
                plants = new List<string>
                {
                    "PCS$ASGARD",
                    "PCS$ASGARD_A",
                    "PCS$ASGARD_B",
                    "PCS$ASGARD_C",
                    "PCS$BACALHAU",
                    "PCS$BAKKEN",
                    "PCS$BALTYK_WIND_II",
                    "PCS$BALTYK_WIND_III",
                    "PCS$BAY_DU_NORD",
                    "PCS$BEACON_WIND_1",
                    "PCS$BEACON_WIND_2",
                    "PCS$BERLING_SUBSEA",
                    "PCS$BRAGE",
                    "PCS$BRAZIL_TOOL_POOL",
                    "PCS$DOGGER_BANK_A",
                    "PCS$DOGGER_BANK_B",
                    "PCS$DOGGER_BANK_C",
                    "PCS$DOLWIN2",
                    "PCS$DRAUPNER",
                    "PCS$DUDGEON",
                    "PCS$EAGLE_FORD",
                    "PCS$EMPIRE_WIND",
                    "PCS$EMPIRE_WIND_1",
                    "PCS$EMPIRE_WIND_2",
                    "PCS$ETZEL_GAS_LAGER",
                    "PCS$FENJA",
                    "PCS$FORSKNINGSENTER",
                    "PCS$GINA_KROG",
                    "PCS$GOLIAT",
                    "PCS$GRANE",
                    "PCS$GUDRUN",
                    "PCS$GULLFAKS_A",
                    "PCS$GULLFAKS_B",
                    "PCS$GULLFAKS_C",
                    "PCS$HEIDRUN",
                    "PCS$HEIDRUN_FSU",
                    "PCS$HEIMDAL",
                    "PCS$HF_LNG",
                    "PCS$HYT",
                    "PCS$HYWIND",
                    "PCS$JOHAN_CASTBERG",
                    "PCS$JOHAN_SVERDRUP",
                    "PCS$KALUNDBORG",
                    "PCS$KLAB_SST_KAARSTOE",
                    "PCS$KOLLSNES",
                    "PCS$KRAFLA",
                    "PCS$KRISTIN",
                    "PCS$KVITEBJORN",
                    "PCS$KAARSTOE_LCI",
                    "PCS$LEISMER",
                    "PCS$LWP",
                    "PCS$MARIA_SUBSEA",
                    "PCS$MARINER",
                    "PCS$MARINER_FSU",
                    "PCS$MARTIN_LINGE",
                    "PCS$MARTIN_LINGE_B",
                    "PCS$MARTIN_LINGE_D",
                    "PCS$MARTIN_LINGE_E",
                    "PCS$MARTIN_LINGE_K",
                    "PCS$MHPP",
                    "PCS$MONGSTAD_NY",
                    "PCS$NESUK_CTRL_ROOM",
                    "PCS$NGPCS_TEST_BROWN",
                    "PCS$NGPCS_TEST_GREEN",
                    "PCS$NJORD",
                    "PCS$NORNE",
                    "PCS$NORTHERN_LIGHTS",
                    "PCS$ORMENLANGE",
                    "PCS$OSEBERG_C",
                    "PCS$OSEBERG_OST",
                    "PCS$OSEBERG_SOR",
                    "PCS$OSF_DELTA",
                    "PCS$PCS_TRAINING",
                    "PCS$PEREGRINO",
                    "PCS$PEREGRINO_C",
                    "PCS$PEREGRINO_FPSO",
                    "PCS$PEREGRINO_WHA",
                    "PCS$PEREGRINO_WHB",
                    "PCS$PIPL",
                    "PCS$PTSP",
                    "PCS$RAIA",
                    "PCS$REGIONAL_OM_FACILITY",
                    "PCS$ROSEBANK",
                    "PCS$ROSEBANK_SUBSEA",
                    "PCS$SHERINGHAM",
                    "PCS$SLEIPNER_A",
                    "PCS$SNORRE_A",
                    "PCS$SNORRE_B",
                    "PCS$SSRP",
                    "PCS$STATFJORD_A",
                    "PCS$STATFJORD_B",
                    "PCS$STATFJORD_C",
                    "PCS$STATOIL_BASIS",
                    "PCS$STURE",
                    "PCS$SUBSEA",
                    "PCS$SUBSEA_ADDON",
                    "PCS$SUBSEA_ITEM_EQ",
                    "PCS$SUBSEA_TOOL_POOL",
                    "PCS$SUN_BASIS",
                    "PCS$T_LAB",
                    "PCS$TCM",
                    "PCS$TJELDBERGODDEN",
                    "PCS$TROLL_A",
                    "PCS$TROLL_B",
                    "PCS$TROLL_C",
                    "PCS$TYRIHANS",
                    "PCS$UK_TOOL_POOL",
                    "PCS$VALEMON",
                    "PCS$VEGA",
                    "PCS$VESLEFRIKK",
                    "PCS$VILJE",
                    "PCS$VISUND",
                    "PCS$WIND_BASIS",
                    "PCS$WISTING",
                    "PCS$ZIDANE_SUBSEA",
                    "PCS$AASTA_HANSTEEN",
                };
                return true;
            default:
                return false;
        }
    }
}