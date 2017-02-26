using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem {

    Sprite Icon { get; }

    float CheckCooldown();

    void Activate(Player player);
}
