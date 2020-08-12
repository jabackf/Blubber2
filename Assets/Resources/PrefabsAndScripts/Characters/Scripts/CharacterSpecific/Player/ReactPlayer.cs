using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactPlayer : React
{
    public override void preExecute(reaction r)
    {
        if (!blubberAnim) return;

        string character = blubberAnim.getCharacterConfigurationString();

        if (character=="None")
            r.resetToDefault();

        if (r.name=="Mean")
        {
            if (character == "Chef")
                r.sayStrings = new List<string>() { "Revenge is a dish best served cold.", "Bon apetit, you jerk!" };
            if (character == "Santa")
                r.sayStrings = new List<string>() { "Merry Christmas, you filthy animal!", "Coal in your stocking just didn't seem good enough", "Maybe next time you'll stay off my naughty list!" };
        }

        //Now that we're back to default we can check for special contexts to change our reaction

    }
}
