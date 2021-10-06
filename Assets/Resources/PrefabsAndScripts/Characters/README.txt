The following are some basic code files that are common to Blubber characters. 

PlayerInput - This captures user input and passes it into a CharacterController2d_Input object. This CC2D_Input is created automatically by this PlayerInput script.

CPUInput - This generates input and passes it into a CharacterController2D_Input. This CC2D_Input is created automatically by this CPUInput script. Input generated might be Incap recordings (input that has been captured and can be replayed) or a basic sort of AI.
 
CharacterController2D_Input - This is the intermediary between either the CPUInput or PlayerInput and CharacterController2D objects. It takes input information, does logic with it (for example, don't jump if you're in the middle of aiming), then uses it to activate the CharacterController2D actions.

CharacterController2D - This contains basic movement and action logic. Stuff like running, jumping, etc. 

BlubberAnimation - This handles some basic graphical stuff like dress objects, eyes, blinking, emote icons, etc. Also has a few asic functions like FaceFoward, CircleOn (spin in a circle), etc. This script inherits from CharacterAnimation, which is a broader script that supports things like run/jump/climb animations and stuff.

React - This script gives us a system for configuring character to react to generic things. Say something nasty happens and this character recieves a Gross() message. This script lets us connect that Gross message with various kinds of responses.

SquishAnim - This does the "breathing" animation thing. Slowly and gently scales the character up and down. Makes the character a little less static.

StretchAndSquash - This does a stretch and squash animation based on RigidBody2d velocity. So you can make the character stretch if he's jumping, squash if he's falling, etc.