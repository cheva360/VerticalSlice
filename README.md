# GDIM33 Vertical Slice
## Milestone 1 Devlog
1. There isnt much c# in my game as of now, essentially all of the code is in the player visual scripting graph and state machine. The player graph is responsible for movement of the arms according to the joysticks, movement of the body if grabbing, and a magnitude distance restriction for the hands from the shoulders. How it works is that when the hand is overlapping a rock, and a grab button is being held, the corresponding grab bool is set to true. It will run the corresponding grab logic to move the hip rigidbody of the rig in the opposite direction of the joystick or in the direction of the mouse. If both hands are being held with mouse movement, then the direction of movement will be reversed to emulate the pulling motion of the joystick style. 

2. The State Machine updates the players hands when the player is grabbing onto a grabbable surface. Each hand has a trigger collider that detects whether its hitting a collider tagged as rock, and if it is while the corresponding grab button is being held, it will update the rigs hand to look like it is grabbing.



## Milestone 2 Devlog
Milestone 2 Devlog goes here.
## Milestone 3 Devlog
Milestone 3 Devlog goes here.
## Milestone 4 Devlog
Milestone 4 Devlog goes here.
## Final Devlog
Final Devlog goes here.
## Open-source assets
- Cite any external assets used here!
