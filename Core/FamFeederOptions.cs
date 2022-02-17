

namespace Core;

public class FamFeederOptions
{
    public FamFeederOptions()
    {
        
    }
    public FamFeederOptions(string proCoSysConnectionString)
    {
        ProCoSysConnectionString = proCoSysConnectionString;
    }

    public string ProCoSysConnectionString { get; set; }

}