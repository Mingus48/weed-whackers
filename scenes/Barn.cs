using Godot;
using System;
using System.Collections.Generic;

public partial class Barn : Node2D
{
	[Export]
	CharacterBody2D player;
	[Export]
	TileMapLayer tileMap;

	string[,] plants = new string[8, 10];
	int[,] stage = new int[8, 10];
	int[,] water = new int[8, 10];
	Vector2I firstPlant = new Vector2I(15, 4);
	Dictionary<string, Vector2I> plantIdx = new Dictionary<string, Vector2I>(){
		{"turnip", new Vector2I(5, 0)},
		{"tomato", new Vector2I(5, 2)}
	};

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
		addSeed("turnip", firstPlant);
		GD.Print(plants[0, 0]);
	}

	private void addSeed(string plant, Vector2I cords){
		plants[cords.X - firstPlant.X, cords.Y - firstPlant.Y] = plant;
		Vector2I atlasCords = plantIdx[plant];
		tileMap.SetCell(cords, 0, atlasCords);
	}
}
