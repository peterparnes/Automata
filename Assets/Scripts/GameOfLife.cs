using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Frame
{
	public readonly int Width, Height;
	public readonly bool Empty;

	public readonly bool[,] Grid;

	public Frame (int width, int height)
	{
		this.Width = width;
		this.Height = height;

		Grid = new bool[width, height];
	}

	public Frame (Frame previous)
	{
		if (previous == null)
			throw new System.NullReferenceException ("Previous frame cannot be null.");

		Width = previous.Width;
		Height = previous.Height;

		Empty = true;

		Grid = new bool[Width, Height];

		for (int x = 1; x < Width - 1; x++)
		{
			for (int y = 1; y < Height - 1; y++)
			{
				var count = GetNeighborCount (previous, x, y);
				bool value;
				if (previous.Grid[x, y])
					value = !(count < 2 || count > 3);
				else
					value = count == 3;

				if (value)
					Empty = false;

				Grid[x, y] = value;
			}
		}
	}

	private static int GetNeighborCount (Frame frame, int x, int y)
	{
		var count = 0;

		var grid = frame.Grid;

		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (grid[x + i, y + j])
					count++;
			}
		}

		if (grid[x, y])
			count--;

		return count;
	}
}

public class GameOfLife : MonoBehaviour
{
	public enum Placement { Noise, Random, Glider, LightweightSpaceship }

	[Header ("Initialization")]
	public int width = 25;
	public int height = 25;
	public Placement placementMode = GameOfLife.Placement.Noise;
	public float frequency = 0.5f;
	[Range (0f, 1f)]
	public float minimum = 0.5f;

	[Header ("Animation")]
	public float delay = 0.05f;
	[Range (1, 50)]
	public int maxFrames = 25;
	public bool scroll = true;
	public Vector3 spacing = Vector3.one;
	public Mesh mesh;
	
	private Material _topMaterial;
	private int _topMaterialCounter = 0;
	public Material[] topMaterials;
	public Material bodyMaterial;
	
	
	private List<Frame> _frames;
	private Coroutine _generateFramesRoutine;

	private void OnEnable ()
	{
		Initialize ();
		_generateFramesRoutine = StartCoroutine (GenerateFramesRoutine ());
	}

	private void OnDisable ()
	{
		if (_generateFramesRoutine != null)
			StopCoroutine (_generateFramesRoutine);
		_frames = new List<Frame> ();
	}

	private void Initialize ()
	{
		_topMaterial = topMaterials[_topMaterialCounter];
		_topMaterialCounter++;
		if(_topMaterialCounter >= topMaterials.Length) _topMaterialCounter = 0;

		_frames = new List<Frame> ();

		var firstFrame = new Frame (width, height);

		var offset = Random.Range (-1000, 1000);

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				switch (placementMode)
				{
					default:
					case Placement.Noise:
						firstFrame.Grid[x, y] = Mathf.PerlinNoise (x * frequency + offset, y * frequency) > 0.5f;
						break;

					case Placement.Random:
						firstFrame.Grid[x, y] = Random.value > 0.5f;
						break;

					case Placement.Glider:
						firstFrame.Grid[x, y] = 
							(x == 1 && y == 1 + height - 5) ||
							(x == 2 && y == 1 + height - 5) ||
							(x == 3 && y == 1 + height - 5) ||
							(x == 3 && y == 2 + height - 5) ||
							(x == 2 && y == 3 + height - 5);
						break;
					case Placement.LightweightSpaceship:
						firstFrame.Grid[x, y] =
							(x == 1 && y == 1 + height / 2) ||
							(x == 4 && y == 1 + height / 2) ||
							(x == 5 && y == 2 + height / 2) ||
							(x == 5 && y == 3 + height / 2) ||
							(x == 5 && y == 4 + height / 2) ||
							(x == 1 && y == 3 + height / 2) ||
							(x == 2 && y == 4 + height / 2) ||
							(x == 3 && y == 4 + height / 2) ||
							(x == 4 && y == 4 + height / 2);
						break;
				}

			}
		}

		_frames.Add (firstFrame);
	}

	private IEnumerator GenerateFramesRoutine ()
	{
		while (true)
		{
			if (_frames.Count > maxFrames)
			{
				if (scroll)
				{
					while (_frames.Count > maxFrames)
						_frames.RemoveAt (0);
				}
				else
				{
					yield return null;
					continue;
				}
			}

			//frames[0] = new Frame (frames[frames.Count - 1]);
			var newFrame = new Frame (_frames[_frames.Count - 1]);

			if (newFrame.Empty)
			{
				if (_generateFramesRoutine != null)
					StopCoroutine (_generateFramesRoutine);
				yield break;
			}

			_frames.Add (newFrame);


			yield return new WaitForSeconds (delay);
		}
	}

	private void Update ()
	{
		var transform1 = transform;
		var transformMatrix = Matrix4x4.TRS (transform1.position, transform1.rotation, transform1.lossyScale);

		for (int i = 0; i < _frames.Count; i++)
		{
			var material = i == _frames.Count - 1 ? _topMaterial : bodyMaterial;
			for (int x = 0; x < _frames[i].Width; x++)
			{
				for (int y = 0; y < _frames[i].Height; y++)
				{
					if (_frames[i].Grid[x, y] == true)
					{
						var matrix = transformMatrix * Matrix4x4.Translate (new Vector3 (x * spacing.x, i * spacing.y, y * spacing.z));
						Graphics.DrawMesh (mesh, matrix, material, 0);
					}
				}
			}
		}
	}
}