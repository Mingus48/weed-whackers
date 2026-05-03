extends CharacterBody2D

@export var speech: Sprite2D
@export var heart: Sprite2D
@export var fruit: Sprite2D
var anim: AnimatedSprite2D
var atDoor := true
var desire := ""
var barnScript
var veggies = ["turnip", "tomato", "melon", "eggplant", "lemon", "wheat", "strawberry", "potato", "orange", "corn"]

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	barnScript = get_parent()
	anim = get_node("AnimatedSprite2D")

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(delta: float) -> void:
	if atDoor:
		anim.play("idle")
		speech.visible = true
		fruit.visible = true
		var rng := RandomNumberGenerator.new()
		rng.randomize()
		desire = veggies[rng.randi_range(0, veggies.size() - 1)]
		fruit.frame_coords = Vector2i(barnScript.plantIdx[desire].x - 5, barnScript.plantIdx[desire].y)
		atDoor = false
