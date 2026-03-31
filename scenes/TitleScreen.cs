using Godot;
using System;

public partial class TitleScreen : Node2D
{
	[Export]
	private Node2D creds;
	[Export]
	private Node2D creds2;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		if(Input.IsKeyPressed(Key.Escape) || Input.IsKeyPressed(Key.Space)){
			creds.Hide();
			creds2.Hide();
		}
	}

	private void _on_button_button_down(){
		GetTree().ChangeSceneToFile("res://scenes/Barn.tscn");
	}

	private void _on_button_2_button_down(){
		creds.Show();
	}

	private void _on_button_3_button_down(){
		creds2.Show();
	}
}
