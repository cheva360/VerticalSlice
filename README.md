# GDIM33 Vertical Slice
## Milestone 1 Devlog
1. There isnt much c# in my game as of now, essentially all of the code is in the player visual scripting graph and state machine. The player graph is responsible for movement of the arms according to the joysticks, movement of the body if grabbing, and a magnitude distance restriction for the hands from the shoulders. How it works is that when the hand is overlapping a rock, and a grab button is being held, the corresponding grab bool is set to true. It will run the corresponding grab logic to move the hip rigidbody of the rig in the opposite direction of the joystick or in the direction of the mouse. If both hands are being held with mouse movement, then the direction of movement will be reversed to emulate the pulling motion of the joystick style. 

2. The State Machine updates the players hands when the player is grabbing onto a grabbable surface. Each hand has a trigger collider that detects whether its hitting a collider tagged as rock, and if it is while the corresponding grab button is being held, it will update the rigs hand to look like it is grabbing.

<img width="1762" height="645" alt="image" src="https://github.com/user-attachments/assets/cf9e4f74-aab8-47e9-a44c-433a7deff2a7" />


## Milestone 2 Devlog

Stamina Complicating feature:
- Zelda style visual stamina
  - Make white circle donut
  - Make it radial fill in ui settings
- Dynamic with movement
  - Check velocity of player
  - If above certain threshold deplete stamina
  - If below certain negative y velocity, stop depleting
- Red trail
  - Add another radial fill behind that lags behind the green fill
  - Updates slower lerping to green radial fill but is also clamped
- Replenishing
  - Stamina will slowly replenish if player is below a certain velocity threshold and not grabbing
  - Green bubbles will replenish stamina with an exponential ease-out lerp.

1. Yes the breakdown helped a lot in thinking about the logic of the code. Sometimes ill dive head first without thinking about it first, and itll cost me time because I haven't figured out exactly what I need to do. Laying things out helps out a ton.
2. I specifically call a c# method from my main player graph for the double arm constraints. The math was just getting too complicated and I couldn't get it to work how I wanted to in visual scripting, so I implemented it as a c# method instead.
3. You will be grading the stamina system. It should work how I outlined it in my readme. 


## Milestone 3 Devlog
1. Shader Graphs
There are two main shader graphs in Sandwich Climbers:
- The Stamina Bubble fade shader graph
- The grass triplanar shadar graph

The simplest of the two is the triplanar grass shader graph. It uses the world position of the grass game objects to determine the UV coordinates for the texture. It essentially acts like a tiled texture but in 2d, and makes the texture look seamless all across completely different objects. 
The more complex of the two is the fade shader graph. I already explained it a bit in the last devlog, but ill go over it again. When the player enters a trigger, the script sets the IsFade bool to true and the start amount to time.time. The shader then subtracts the start time from the current time to get the elapsed time, and is divided by the duration float as well as put into a smooth step to create the ease effect. 

Here is the shader graph for the stamina bubble fade effect:

2. Improvements
There are two major improvements in this build over the last.
The first is fixing the major leg bug glitch. Whenever it occured, it would essentially break the entire players limbs and make them fly all over the place. I changed the legs to ignore all collisions and rely on the hip box collider instead.
The second improvement is adding rotation to the player, and a smooth camera following script. The camera script lerps the camera to the player with a ease out curve, and an adjusutable speed. This was created first because the player rotation was also rotating the camera, and then turned into a lerp for smoothness. The player rotation was added by ignoring all the collisions on all the limbs except the hips, allowing the rest of the body and most importantly the spine to move allong with the arm iks. With the added gravity to the legs, and changing to the angles on the character joints, it adds a much more natural ragdoll feel to the game which I believe fits the style of the game. 

3. Gameplay Changes
This could also be considered an improvement, but a main part of playtesting feedback was that the stamina bubbles didn't respawn, meaning that the player could get stuck without enough stamina to get to the next section. As well as this, I lowered the friction a bit on the player so if they run out of stamina near the end, they have the chance to slide all the way down to the start, in true foddian fashion. This overall makes for gameplay more similar to the vision, and adds a better feel to the game. 



## Milestone 4 Devlog
Milestone 4 Devlog goes here.
## Final Devlog
Final Devlog goes here.
## Open-source assets
- Cite any external assets used here!
