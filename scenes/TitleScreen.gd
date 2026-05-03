extends Node2D

@export var creds: Node2D
@export var creds2: Node2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if Input.is_key_pressed(KEY_ESCAPE) or Input.is_key_pressed(KEY_SPACE):
		creds.hide()
		creds2.hide()

func _on_button_button_down() -> void:
	get_tree().change_scene_to_file("res://scenes/Barn.tscn")

func _on_button_2_button_down() -> void:
	creds.show()

func _on_button_3_button_down() -> void:
	creds2.show()
