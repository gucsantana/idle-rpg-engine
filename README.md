# idle-rpg-engine
Skeleton of an idle RPG engine, tentatively called Tower of Wishes

Built to practice creating an RPG-like stats and equipment progression engine, attached to a self-sustained combat.
Basic first version is around 80-90% done; mostly missing graphics and control menus.

BUILD: Windows 32-bit
	Unpack the folder, and execute the "Tower of Idle.exe" file
	
CONTROLS: mouse left click for all actions
	Alt+F4 to quit the game (proper menus not implemented yet)
	
	Character fights enemies automatically, gains experience (and eventually levels and stats), and drops items
	Items can be found in the "Loot Box" button; click the chests to open them
	Obtained items can be equipped by clicking the slots on the Gear menu on the left; there is a limit of 10 items before the box is full
	Player cannot change equipment during a fight; any changes are queued for the end of the fight
	Player can change areas by clicking the "Advance" and "Return" buttons on the left and right edges of the screen
	Player cannot change areas during combat; change is queued for the end of the fight
	There is a minimum level to access each area, and a maximum level that can be reached on each area
	
BIGGEST CHALLENGE: getting the combat damage calculations correctly, and having them consider the stats and perks of equipment.

BIGGEST EFFORT: implementing the armory and bestiary systems (with localization support for multiple languages) took some time and planning,
	and resulted in a system that is reasonably optimized, easy to understand, and simple to add more objects to.
