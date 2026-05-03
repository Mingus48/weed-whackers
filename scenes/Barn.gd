extends Node2D

@export var player: Node2D
@export var tickTimer: Timer
@export var tileMap: TileMapLayer
@export var waterTiles: TileMapLayer
@export var bonusMap: TileMapLayer

var plants = []
var water = []
var growTime = []
var bonus = []
var fruit: PackedScene
#How long it will take for a fully watered plant to become dry * 100
var maxWater := 30000

#The atlas coords at where you can find the plant
var plantIdx = {
	"turnip": Vector2i(5, 0),
	"tomato": Vector2i(5, 2),
	"melon": Vector2i(11, 2),
	"eggplant": Vector2i(5, 3),
	"lemon": Vector2i(11, 3),
	"wheat": Vector2i(5, 5),
	"strawberry": Vector2i(5, 6),
	"potato": Vector2i(5, 7),
	"orange": Vector2i(5, 8),
	"corn": Vector2i(5, 9)
}

#Growth time
#The number is the time in seconds it takes to grow * 100
var growIdx = {
	"turnip": 3000,
	"potato": 6000,
	"wheat": 9000,
	"strawberry": 12000,
	"tomato": 24000,
	"corn": 30000,
	"eggplant": 36000,
	"melon": 48000,
	"lemon": 60000,
	"orange": 60000
}

#For the bonus system
#Everything that BOOSTS(not everything that is boosted by) the key plant is listed
#Everything that DIMINISHES(not everything that is diminished by) the plant is listed in the -plant key
var bonusIdx = {
	"turnip": ["strawberry", "tomato"],
	"tomato": ["turnip", "lemon"],
	"melon": ["corn", "orange"],
	"eggplant": ["potato", "tomato"],
	"lemon": ["strawberry", "tomato"],
	"wheat": ["corn", "turnip"],
	"strawberry": ["tomato", "lemon"],
	"potato": ["corn", "wheat"],
	"orange": ["melon", "eggplant"],
	"corn": ["potato", "melon"],
	"-turnip": ["corn"],
	"-tomato": ["potato", "corn"],
	"-melon": ["potato"],
	"-eggplant": ["strawberry"],
	"-lemon": ["orange"],
	"-wheat": ["tomato"],
	"-strawberry": ["eggplant"],
	"-potato": ["tomato", "melon"],
	"-orange": ["lemon"],
	"-corn": ["tomato"]
}

var buyIdx = {
	"turnip": 10,
	"potato": 25,
	"wheat": 40,
	"strawberry": 60,
	"tomato": 120,
	"corn": 150,
	"eggplant": 200,
	"melon": 300,
	"lemon": 500,
	"orange": 550
}

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	fruit = load("res://scenes/Collectable.tscn")
	var cam: Camera2D = player.get_node("Camera2D")
	cam.limit_left = 0
	cam.limit_right = 416
	cam.limit_top = 0
	cam.limit_bottom = 272
	bonusMap.clear()

	for i in range(8):
		plants.append([])
		water.append([])
		growTime.append([])
		bonus.append([])
		for j in range(10):
			plants[i].append(null)
			water[i].append(0.0)
			growTime[i].append(0)
			bonus[i].append(1.0)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	harvestPlant()

func harvestPlant() -> void:
	if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
		var mousePos: Vector2i = tileMap.local_to_map(tileMap.to_local(get_global_mouse_position()))
		if mousePos.x >= 0 and mousePos.y >= 0 and mousePos.x < plants.size() and mousePos.y < plants[0].size():
			if plants[mousePos.x][mousePos.y] != null and growTime[mousePos.x][mousePos.y] == 0:
				var plant: String = plants[mousePos.x][mousePos.y]
				plants[mousePos.x][mousePos.y] = null
				bonus[mousePos.x][mousePos.y] = 1.0
				if mousePos.x - 1 >= 0:
					reverseBonus(mousePos.x - 1, mousePos.y, plant)
				if mousePos.y - 1 >= 0:
					reverseBonus(mousePos.x, mousePos.y - 1, plant)
				if mousePos.x + 1 < plants.size():
					reverseBonus(mousePos.x + 1, mousePos.y, plant)
				if mousePos.y + 1 < plants[0].size():
					reverseBonus(mousePos.x, mousePos.y + 1, plant)
				tileMap.erase_cell(mousePos)
				spawnFruit(plant, get_local_mouse_position())

func addSeed(plant: String, globalCords: Vector2) -> void:
	var cords: Vector2i = tileMap.local_to_map(tileMap.to_local(globalCords))
	if cords.x < 0 or cords.x >= plants.size() or cords.y < 0 or cords.y >= plants[0].size():
		return
	if plants[cords.x][cords.y] != null:
		return
	var playerMoney: int = int(player.get("money"))
	if playerMoney - buyIdx[plant] < 0:
		return

	#Adds plant and grow times to the 2d array
	plants[cords.x][cords.y] = plant
	growTime[cords.x][cords.y] = growIdx[plant]
	player.set("money", playerMoney - buyIdx[plant])
	var atlasCords: Vector2i = plantIdx[plant]
	#Handles bonuses
	var boosted := 0
	if cords.x - 1 >= 0:
		boosted += plantBonuses(cords.x - 1, cords.y, plant)
	if cords.y - 1 >= 0:
		boosted += plantBonuses(cords.x, cords.y - 1, plant)
	if cords.x + 1 < plants.size():
		boosted += plantBonuses(cords.x + 1, cords.y, plant)
	if cords.y + 1 < plants[0].size():
		boosted += plantBonuses(cords.x, cords.y + 1, plant)
	bonus[cords.x][cords.y] += boosted / 4.0
	#Puts the tile on the map
	tileMap.set_cell(cords, 0, atlasCords)

