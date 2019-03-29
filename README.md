# prx_fishtank

Self assembly of peroxiredoxin  

Peroxiredoxins (Prxs, EC 1.11.1.15; pronounced per-oxy- red-ox-in) are a ubiquitous family of highly abundant antioxidant enzymes that protect all cells from free radical damage. They have an intriguing repertoire of self-assembled structures which helps modulate their activity inside cells and also provides an exciting scaffolding opportunity in vitro for nanotechnology applications.  

This visualisation shows the assembly of monomers into dimers, dimers into rings, and rings into nanotubes. You can teleport with either touchpad, and you can adjust the pH by grabbing the handle above one of your controllers with the opposite controller (using the trigger on the underside) and moving it left or right. The pH controls the degree of assembly. You can pick up a monomer, dimer or ring, and you can break it into its smaller components by pulling it apart with two hands or shaking it while holding it (if it's a dimer or a ring).  

You can re-order rings, or re-pair monomers by simply moving them closer - as monomers/dimers/rings will aim to pair with the closest object of the same type.  

This simulation was developed by Nick Young, Bianca Haux, and Warrick Corfe-Tan at the Centre for eResearch at the University of Auckland in collaboration with David Doak, Norwich University of the Arts, Juliet Gerrard, and Michael Barnett from the University of Auckland School of Biological Sciences.  

## Instructions

### Configuation Options 

There are some settings for the game that may be changed outside of the game. To change configurations go to [Build folder]/SteamVR_Data/StreamingAssets/config.xml and change the value to "true" or "false" (case sensitive) depending on your selection.

#### Configuation Options

| option | true | false |
| --- | --- | --- |
| touchpad-menu-cycling | When true, you can you the touchpad to swipe and change menus while the top part of the touchpad can be used to teleport | When disabled the entire touchpad acts as a teleport button. |
| button-hold-overloads | When true, holding down the top menu button for 2 seconds will hide all menus.  This also changes menu cycling to occur on the release of the button rather than the downwards press. | No menu hiding functionality within the menu button, menu cycling occurs on downwards press. |
| transition-materials | When true the rings will gradually fade to a transparent material when they form a stack. Monomers, Dimers, and Rings will also look slightly different due to using different shaders. | If false the rings will immediately transform to transparent when a stack is formed. |
| grab-split-stack | When true, grabbing a stack with two hands will split in the position of the second hand to grab the stack and attach the new separated stack to the new hand. | When false, removing a ring from a stack will only displace the ring directly grabbed and the stack will remain attached to the original hand. |
| smooth-stack-rotation | When true, ring stacks rotation will be scaled based on the distance. The further they are from the ring they are partnered to, the less magnitude will be used for the rotation. | If false, the ring stacks will rotate at a constant speed. |
