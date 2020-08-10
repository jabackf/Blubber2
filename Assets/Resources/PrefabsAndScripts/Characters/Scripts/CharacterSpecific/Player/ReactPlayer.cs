using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactPlayer : React
{
    public override void preExecute(reaction r)
    {
        //This will reset us to the default state specified when the character was configured in the editor
        //r.resetToDefault();

        //Now that we're back to default we can check for special contexts to change our reaction
    }
}
