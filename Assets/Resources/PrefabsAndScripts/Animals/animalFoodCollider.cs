using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Apply to child objects of animals whose "foodColliderIsSeparate" option is checked. This allows the gameobject to serve as a food collider for the animal
//See Animal class for more details

public class animalFoodCollider : MonoBehaviour
{
    Animal animal;

    // Start is called before the first frame update
    void Start()
    {
        animal = transform.parent.gameObject.GetComponent<Animal>() as Animal;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        animal.triggerEntered(other); 
    }

    void OnTriggerExit2D(Collider2D other)
    {
        animal.triggerExited(other);
    }
}
