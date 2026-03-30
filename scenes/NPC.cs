using Godot;
using System;

public partial class NPC : CharacterBody2D{
	[Export]
	private Sprite2D speech;
	[Export]
	private Sprite2D heart;
	[Export]
	private Sprite2D fruit;
	private bool atDoor = true;
	private string desire;
	private Barn barnScript;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		barnScript = (Barn)GetParent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		if(atDoor){
			speech.Visible = true;
			fruit.FrameCoords = barnScript.plantIdx[desire];
		}
	}
}
