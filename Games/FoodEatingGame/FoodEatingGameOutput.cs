﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGame.Games.FoodEatingGame
{
    class FoodEatingGameOutput
    {
        public Direction DirectionToMoveIn;

        public FoodEatingGameOutput(Direction d)
        {
            DirectionToMoveIn = d;
        }
    }
}
