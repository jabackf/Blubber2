-Try subtracting conveyor velocity instead of setting to 0. Also, some blocks get stuck inside the conveyor when tossed through it.

-Bouncy block doesn't push the player very far when he collides horizontally with it. Maybe check if it's sideways and add additional force? Or use something other than velocity to bounce? Such as force?

-Add other forms of movement to moving platforms, and maybe a pause option

-general cleanup needs done

-Get get on stuck walls when holding some items.

-Heavy items can land on head and be hard to get off. Try making character head collider round?

-It might be better to use a fixed joint for climbing. That way you can use a break force to knock off the ladder.

-Maybe throw aim with the mouse?

-Stuff to add: Item spawner pipe, push buttons, drop platforms, dissappearing platforms, enemies, water, dialog system, inventory system, items with actions

Dialog
-Reduce velocity to 0 when dialog initiated
-Maybe a default answer branch? IE, a place to go if none of the listed options are entered
-Maybe add a topperstring to the AnswerBranch class? This string gets added to the front of the message in the dialog box
-Character emotions or actions while talking
-Character change facing direction to point to the one talking
-Case sensitivty for text entry?
-Add pause and resume feature for auto dialog (for if character interupts)