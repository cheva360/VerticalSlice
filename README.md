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

Here is the shader graph for the stamina bubble fade effect: <img width="2469" height="1249" alt="image" src="https://github.com/user-attachments/assets/bb15c486-e261-49b7-99fd-61f96094289b" />


2. Improvements

There are two major improvements in this build over the last.
The first is fixing the major leg bug glitch. Whenever it occured, it would essentially break the entire players limbs and make them fly all over the place. I changed the legs to ignore all collisions and rely on the hip box collider instead.
The second improvement is adding rotation to the player, and a smooth camera following script. The camera script lerps the camera to the player with a ease out curve, and an adjusutable speed. This was created first because the player rotation was also rotating the camera, and then turned into a lerp for smoothness. The player rotation was added by ignoring all the collisions on all the limbs except the hips, allowing the rest of the body and most importantly the spine to move allong with the arm iks. With the added gravity to the legs, and changing to the angles on the character joints, it adds a much more natural ragdoll feel to the game which I believe fits the style of the game. 

3. Gameplay Changes

This could also be considered an improvement, but a main part of playtesting feedback was that the stamina bubbles didn't respawn, meaning that the player could get stuck without enough stamina to get to the next section. As well as this, I lowered the friction a bit on the player so if they run out of stamina near the end, they have the chance to slide all the way down to the start, in true Foddian fashion. This overall makes for gameplay more similar to the vision, and adds a better feel to the game. 


## Final Devlog
1. The core gameplay loop of Sandwich Climbers is very Foddian in nature. The player climbs using a physics-based movement system in which you independently drive each arm with the joysticks or mouse, and use them to pull yourself up. You must maintain your stamina either by regaining stamina with the bubbles, or through efficient movement to get to the next resting point. These main mechanics are what would also drive the full game, and the vertical slice is meant to illustrate just a small level section of what would be a huge mountain. 

<img width="1013" height="367" alt="image" src="https://github.com/user-attachments/assets/a02501ee-3815-43f1-89b0-862a01719e67" />

2. Shown in this shader graph screenshot, the vignette is driven directly by the player's stamina. As the player's stamina depletes under a certain percent threshold, the vignette slowly starts to increase in intensity. The vignette is controlled by the global volume in the scene, which is accessed in the StaminaBar script via a serialized field variable. The intensity is directly controlled via the script, going from 0 to 0.2 as the player's stamina goes below a low percentage of the total stamina, as I wanted the vignette to generally be a more gradual effect that made the game a bit more tense in the situation that your stamina is really low, and you are about to lose grip. 

3. I broke down Sandwich Climber mostly into tasks, and I also broke down those tasks further. This, for the most part, worked during development, but I found that the major system breakdowns at the start of the quarter were also vital in helping me understand the scope of the project, as well as how I wanted to implement the mechanics. I think the main thing I would do differently is generally just write a bit more in the breakdowns. I did one big breakdown at the very start, but a lot of things and ideas changed all throughout the development of the vertical slice, so keeping up documentation and the brainstorming of the game is pretty important. I think the main thing that went well, or in general my style of working, was that I like to have a very good mental image of how the mechanics will feel to play, or how it'll look, and I think I executed the game pretty accurately to the vision of the game I had in mind. It was also through playing a lot of games and finding a lot of references that I was able to write and formulate a clear vision of the game that really helped with keeping me on task and knowing exactly what I wanted out of the game. In the future, of course, when I work in teams, but even for myself, it's important that I write down that vision. 

## Open-source assets
Raw Grab SFX:

Sound Effect by <a href="https://pixabay.com/users/spinopel-46570060/?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=381926">Spin Opel</a> from <a href="https://pixabay.com//?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=381926">Pixabay</a>

Sound Effect by <a href="https://pixabay.com/users/freesound_community-46691455/?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=91931">freesound_community</a> from <a href="https://pixabay.com/sound-effects//?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=91931">Pixabay</a>

"Sandwich (hand-painted)" (https://skfb.ly/oOZtv) by adamaysils is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

"Picnic Table" (https://skfb.ly/6WHWB) by Plat is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
