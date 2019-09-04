-Sometimes the player can land on the bottom side of a platform collision box for platforms with an effector

-Try subtracting conveyor velocity instead of setting to 0.

-Bouncy block doesn't push the player very far when he collides horizontally with it. Maybe check if it's sideways and add additional force? Or use something other than velocity to bounce? Such as force?

-Add other forms of movement to moving platforms, and maybe a pause option

-When pushing against wall:
NullReferenceException: Object reference not set to an instance of an object
CharacterController2D.Move (System.Single move, System.Boolean crouch, System.Boolean jump)

-Conveyor warning thingy

-general cleanup needs done

-Check for ceiling before pickup up items

-Get get on stuck walls when holding some items.

-Heavy items can land on head and be hard to get off. Try making character head collider round?

-Add random offset to squishanim