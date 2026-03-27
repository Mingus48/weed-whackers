using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	private AnimationTree animTree;
	[Export]
	private TileMapLayer seedsLayer;
	private int maxSpeed = 75;
	private float acceleration = .4f;
	private float friction = .2f;
	private bool isRunning;
	private Vector2 lastAxis = new Vector2(0, 1);
	private Barn barnScript;
	private int mode = 0;
	private string currentPlant = "turnip";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		barnScript = (Barn)GetParent();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta){
		if(Input.IsKeyPressed(Key.Key1)){
			mode = 1;
		}

		//Handles planting
		if(mode == 1){
			Vector2 offset = -1 * (Position % 16);
			seedsLayer.Position = offset;
			seedsLayer.Position += new Vector2(16, 16);
			Vector2I coords = (Vector2I)((ToLocal(GetGlobalMousePosition()) + offset) / 16);
			seedsLayer.SetCell(coords, 0, barnScript.plantIdx[currentPlant]);
			if(Input.IsMouseButtonPressed(MouseButton.Left)){
				//barnScript.addSeed(currentPlant, GetGlobalMousePosition());
			}
		}

		//Handles movement
		Vector2 axisPowers = Input.GetVector("left", "right", "up", "down");
		if(axisPowers == Vector2.Zero){
			//Friction
			Velocity = Velocity.Lerp(Vector2.Zero, friction);
			//Idle anim
			isRunning = false;
			animTree.Set("parameters/IdleRun/IdleSpace/blend_position", lastAxis);
			animTree.Set("parameters/TimeScale/scale", .75);
		}else{
			//Acceleration
			Velocity = Velocity.Lerp(axisPowers * maxSpeed, acceleration);
			//Move anim
			isRunning = true;
			animTree.Set("parameters/IdleRun/RunSpace/blend_position", axisPowers);
			animTree.Set("parameters/TimeScale/scale", 1.5);

			lastAxis = axisPowers;
		}
		MoveAndSlide();
	}
}
