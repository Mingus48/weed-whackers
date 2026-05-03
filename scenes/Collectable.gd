extends RigidBody2D

@export var anim: AnimationPlayer
@export var pic: TileMapLayer

var plant := ""
var barnScript
var trackPlayer := false
var player
var speed := 100
var startingY := 0.0
var oneDance := true

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	barnScript = get_parent()

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(delta: float) -> void:
	if position.y >= startingY:
		gravity_scale = 0
		linear_velocity = Vector2(linear_velocity.x, 0)
		linear_damp = 5
	if trackPlayer:
		linear_velocity = to_local(player.position).normalized() * speed
		if (position - player.position).length() < 16:
			if oneDance:
				player.getMoney(plant)
				oneDance = false
			anim.play("delete")

func setTile(plantName: String) -> void:
	plant = plantName
	var atlasCords: Vector2i = barnScript.plantIdx[plant]
	atlasCords.x -= 5
	pic.set_cell(Vector2i.ZERO, 0, atlasCords)
	var rng := RandomNumberGenerator.new()
	rng.randomize()
	var rng1 := rng.randi_range(0, 29)
	var rng2 := rng.randi_range(0, 1)
	var theta := 0
	if rng2 == 0:
		theta = 30 + rng1
	else:
		theta -= 30 - rng1
	var tanVal := tan(deg_to_rad(float(theta)))
	startingY = position.y
	position = Vector2(position.x, position.y - 1)
	linear_velocity = Vector2(tanVal, -1).normalized() * 60

func _on_area_2d_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		trackPlayer = true
		player = body
