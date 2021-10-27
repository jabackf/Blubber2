using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This was added so that an object can contain multiple tags. Some objects might check for a customTags script to get a list of tags that it contains.

//Here is a list of some known tags.

//Tags related to vacuum cleaner
//edibleByVacuum (destroys the object when it reaches the end of the vacuum vortex. This one does not require the object to have a rigidbody when sucked by the vacuum, and it will be moved manually if no rb is attached.)
//vacuumNoEatSound (when edibleByVacuum tag is present and the object is eaten, this tag will prevent it from playing sndEat)
//ignoredByVacuum
//movePositionByVacuum (Moves the object when it gets into the vortex, regardless of rather it has a rigidbody or not.)
//addForceByVacuum (Moves the object via rigidbody.addforce, regardless of rather the vacuum is configured to use movePosition by default)
//swirledByVacuum (Adds rotation while in the suction vortex.)
//shrunkByVacuum (Shrinks the object to a smaller size while it's in the suction vortex.)
//clogsVacuum (must be tagged as edibleByVacuum as well)
//removedFromHolderByVacuum (The vacuum will remove the item from the holder if it's being held)


public class customTags : MonoBehaviour
{
     [SerializeField]
     private List<string> tags = new List<string>();
     
     public bool hasTag(string tag)
     {
         return tags.Contains(tag);
     }
     
     public IEnumerable<string> getTags()
     {
         return tags;
     }
     
     public void rename(int index, string tagName)
     {
         tags[index] = tagName;
     }
     
     public string getAtIndex(int index)
     {
         return tags[index];
     }
     
     public int count
     {
         get { return tags.Count; }
     }
}
