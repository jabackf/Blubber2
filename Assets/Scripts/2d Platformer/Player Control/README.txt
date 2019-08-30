This controller is originally based on the player movement scripts found ing Brackey's 2d platformer movement video. Motion of the character is largely handled by the character controller 2d script, while input is handled by player movement. To setup for best use:

Add a player object.

Add two empty gameobject children to the player: a ground check and a ceiling check. Position both appropriately.

Add two colliders, one for the top and one for the bottom. Recommended use is to use a circle for the bottom and a box (with the bottom corners internal to the circle) for the head. To prevent the player from sticking to corners, add to the colliders a slippery physics material with both the friction and bounciness turned all the way down.

Add a rigid body 2d. It is recommended you turn the gravity scale up (try 3). To prevent rotation, open constraints and freeze rotation on the z axis.

Add both scripts to the player.

For the controller script:
Air control lets you control the player while jumping. What is Ground is a layer for ground objects. Ground check and ceiling check are the empty child objects you created. Can crouch enables/disables crouching. Crouch disable is the collider used on the top half of your character (it gets disabled when you crouch). Other variables adjust motion.

For the movement script:
Controller is the CharacterController2d.cs that you added to your character.
The animator flags are string names of flags that you can define within the animator. The script will activate these flags for animations. Leave these flags blank for no animation.