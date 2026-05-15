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
Milestone 3 Devlog goes here.
## Milestone 4 Devlog
Milestone 4 Devlog goes here.
## Final Devlog
Final Devlog goes here.
## Open-source assets
- Cite any external assets used here!
