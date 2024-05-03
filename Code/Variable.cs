﻿namespace Sandbox;

public class Variable<T> : GameResource
{
	[Property]
	public virtual T? Value { get; set; } = default;
}

