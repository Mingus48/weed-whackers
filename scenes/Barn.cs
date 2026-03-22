using Godot;
using System;

public partial class Barn : Node2D
{
	[Export]
	CharacterBody2D player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		Camera2D cam = player.GetNode<Camera2D>("Camera2D");
		cam.LimitLeft = 0;
		cam.LimitRight = 416;
		cam.LimitTop = 0;
		cam.LimitBottom = 272;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
	}
}
