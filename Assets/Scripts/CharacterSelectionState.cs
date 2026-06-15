using Fusion;

public class CharacterSelectionState : NetworkBehaviour
{
    [Networked] public int Character0 { get; set; } = -1;
    [Networked] public int Character1 { get; set; } = -1;
    [Networked] public int Character2 { get; set; } = -1;
    [Networked] public int Character3 { get; set; } = -1;
    [Networked] public int Character4 { get; set; } = -1;
    [Networked] public int Character5 { get; set; } = -1;
    [Networked] public int Character6 { get; set; } = -1;
    [Networked] public int Character7 { get; set; } = -1;
    [Networked] public int Character8 { get; set; } = -1;
    [Networked] public int Character9 { get; set; } = -1;
    
    [Networked] public int Ready0 { get; set; }
    [Networked] public int Ready1 { get; set; }
    [Networked] public int Ready2 { get; set; }
    [Networked] public int Ready3 { get; set; }
    [Networked] public int Ready4 { get; set; }
    [Networked] public int Ready5 { get; set; }
    [Networked] public int Ready6 { get; set; }
    [Networked] public int Ready7 { get; set; }
    [Networked] public int Ready8 { get; set; }
    [Networked] public int Ready9 { get; set; }
}