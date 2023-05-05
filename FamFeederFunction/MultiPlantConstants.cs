using System.Collections.Generic;

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
            case "All_ACCEPTED":
                plants = new List<string> { "PCS$JOHAN_CASTBERG", "PCS$OSEBERG_SOR", "PCS$OSF_DELTA", "PCS$KOLLSNES" };
                return true;
            default:
                return false;
        }
    }
}