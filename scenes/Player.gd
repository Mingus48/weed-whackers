extends CharacterBody2D

@export var animTree: AnimationTree
@export var seedsLayer: TileMapLayer
@export var inventSelect: Sprite2D
@export var label: Label
@export var moneyTxt: Label
@export var decrypt: Label

var money := 50

var maxSpeed := 75
var acceleration := 0.4
var friction := 0.2
var isRunning := false
var isWatering := false
var lastAxis := Vector2(0, 1)
var barnScript
var isFarming := false
var currentPlant := "turnip"
var previousPlant := "turnip"
var lastSeedPos := Vector2i.ZERO
var firstInventPos := Vector2(350, 570)
var inventPos := -1
var veggies = ["turnip", "tomato", "melon", "eggplant", "lemon", "wheat", "strawberry", "potato", "orange", "corn"]
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
var sellIdx = {
	"turnip": 20,
	"potato": 55,
	"wheat": 90,
	"strawberry": 140,
	"tomato": 300,
	"corn": 375,
	"eggplant": 550,
	"melon": 900,
	"lemon": 1500,
	"orange": 1650
}
var descriptionIdx = {
	"turnip": "Fast-growing and hardy.\n• Best Neighbors: Strawberry, Tomato\n• Avoid: Corn\n• Why (+): Acts as a 'trap crop' for pests and loosens soil for strawberries.\n• Why (-): Corn is a heavy feeder that will starve the turnip of nitrogen.",
	"potato": "Deep-rooted tubers.\n• Best Neighbors: Corn, Wheat\n• Avoid: Tomato, Melon\n• Why (+): Benefits from corn's shade and the nitrogen wheat leaves in the soil.\n• Why (-): Shares blight diseases with tomatoes and competes for space with melons.",
	"wheat": "A gold-standard staple.\n• Best Neighbors: Corn, Turnip\n• Avoid: Tomato\n• Why (+): Neighbors help break up heavy soil, allowing wheat's shallow roots to spread.\n• Why (-): Can attract cereal aphids which are also harmful to tomato fruit.",
	"strawberry": "A delicate ground-cover.\n• Best Neighbors: Tomato, Lemon\n• Avoid: Eggplant\n• Why (+): Flourishes in citrus shade and the pest-deterring scent of tomato leaves.\n• Why (-): Both are highly susceptible to Verticillium wilt, a shared soil fungus.",
	"tomato": "The garden's heavy-hitter.\n• Best Neighbors: Turnip, Lemon\n• Avoid: Potato, Corn\n• Why (+): Turnips lure aphids away while trees provide a windbreak for heavy fruit.\n• Why (-): Potatoes spread blight, and corn attracts the shared 'Tomato Fruitworm' pest.",
	"corn": "A natural trellis.\n• Best Neighbors: Potato, Melon\n• Avoid: Tomato\n• Why (+): Provides cooling shade for melons and shares deep-soil minerals with potatoes.\n• Why (-): Both are high-nitrogen consumers; they will stunt each other's growth.",
	"eggplant": "A sun-loving nightshade.\n• Best Neighbors: Potato, Tomato\n• Avoid: Strawberry\n• Why (+): Best grown near cousins that share the same nutrient and irrigation needs.\n• Why (-): Strawberries can harbor soil-borne pathogens that kill eggplant roots.",
	"melon": "A thirsty, sprawling vine.\n• Best Neighbors: Corn, Orange\n• Avoid: Potato\n• Why (+): Uses corn stalks for shade to prevent sun-scorch and likes citrus-enriched soil.\n• Why (-): Potato tubers and melon roots fight for the same horizontal soil space.",
	"lemon": "A vibrant citrus tree.\n• Best Neighbors: Strawberry, Tomato\n• Avoid: Orange\n• Why (+): Its canopy creates a cool micro-climate that prevents ground-crops from wilting.\n• Why (-): Closely related trees compete too aggressively for the same trace minerals.",
	"orange": "The king of the orchard.\n• Best Neighbors: Melon, Eggplant\n• Avoid: Lemon\n• Why (+): Creates a high-moisture 'dome' that keeps the soil perfect for heavy feeders.\n• Why (-): Planting too close to other citrus leads to nutrient deficiencies and root rot."
};

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	barnScript = get_parent()

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(delta: float) -> void:
	#Handles switching to water
	if Input.is_action_just_pressed("water"):
		if isWatering:
			isWatering = false
		else:
			isWatering = true
			isFarming = false

	#Handles Inventory
	inventory()

	#Handles planting and watering
	plantWater(delta)

	#Handles movement
	move()

	moneyTxt.text = "Money: $" + str(money)
	move_and_slide()

