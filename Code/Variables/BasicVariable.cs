using System;

namespace Sandbox.Variables;

// Since I can't auto generate the Variable game resources, I'll just have to do it manually... 
// So this is for the basic types, I'll have to do this for all the other types as well
// Using internal because, user should use the generic version of this class. Those are only for the Resource system.
// Maybe there is a better way to do this, but I don't know it yet.


[GameResource( "Float Variable", "float", "A float variable.", Category = "Variables", Icon = "trip_origin" )]
internal class FloatVariable : Variable<float>
{
}

[GameResource("String Variable", "string", "A string variable.", Category = "Variables", Icon = "trip_origin")]
internal class StringVariable : Variable<String>
{
}

[GameResource("Int Variable", "integer", "A int variable.", Category = "Variables", Icon = "trip_origin")]
internal class IntVariable : Variable<int>
{
}

[GameResource("Bool Variable", "bool", "A bool variable.", Category = "Variables", Icon = "trip_origin")]
internal class BoolVariable : Variable<bool>
{
}

[GameResource("Vector2 Variable", "vectwo", "A Vector2 variable.", Category = "Variables", Icon = "trip_origin")]
internal class Vector2Variable : Variable<Vector2>
{
}

[GameResource("Vector3 Variable", "vecthree", "A Vector3 variable.", Category = "Variables", Icon = "trip_origin")]
internal class Vector3Variable : Variable<Vector3> 
{
}

[GameResource("Transform Variable", "trs", "A Transform variable.", Category = "Variables", Icon = "trip_origin")]
internal class TransformVariable : Variable<Transform>
{
}

[GameResource("Rotation Variable", "rot", "A Quaternion variable.", Category = "Variables", Icon = "trip_origin")]
internal class RotationVariable : Variable<Rotation>
{
}





