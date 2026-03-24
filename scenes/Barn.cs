using Godot;
using System;
using System.Collections.Generic;

public partial class Barn : Node2D
{
	[Export]
	CharacterBody2D player;
	[Export]
	Timer waterTimer;
	[Export]
	TileMapLayer tileMap;
	[Export]
	TileMapLayer waterTiles;

	string[,] plants = new string[8, 10];
	int[,] stage = new int[8, 10];
	int[,] water = new int[8, 10];
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
		addSeed("turnip", new Vector2I(0, 0));
		Vector2 localPos = GetGlobalMousePosition() - tileMap.Position;
		localPos /= 16;
		if(localPos.X < 8 && localPos.Y < 10 && localPos.X > 0 && localPos.Y > 0){
			water[(int)localPos.X, (int)localPos.Y] = 30000;
			updateWater();
		}
		//GD.Print(GetGlobalMousePosition());
		//GD.Print(localPos);
		GD.Print(water[0,0]);
	}

	private void addSeed(string plant, Vector2I cords){
		plants[cords.X, cords.Y] = plant;
		Vector2I atlasCords = plantIdx[plant];
		tileMap.SetCell(cords, 0, atlasCords);
	}

	private void updateWater(){
		for(int i = 0; i < water.GetLength(0); i ++){
			for(int j = 0; j < water.GetLength(1); j ++){
				if(water[i, j] > 20000){
					waterTiles.SetCell(new Vector2I(i, j), 0, new Vector2I(8, 8));
				}else if(water[i, j] > 10000){
					waterTiles.SetCell(new Vector2I(i, j), 0, new Vector2I(7, 8));
				}else{
					waterTiles.SetCell(new Vector2I(i, j), 0, new Vector2I(6, 8));
				}
			}
		}
	}

	private void _on_water_timer_timeout(){
		GD.Print("update water");
		for(int i = 0; i < water.GetLength(0); i ++){
			for(int j = 0; j < water.GetLength(1); j ++){
				if(water[i, j] != 0){
					water[i, j] -= (int)waterTimer.WaitTime * 100;
					if(water[i, j] < 0){
						water[i, j] = 0;
					}
				}
			}
		}
		updateWater();
	}
}