func move() -> void:
	var axisPowers: Vector2 = Input.get_vector("left", "right", "up", "down")
	if axisPowers == Vector2.ZERO:
		#Friction
		velocity = velocity.lerp(Vector2.ZERO, friction)
		#Idle anim
		isRunning = false
		if not isWatering:
			animTree.set("parameters/IdleRun/IdleSpace/blend_position", lastAxis)
			animTree.set("parameters/TimeScale/scale", 0.75)
		else:
			animTree.set("parameters/IdleRun/WaterSpace/blend_position", lastAxis)
			animTree.set("parameters/TimeScale/scale", 0.75)
	else:
		#Acceleration
		velocity = velocity.lerp(axisPowers * maxSpeed, acceleration)
		#Move anim
		isRunning = true
		animTree.set("parameters/IdleRun/RunSpace/blend_position", axisPowers)
		animTree.set("parameters/TimeScale/scale", 1.5)
		lastAxis = axisPowers

func inventory() -> void:
	if isWatering:
		inventSelect.position = firstInventPos
		if not inventSelect.visible:
			inventSelect.show()
		label.text = "\nwater"
		decrypt.text = ""
		label.pivot_offset = Vector2(label.size.x / 2.0, 0)
		inventPos = -1
	if Input.is_action_just_released("scrollUp"):
		inventPos += 1
		if inventPos > 9:
			inventPos = 0
		isFarming = true
	if Input.is_action_just_released("scrollDown"):
		inventPos -= 1
		if inventPos < 0:
			inventPos = 9
		isFarming = true
	checkKeyPressed(0)
	checkKeyPressed(1)
	checkKeyPressed(2)
	checkKeyPressed(3)
	checkKeyPressed(4)
	checkKeyPressed(5)
	checkKeyPressed(6)
	checkKeyPressed(7)
	checkKeyPressed(8)
	checkKeyPressed(9)
	if isFarming:
		currentPlant = veggies[inventPos]
		inventSelect.position = Vector2(firstInventPos.x + 70 + (inventPos * 42), firstInventPos.y)
		if not inventSelect.visible:
			inventSelect.show()
			inventSelect.position = firstInventPos
		label.text = currentPlant + "\nbuy $" + str(buyIdx[currentPlant]) + " | sell $" + str(sellIdx[currentPlant])
		label.pivot_offset = Vector2(label.size.x / 2.0, 0)
		decrypt.text = descriptionIdx[currentPlant]
		isWatering = false
	elif not isWatering:
		inventSelect.hide()
		label.text = ""
		decrypt.text = ""

func plantWater(delta: float) -> void:
	if isFarming:
		var offset: Vector2 = -1 * position.posmodv(Vector2(16, 16))
		offset += Vector2(16, 16)
		seedsLayer.position = offset
		var mousePos: Vector2 = seedsLayer.to_local(get_global_mouse_position())
		var coords: Vector2i = seedsLayer.local_to_map(mousePos)
		if coords != lastSeedPos or currentPlant != previousPlant:
			seedsLayer.erase_cell(lastSeedPos)
			seedsLayer.set_cell(coords, 0, barnScript.plantIdx[currentPlant])
			lastSeedPos = coords
			previousPlant = currentPlant
			barnScript.showBonuses(currentPlant, get_global_mouse_position())
		if Input.is_action_just_pressed("leftClick"):
			barnScript.addSeed(currentPlant, get_global_mouse_position())
		previousPlant = currentPlant
	else:
		seedsLayer.clear()
		barnScript.clearBonus()
		lastSeedPos = Vector2i(-1, -1)
	if isWatering and not isRunning:
		barnScript.waterSpot(Vector2(position.x, position.y + 8), delta)

func checkKeyPressed(x: int) -> void:
	if Input.is_action_just_pressed(str(x)):
		if x == 0:
			x = 10
		if inventPos != x - 1:
			inventPos = x - 1
			isFarming = true
		else:
			isFarming = false
			inventPos = x - 2
			if inventPos < 0:
				inventPos += 10

func getMoney(plant: String) -> void:
	#GD.Print(plant);
	money += sellIdx[plant]