func waterSpot(globalCords: Vector2, deltaForce: float) -> void:
	var cords: Vector2i = waterTiles.local_to_map(waterTiles.to_local(globalCords))
	if cords.x < 0 or cords.x >= plants.size() or cords.y < 0 or cords.y >= plants[0].size():
		return
	water[cords.x][cords.y] += maxWater / 10.0
	#Amount of seconds it takes to fill ^
	if cords.x - 1 >= 0:
		water[cords.x - 1][cords.y] += maxWater / 10.0 * deltaForce
	if cords.y - 1 >= 0:
		water[cords.x][cords.y - 1] += maxWater / 20.0 * deltaForce
	if cords.x + 1 < water.size():
		water[cords.x + 1][cords.y] += maxWater / 20.0 * deltaForce
	if cords.y + 1 < water[0].size():
		water[cords.x][cords.y + 1] += maxWater / 20.0 * deltaForce
	updateWater()

func plantBonuses(x: int, y: int, plant: String) -> int:
	if plants[x][y] == null:
		return 0
	if bonusIdx[plants[x][y]].has(plant):
		bonus[x][y] += 0.25
	elif bonusIdx["-" + plants[x][y]].has(plant):
		bonus[x][y] -= 0.25
	if bonusIdx[plant].has(plants[x][y]):
		return 1
	elif bonusIdx["-" + plant].has(plants[x][y]):
		return -1
	return 0

func reverseBonus(x: int, y: int, plant: String) -> void:
	if plants[x][y] == null:
		return
	if bonusIdx[plants[x][y]].has(plant):
		bonus[x][y] -= 0.25
	elif bonusIdx["-" + plants[x][y]].has(plant):
		bonus[x][y] += 0.25

func updateWater() -> void:
	#Updates all farmland tiles to have the correct water
	for i in range(water.size()):
		for j in range(water[0].size()):
			if water[i][j] != 0:
				if water[i][j] > maxWater / 2.0:
					waterTiles.set_cell(Vector2i(i, j), 0, Vector2i(8, 8))
				elif water[i][j] > 0:
					waterTiles.set_cell(Vector2i(i, j), 0, Vector2i(7, 8))
				else:
					waterTiles.set_cell(Vector2i(i, j), 0, Vector2i(6, 8))

func updatePlants() -> void:
	#Updates all plant tiles to be in the correct state
	for i in range(growTime.size()):
		for j in range(growTime[0].size()):
			if plants[i][j] != null:
				var atlasCords: Vector2i = plantIdx[plants[i][j]]
				atlasCords.x -= 4 - int(5 * ((float(growTime[i][j]) - 0.001) / float(growIdx[plants[i][j]])))
				if atlasCords.x == 1 or atlasCords.x == 7:
					growTime[i][j] = 0
				tileMap.set_cell(Vector2i(i, j), 0, atlasCords)

func showBonuses(plant: String, globalCords: Vector2) -> void:
	bonusMap.clear()
	var cords: Vector2i = bonusMap.local_to_map(bonusMap.to_local(globalCords))
	if cords.x < 0 or cords.x >= plants.size() or cords.y < 0 or cords.y >= plants[0].size():
		return
	if plants[cords.x][cords.y] != null:
		return
	var boosted := 0
	if cords.x - 1 >= 0:
		boosted += setBonusCell(cords.x - 1, cords.y, plant)
	if cords.y - 1 >= 0:
		boosted += setBonusCell(cords.x, cords.y - 1, plant)
	if cords.x + 1 < plants.size():
		boosted += setBonusCell(cords.x + 1, cords.y, plant)
	if cords.y + 1 < plants[0].size():
		boosted += setBonusCell(cords.x, cords.y + 1, plant)
	if boosted > 0:
		bonusMap.set_cell(cords, 0, Vector2i(0, 0))
	elif boosted < 0:
		bonusMap.set_cell(cords, 0, Vector2i(1, 0))

func setBonusCell(x: int, y: int, plant: String) -> int:
	if plants[x][y] == null:
		return 0
	if bonusIdx[plants[x][y]].has(plant):
		bonusMap.set_cell(Vector2i(x, y), 0, Vector2i(0, 0))
	elif bonusIdx["-" + plants[x][y]].has(plant):
		bonusMap.set_cell(Vector2i(x, y), 0, Vector2i(1, 0))
	if bonusIdx[plant].has(plants[x][y]):
		return 1
	elif bonusIdx["-" + plant].has(plants[x][y]):
		return -1
	return 0

func spawnFruit(plant: String, pos: Vector2) -> void:
	var veggie = fruit.instantiate()
	add_child(veggie)
	veggie.position = pos
	veggie.setTile(plant)

func _on_tick_timer_timeout() -> void:
	for i in range(water.size()):
		for j in range(water[0].size()):
			if water[i][j] != 0:
				water[i][j] -= int(tickTimer.wait_time) * 100
				if water[i][j] < 0:
					water[i][j] = 0

	for i in range(growTime.size()):
		for j in range(growTime[0].size()):
			if growTime[i][j] != 0 and water[i][j] != 0:
				growTime[i][j] -= int(tickTimer.wait_time * 100 * bonus[i][j])
				if growTime[i][j] < 0:
					growTime[i][j] = 0
	updateWater()
	updatePlants()

func clearBonus() -> void:
	bonusMap.clear()
