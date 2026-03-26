using Godot;
using System;
using System.Collections.Generic;

public partial class Barn : Node2D
{
	[Export]
	CharacterBody2D player;
	[Export]
	Timer tickTimer;
	[Export]
	TileMapLayer tileMap;
	[Export]
	TileMapLayer waterTiles;

	string[,] plants = new string[8, 10];
	int[,] stage = new int[8, 10];
	int[,] water = new int[8, 10];
	int[,] growTime = new int[8, 10];
	int[,] bonus = new int[8, 10];
	Dictionary<string, Vector2I> plantIdx = new Dictionary<string, Vector2I>(){
		{"turnip", new Vector2I(5, 0)},
		{"tomato", new Vector2I(5, 2)}
	};
	Dictionary<string, int> growIdx = new Dictionary<string, int>(){
		{"turnip", 100},
		{"tomato", 200}
	};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		Camera2D cam = player.GetNode<Camera2D>("Camera2D");
		cam.LimitLeft = 0;
		cam.LimitRight = 416;
		cam.LimitTop = 0;
		cam.LimitBottom = 272;
		addSeed("turnip", new Vector2I(0, 0));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		//Current watering script(will shift to player)
		Vector2 localPos = GetGlobalMousePosition() - tileMap.Position;
		localPos /= 16;
		if(localPos.X < 8 && localPos.Y < 10 && localPos.X > 0 && localPos.Y > 0){
			water[(int)localPos.X, (int)localPos.Y] = 30000;
			updateWater();
		}
	}

	private void addSeed(string plant, Vector2I cords){
		plants[cords.X, cords.Y] = plant;
		growTime[cords.X, cords.Y] = growIdx[plant];
		Vector2I atlasCords = plantIdx[plant];
		tileMap.SetCell(cords, 0, atlasCords);
	}

	private void updateWater(){
		//Updates all farmland tiles to have the correct water
		for(int i = 0; i < water.GetLength(0); i ++){
			for(int j = 0; j < water.GetLength(1); j ++){
				if(water[i,j] != 0){
					if(water[i, j] > 20000){
						waterTiles.SetCell(new Vector2I(i, j), 0, new Vector2I(8, 8));
						if(bonus[i, j] != 2){
							bonus[i, j] = 2;
						}
					}else if(water[i, j] > 10000){
						waterTiles.SetCell(new Vector2I(i, j), 0, new Vector2I(7, 8));
						if(bonus[i, j] != 1){
							bonus[i, j] = 1;
						}
					}else{
						waterTiles.SetCell(new Vector2I(i, j), 0, new Vector2I(6, 8));
						if(bonus[i, j] != 0){
							bonus[i, j] = 0;
						}
					}
				}
			}
		}
	}

	private void updatePlants(){
		//Updates all plant tiles to be in the correct state
		for(int i = 0; i < growTime.GetLength(0); i ++){
			for(int j = 0; j < growTime.GetLength(1); j ++){
				if(plants[i, j] != null){
					Vector2I atlasCords = plantIdx[plants[i, j]];
					atlasCords.X -= 4 - (int)(5 * ((growTime[i, j] - .001)/growIdx[plants[i, j]]));
					GD.Print(atlasCords);
					GD.Print((int)(5 * ((float)growTime[i, j]/growIdx[plants[i, j]])));
					tileMap.SetCell(new Vector2I(i, j), 0, atlasCords);
				}
			}
		}
		GD.Print(growTime[0, 0]);
	}

	private void _on_tick_timer_timeout(){
		GD.Print("update water");
		for(int i = 0; i < water.GetLength(0); i ++){
			for(int j = 0; j < water.GetLength(1); j ++){
				if(water[i, j] != 0){
					water[i, j] -= (int)tickTimer.WaitTime * 100;
					if(water[i, j] < 0){
						water[i, j] = 0;
					}
				}
			}
		}

		for(int i = 0; i < growTime.GetLength(0); i ++){
			for(int j = 0; j < growTime.GetLength(1); j ++){
				if(growTime[i, j] != 0){
					growTime[i, j] -= (int)tickTimer.WaitTime * 10 * bonus[i, j];
					if(growTime[i, j] < 0){
						growTime[i, j] = 0;
					}
				}
			}
		}
		updateWater();
		updatePlants();
	}
}
