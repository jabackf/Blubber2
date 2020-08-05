Dialog system

The two main components you're going to use are the Dialog script and the DialogRange script.

DialogRange is used for player intitiating dialogs and is derived from the Range system. The object containing this script will
be "in range" for dialog initiation when a collision is detected from a collider that has the specified range collider tag.
Icon Offset x and y can be used to adjust the location of the talk icon. The "Dialog" field is a reference to the specific dialog
script that will be initiated. Dialog range is only required for conversations that the player initiates!

Dialog script

This is the script that contains the dialog chain. Here are some important variables:

My Type determines the behavior of the dialog box. In general, the two main types are auto (meaning the dialog plays through automatically)
and straight shot (meaning player input is required for progressing through the dialog.) Here is a breakdown of the types:

     * Straightshot plays whole dialog through to finish, using player input to initiate and progress through dialog boxes
     * Group plays through the currently selected message group, using player input to initiate and progress through dialog boxes
     * Random picks a message group randomly and plays through it, using player input to initiate and close the dialog box
     * AutoRandom plays random message group entries automatically with timers
     * AutoStraightshot plays the whole conversation through from beginning to end automatically with timers.
     * AutoStraightLoop does the same as above, but loops back to the beginning every time it ends
     * AutoGroup plays the selected group automatically with timers.
     * AutoGroupLoop works like above, but loops to the beginning of the group when it reaches the end

Don't Repeat Random: If set to a random type, then this option will prevent a randomly selected dialog from being selected twice in a row.

Face Initiator: If the dialog is initiated via a DialogRange script, the initiator will be the character that walked up and pressed the
dialog button to start the dialog. The Face Initiator option will turn the character to whom this dialog is attached to face the initiator.
This option requires the object containing the dialog script to also have a CharacterController2D.

Start Group: See "Group" under dialog entries for an explanation of groups. If the dialog's type utilizes groups, then the start group will
be the group that the dialog starts with by default.

INTERUPTIONS occur when an automatic conversation is interupted by the player via DialogRange to engage in a different dialog.

Interupt Dialog is the AUTOMATIC dialog script that should be interupted, should this dialog be initiated. This script will receive the
Interupt and Resume messages at the start and end of the interuption. This option is intended to be set on the straightshot/interuptor dialog script.

Restart after interuption will tell the dialog to restart the automatic conversation after the interuption dialog has complete. If not checked, the
dialog will resume with the last active dialog entry. This option is intended to be set on the automatic/interupted script.

Wait time before resume is the amount of time to wait after the interuption and before resuming the auto conversation. The auto off time multiplier applies
to this time. This option is intended to be set on the automatic/interupted script.

Resume topper string is a string added to the beginning of the message when a script is resumed. For example, you could set the string to something like
"Now where was I before I was interupted? Oh ya! " This option is intended to be set on the automatic/interupted script.

Resume Message Add Time is an amount of time to add to the on screen time of the message box when it is resumed. This is meant to compensate for the extra
length of the string from the topper string. In other words, use this to make the first dialog box after being resumed stay on screen longer.
This option is intended to be set on the automatic/interupted script.

AUTO TIMERS are used to control the timing of automatic dialogs. On screen time is the amount of time a dialog box stays on screen, off screen time is
the time between two dialog boxes

Default On / Off screen time. This is the amount of time a dialog will stay on the screen, or off the screen (beetween dialogs), by default. Any dialog
that has an on or off screen time of 0 or less will defer to this default time.

Time on / off screen multiplier. This is multiplied to any on / off screen times. Can be used to make timing adjustments to the whole dialog, rather than
just individual dialog boxes.

Dialog Chain entries. An entry is a single message box. The size of this list is the number of message boxes that will display. Here is a breakdown of a single
dialog chain entry:

Message is the actual text that will display in the text box. You can use the following special tags in the message:
 * "[answer]" added to message text will replace the text in quotes with the last provided answer
 * "[title]" is replaced with the message box title (or character name)

Title is the title of the box, or the name of the character saying the message. If blank, the script will try to pull a name from either the iniator's characterController2D
or the characterController2D attached the same object that this dialog script is attached to.

Said by initiator:  If the dialog is initiated via a DialogRange script, the initiator will be the character that walked up and pressed the
dialog button to start the dialog. if checked, the dialog system will assume the initiator is saying this and will pull any relevant information 
from the initiator.

Send Message start / end. This will send a message to game object specified in this dialog entry at the start and end of the current message. For example,
if the characterController saying this needs to look angry, you could send an "Angry" message at the start and a "Normal" message at the end, and
have relevant functions to respond in the character's animation controller.

GameObject is the gameobject saying this message. If said by initiator is checked, this game object will be replaced by the intiator.

Location top / bottom is the location transform for the message when it is above or below a player. Of both are blank, then we will try to pull the transforms
from the CharacterController2D of the gameObject for this message or the initiator. If none of these work, the message will be displayed in the center of the screen.

Group is the group identifier for this message. This identifier can be used to group together certain messages in the dialog chain. Some dialog types utilize groups,
so see dialog types for more info.

Jump to is the dialog chain entry index to jump to at the end of this message. Use -1 to just go to the next message in the chain

Time on/off screen specifies the amount of time a dialog stays on the screen, or off the screen after this dialog and before the next.

Get Text Input: When checked, the box will have a text field for receiving input

Repeat if empty: This will repeat the current message if player input is empty

Repeat add text: This is a string that will be added to the beginning of a repeated message that was repeated via the repeat if empty option. 
For example, "You didn't answer me! Let's try again. "

Input Type is the input validation type for the Get Text Input field. You can change this, for instance, to only accept numbers

Image Resource: A link to an image resource that will be loaded dynamicallg and displayed in the dialog box.

ANSWERS list will give a menu for the player to select from if more than two answers are provided. Each answer is a string.

Answer Branch is a series of possible reponses and messages to jump to if those reponses are given. An answer in the answer branch can either be
an answer selected by a menu, or an answer typed through a text input. Answer branch options are:

	Answer: The given answer to match with. If this answer is provided, then this answer branch is used to determine what action to take. The answer is a string.
	For instance, if the user were to type "Yes" into a text input or select "Yes" as a menu option, then a the branch with "Yes" specified as the answer will be used.
	You can also use the special word "default" for your branch answer here. Default branch will be selected if no other answer branches are matched.

	Index is the dialog chain entry index that we will jump to if the answer specified in this branch is given.

	Ignore case will match the answers "boogers" with "BoOgErS"

	Add to next message is a string that will be added to the beginning of the next message.

Callbacks are functions that are called at the end of the given message box. Callbacks are passed this current dialog script. An example callback might be:

    public void TestCallbackOne(Dialog d)
    {
        int a;
        if ( Int32.TryParse(d.getAnswer(), out a) )
        {
            a += 500;
            d.setNextText(d.getNextText() + " More like " + a.ToString() + " years old!");
        }
    }

Some useful public functions for callbacks inside of the dialog script include:

    public int getIndex() //Returns index of the current dialog chain entry
    public void setIndex(int i) 
    public string getAnswer() //Returns the last answer provided
    public void setAnswer(string answer)
    public string getNextText() //Returns the message of the next dialog box in the chain. May not function correctly with random types!
    public void setNextText(string message) 
    public void addToNextMessageBeginning(string text) //Adds a string to the beginning of the next message
    public void addToNextMessageEnd(string text) 
    public void addTime(float time) //Adds to the on screen time of the next message
	public void EndConversation()