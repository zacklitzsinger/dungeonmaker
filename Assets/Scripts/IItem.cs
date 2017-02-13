using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem {

    float CheckCooldown();

    void Activate(Player player);
}
